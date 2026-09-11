using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MedAdvice.Areas.Identity.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace MedAdvice.Tests.Infrastructure
{
    /// Wires the pieces of MVC that a directly-constructed controller still needs:
    /// the signed-in principal, TempData, and session.
    public static class TestContext
    {
        public static void Prepare(Controller controller, ApplicationUser signedInAs = null, ISession session = null)
        {
            // RedirectToAction resolves IUrlHelperFactory from RequestServices, so a
            // directly-constructed controller still needs it registered.
            ServiceCollection services = new ServiceCollection();
            services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();

            DefaultHttpContext httpContext = new DefaultHttpContext
            {
                RequestServices = services.BuildServiceProvider()
            };

            if (signedInAs != null)
            {
                httpContext.User = PrincipalFor(signedInAs);
            }

            if (session != null)
            {
                httpContext.Session = session;
            }

            // A bare ControllerContext is not enough: UrlHelper dereferences RouteData and
            // ActionDescriptor, so RedirectToAction throws without them.
            controller.ControllerContext = new ControllerContext(
                new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor()));
            controller.TempData = new TempDataDictionary(httpContext, new NullTempDataProvider());
        }

        /// Identity resolves the current user from the NameIdentifier claim, so that is what
        /// UserManager.GetUserAsync reads.
        public static ClaimsPrincipal PrincipalFor(ApplicationUser user)
        {
            ClaimsIdentity identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName)
            }, "TestAuth");

            return new ClaimsPrincipal(identity);
        }

        public static void AddModelError(Controller controller, string key, string message)
        {
            controller.ModelState.AddModelError(key, message);
        }

        sealed class NullTempDataProvider : ITempDataProvider
        {
            public IDictionary<string, object> LoadTempData(HttpContext context)
            {
                return new Dictionary<string, object>();
            }

            public void SaveTempData(HttpContext context, IDictionary<string, object> values)
            {
            }
        }
    }

    /// Minimal ISession so actions that stash ids between requests can be exercised.
    public sealed class FakeSession : ISession
    {
        readonly Dictionary<string, byte[]> store = new Dictionary<string, byte[]>();

        public bool IsAvailable
        {
            get { return true; }
        }

        public string Id
        {
            get { return "test-session"; }
        }

        public IEnumerable<string> Keys
        {
            get { return store.Keys; }
        }

        public void Clear()
        {
            store.Clear();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            store.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            store[key] = value;
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            return store.TryGetValue(key, out value);
        }
    }

    /// An IFormFile backed by a byte array, for the upload tests.
    public sealed class FakeFormFile : IFormFile
    {
        readonly byte[] content;

        public FakeFormFile(string fileName, byte[] content)
        {
            FileName = fileName;
            this.content = content ?? Array.Empty<byte>();
        }

        public FakeFormFile(string fileName, int sizeInBytes)
            : this(fileName, new byte[sizeInBytes])
        {
        }

        public string ContentType { get; set; } = "application/octet-stream";
        public string ContentDisposition { get; set; } = string.Empty;
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public long Length
        {
            get { return content.Length; }
        }
        public string Name { get; set; } = "file";
        public string FileName { get; set; }

        public void CopyTo(Stream target)
        {
            target.Write(content, 0, content.Length);
        }

        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        {
            return target.WriteAsync(content, 0, content.Length, cancellationToken);
        }

        public Stream OpenReadStream()
        {
            return new MemoryStream(content, writable: false);
        }
    }

    /// A stream that deliberately returns fewer bytes than asked for, to prove the upload
    /// reader loops until the buffer is full instead of trusting a single Read.
    public sealed class DribbleFormFile : IFormFile
    {
        readonly byte[] content;

        public DribbleFormFile(string fileName, byte[] content)
        {
            FileName = fileName;
            this.content = content;
        }

        public string ContentType { get; set; } = "application/octet-stream";
        public string ContentDisposition { get; set; } = string.Empty;
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public long Length
        {
            get { return content.Length; }
        }
        public string Name { get; set; } = "file";
        public string FileName { get; set; }

        public void CopyTo(Stream target)
        {
            target.Write(content, 0, content.Length);
        }

        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        {
            return target.WriteAsync(content, 0, content.Length, cancellationToken);
        }

        public Stream OpenReadStream()
        {
            return new DribbleStream(content);
        }

        sealed class DribbleStream : Stream
        {
            readonly byte[] data;
            int position;

            public DribbleStream(byte[] data)
            {
                this.data = data;
            }

            public override bool CanRead
            {
                get { return true; }
            }
            public override bool CanSeek
            {
                get { return false; }
            }
            public override bool CanWrite
            {
                get { return false; }
            }
            public override long Length
            {
                get { return data.Length; }
            }
            public override long Position
            {
                get { return position; }
                set { throw new NotSupportedException(); }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (position >= data.Length)
                {
                    return 0;
                }

                // one byte at a time: the worst case a caller must tolerate
                buffer[offset] = data[position];
                position++;
                return 1;
            }

            public override void Flush()
            {
            }
            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }
            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }
            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }
        }
    }
}
