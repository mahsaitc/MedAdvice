using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MedAdvice.Services
{
    /// Reads an uploaded image into a byte array, enforcing a size cap and an extension
    /// allowlist. Returns false with a message when the file is not acceptable, so callers
    /// can report the problem rather than silently storing nothing.
    public static class ImageUpload
    {
        public const long MaxBytes = 5 * 1024 * 1024;

        static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

        public static bool TryRead(IFormFile file, out byte[] content, out string error)
        {
            content = null;
            error = null;

            if (file == null || file.Length == 0)
            {
                error = "لطفا یک تصویر انتخاب کنید.";
                return false;
            }

            if (file.Length > MaxBytes)
            {
                error = $"حجم فایل «{file.FileName}» بیشتر از ۵ مگابایت است.";
                return false;
            }

            string extension = Path.GetExtension(file.FileName ?? string.Empty).ToLowerInvariant();
            if (Array.IndexOf(AllowedExtensions, extension) < 0)
            {
                error = $"فرمت فایل «{file.FileName}» مجاز نیست. فقط jpg، jpeg و png پذیرفته می‌شود.";
                return false;
            }

            byte[] buffer = new byte[file.Length];
            using (Stream stream = file.OpenReadStream())
            {
                // ReadExactly loops until the buffer is full or throws; a bare Read may
                // return fewer bytes than requested and silently truncate the image.
                stream.ReadExactly(buffer, 0, buffer.Length);
            }

            content = buffer;
            return true;
        }

        /// Reads a default image shipped alongside the application. Resolved from the
        /// content root so it does not depend on the current working directory.
        public static byte[] ReadDefault(IWebHostEnvironment env, string fileName)
        {
            return File.ReadAllBytes(Path.Combine(env.ContentRootPath, fileName));
        }
    }
}
