using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MedAdvice.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MedAdvice.IntegrationTests
{
    /// The one claim the unit tests cannot make: that a stored image is actually reachable
    /// over HTTP. ImageStorage proves the file lands on disk at a given path, but whether
    /// UseStaticFiles then serves that path is a property of the pipeline, and the pipeline
    /// is what a WebApplicationFactory has.
    ///
    /// This is what replaces the guarantee base64 used to give for free: an inline data URI
    /// could not 404.
    public class UploadedImageServingTests : IClassFixture<MedAdviceFactory>
    {
        readonly MedAdviceFactory factory;

        public UploadedImageServingTests(MedAdviceFactory factory)
        {
            this.factory = factory;
        }

        static IFormFile PngFile(string name, byte[] content)
        {
            MemoryStream stream = new MemoryStream(content);
            return new FormFile(stream, 0, content.Length, "file", name)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
        }

        [Fact]
        public async Task An_uploaded_image_is_served_over_http_from_the_path_that_was_stored()
        {
            byte[] content = new byte[512];
            new Random(1).NextBytes(content);

            ImageStorage storage = factory.Services.GetRequiredService<ImageStorage>();
            ImageSaveResult saved = await storage.SaveAsync(PngFile("photo.png", content), ImageFolders.Products);
            Assert.True(saved.Ok, saved.Error);

            try
            {
                HttpResponseMessage response = await factory.CreateSessionClient().GetAsync(saved.Path);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(content, await response.Content.ReadAsByteArrayAsync());
            }
            finally
            {
                storage.Delete(saved.Path);
            }
        }

        [Fact]
        public async Task A_deleted_image_stops_being_served()
        {
            ImageStorage storage = factory.Services.GetRequiredService<ImageStorage>();
            ImageSaveResult saved = await storage.SaveAsync(PngFile("photo.png", new byte[64]), ImageFolders.Products);

            storage.Delete(saved.Path);

            HttpResponseMessage response = await factory.CreateSessionClient().GetAsync(saved.Path);

            Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task The_checked_in_default_images_are_served()
        {
            // The views fall back to these whenever a path is null, so a broken path here
            // would show as a broken image on every page with no uploaded picture.
            HttpClient client = factory.CreateSessionClient();

            foreach (string path in new[] { "/img/defaults/advice.jpg", "/img/defaults/doctor.png" })
            {
                HttpResponseMessage response = await client.GetAsync(path);
                Assert.True(response.StatusCode == HttpStatusCode.OK,
                    path + " returned " + response.StatusCode);
            }
        }
    }
}
