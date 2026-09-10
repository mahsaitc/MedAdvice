using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MedAdvice.Services
{
    /// Outcome of reading an uploaded image: the bytes on success, or the reason it was
    /// rejected. Exists because async methods cannot use out parameters.
    public readonly struct ImageReadResult
    {
        ImageReadResult(bool ok, byte[] content, string error)
        {
            Ok = ok;
            Content = content;
            Error = error;
        }

        public bool Ok { get; }
        public byte[] Content { get; }
        public string Error { get; }

        public static ImageReadResult Success(byte[] content)
        {
            return new ImageReadResult(true, content, null);
        }

        public static ImageReadResult Failure(string error)
        {
            return new ImageReadResult(false, null, error);
        }
    }

    /// Reads an uploaded image, enforcing a size cap and an extension allowlist, so callers
    /// can report the problem rather than silently storing nothing.
    public static class ImageUpload
    {
        public const long MaxBytes = 5 * 1024 * 1024;

        static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

        public static async Task<ImageReadResult> ReadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return ImageReadResult.Failure("لطفا یک تصویر انتخاب کنید.");
            }

            if (file.Length > MaxBytes)
            {
                return ImageReadResult.Failure($"حجم فایل «{file.FileName}» بیشتر از ۵ مگابایت است.");
            }

            string extension = Path.GetExtension(file.FileName ?? string.Empty).ToLowerInvariant();
            if (Array.IndexOf(AllowedExtensions, extension) < 0)
            {
                return ImageReadResult.Failure($"فرمت فایل «{file.FileName}» مجاز نیست. فقط jpg، jpeg و png پذیرفته می‌شود.");
            }

            byte[] buffer = new byte[file.Length];
            using (Stream stream = file.OpenReadStream())
            {
                // ReadExactlyAsync loops until the buffer is full or throws; a bare Read may
                // return fewer bytes than requested and silently truncate the image.
                await stream.ReadExactlyAsync(buffer, 0, buffer.Length);
            }

            return ImageReadResult.Success(buffer);
        }

        /// Reads a default image shipped alongside the application. Resolved from the content
        /// root so it does not depend on the current working directory.
        public static Task<byte[]> ReadDefaultAsync(IWebHostEnvironment env, string fileName)
        {
            return File.ReadAllBytesAsync(Path.Combine(env.ContentRootPath, fileName));
        }
    }
}
