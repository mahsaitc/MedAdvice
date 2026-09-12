using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MedAdvice.Services
{
    /// The folders uploads are grouped into, one per entity family. Grouping is for the
    /// operator's benefit only -- the file names are unique on their own.
    public static class ImageFolders
    {
        public const string Advices = "advices";
        public const string Blogs = "blogs";
        public const string Doctors = "doctors";
        public const string Products = "products";
        public const string Homepage = "homepage";
    }

    /// Outcome of storing an uploaded image: the web-relative path on success, or the reason
    /// it was rejected. A struct rather than out parameters, which async methods cannot use.
    public readonly struct ImageSaveResult
    {
        ImageSaveResult(bool ok, string path, string error)
        {
            Ok = ok;
            Path = path;
            Error = error;
        }

        public bool Ok { get; }

        /// Web-relative, for example "/uploads/advices/a1b2c3.jpg". Stored as-is on the
        /// entity and used directly as an img src.
        public string Path { get; }

        public string Error { get; }

        public static ImageSaveResult Success(string path)
        {
            return new ImageSaveResult(true, path, null);
        }

        public static ImageSaveResult Failure(string error)
        {
            return new ImageSaveResult(false, null, error);
        }
    }

    /// Stores uploaded images as files under wwwroot and hands back the path to serve them
    /// from, replacing the previous arrangement where every image was a varbinary(max)
    /// column rendered as a base64 data URI on every page that showed it.
    ///
    /// A service rather than a static class: it needs the web root, and writing to disk is
    /// worth being able to point at a temporary directory under test.
    public class ImageStorage
    {
        public const long MaxBytes = 5 * 1024 * 1024;

        /// Everything this service writes lives under one folder, which is the folder
        /// .gitignore excludes. Nothing else in wwwroot is user-supplied.
        public const string RootFolder = "uploads";

        static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

        readonly IWebHostEnvironment environment;

        public ImageStorage(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        public async Task<ImageSaveResult> SaveAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
            {
                return ImageSaveResult.Failure("لطفا یک تصویر انتخاب کنید.");
            }

            if (file.Length > MaxBytes)
            {
                return ImageSaveResult.Failure($"حجم فایل «{file.FileName}» بیشتر از ۵ مگابایت است.");
            }

            string extension = System.IO.Path.GetExtension(file.FileName ?? string.Empty).ToLowerInvariant();
            if (Array.IndexOf(AllowedExtensions, extension) < 0)
            {
                return ImageSaveResult.Failure($"فرمت فایل «{file.FileName}» مجاز نیست. فقط jpg، jpeg و png پذیرفته می‌شود.");
            }

            // The uploader's file name never reaches disk. A new identifier removes both the
            // collision between two people uploading "photo.jpg" and any chance of a crafted
            // name escaping the folder.
            string fileName = Guid.NewGuid().ToString("N") + extension;

            string absoluteFolder = System.IO.Path.Combine(WebRoot(), RootFolder, folder);
            Directory.CreateDirectory(absoluteFolder);

            string absolutePath = System.IO.Path.Combine(absoluteFolder, fileName);
            using (FileStream target = new FileStream(absolutePath, FileMode.CreateNew, FileAccess.Write))
            {
                // Streamed rather than buffered: the previous implementation read the whole
                // file into a byte array first, because the array was what got stored.
                await file.CopyToAsync(target);
            }

            return ImageSaveResult.Success($"/{RootFolder}/{folder}/{fileName}");
        }

        /// Removes a stored image. Safe to call with null, a path this service did not
        /// produce, or a file that is already gone: replacing an image should not fail
        /// because the previous one is missing.
        public void Delete(string webPath)
        {
            string absolutePath = ResolveStoredPath(webPath);
            if (absolutePath == null)
            {
                return;
            }

            try
            {
                if (File.Exists(absolutePath))
                {
                    File.Delete(absolutePath);
                }
            }
            catch (IOException)
            {
                // A locked file is not worth failing an admin's save over. The row no longer
                // points at it, so it is orphaned rather than lost.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        /// Maps a stored web path back to disk, refusing anything outside the upload folder
        /// so a tampered value cannot delete arbitrary files.
        string ResolveStoredPath(string webPath)
        {
            if (string.IsNullOrWhiteSpace(webPath))
            {
                return null;
            }

            string prefix = "/" + RootFolder + "/";
            if (webPath.StartsWith(prefix, StringComparison.Ordinal) == false)
            {
                return null;
            }

            string relative = webPath.Substring(prefix.Length).Replace('/', System.IO.Path.DirectorySeparatorChar);
            string uploadRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(WebRoot(), RootFolder));
            string candidate = System.IO.Path.GetFullPath(System.IO.Path.Combine(uploadRoot, relative));

            // GetFullPath resolves any "..", so this comparison is what actually enforces
            // containment rather than the string check above.
            if (candidate.StartsWith(uploadRoot + System.IO.Path.DirectorySeparatorChar, StringComparison.Ordinal) == false)
            {
                return null;
            }

            return candidate;
        }

        /// WebRootPath is null when wwwroot does not exist yet, which is the case for a
        /// freshly cloned checkout before the first upload.
        string WebRoot()
        {
            string root = environment.WebRootPath;
            if (string.IsNullOrEmpty(root))
            {
                root = System.IO.Path.Combine(environment.ContentRootPath, "wwwroot");
            }
            return root;
        }
    }
}
