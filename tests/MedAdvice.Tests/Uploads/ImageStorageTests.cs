using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MedAdvice.Services;
using MedAdvice.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace MedAdvice.Tests.Uploads
{
    /// ImageStorage is the single writer for every upload path. It replaced ImageUpload,
    /// which read the file into a byte array for storage in a varbinary column; these carry
    /// the validation cases across unchanged and add the ones that only matter now that a
    /// real file lands on a real disk.
    public class ImageStorageTests : IDisposable
    {
        readonly string root;
        readonly ImageStorage storage;

        public ImageStorageTests()
        {
            root = Path.Combine(Path.GetTempPath(), "medadvice-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            storage = new ImageStorage(new TestWebHostEnvironment(root));
        }

        static byte[] Bytes(int length)
        {
            byte[] data = new byte[length];
            for (int i = 0; i < length; i++)
            {
                data[i] = (byte)(i % 251);
            }
            return data;
        }

        string Absolute(string webPath)
        {
            return Path.Combine(root, webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        }

        // ---- validation, carried over from ImageUploadTests ----

        [Fact]
        public async Task A_null_file_is_rejected()
        {
            ImageSaveResult result = await storage.SaveAsync(null, ImageFolders.Advices);

            Assert.False(result.Ok);
            Assert.Null(result.Path);
            Assert.False(string.IsNullOrWhiteSpace(result.Error));
        }

        [Fact]
        public async Task An_empty_file_is_rejected_rather_than_written_as_an_empty_file()
        {
            ImageSaveResult result = await storage.SaveAsync(new FakeFormFile("photo.jpg", 0), ImageFolders.Advices);

            Assert.False(result.Ok);
            Assert.False(Directory.Exists(Path.Combine(root, ImageStorage.RootFolder)));
        }

        [Fact]
        public async Task A_file_over_five_megabytes_is_rejected()
        {
            ImageSaveResult result = await storage.SaveAsync(
                new FakeFormFile("huge.jpg", (int)ImageStorage.MaxBytes + 1), ImageFolders.Advices);

            Assert.False(result.Ok);
        }

        [Fact]
        public async Task A_file_of_exactly_five_megabytes_is_accepted()
        {
            ImageSaveResult result = await storage.SaveAsync(
                new FakeFormFile("exact.jpg", (int)ImageStorage.MaxBytes), ImageFolders.Advices);

            Assert.True(result.Ok, result.Error);
            Assert.Equal(ImageStorage.MaxBytes, new FileInfo(Absolute(result.Path)).Length);
        }

        [Theory]
        [InlineData("photo.jpg")]
        [InlineData("photo.jpeg")]
        [InlineData("photo.png")]
        [InlineData("PHOTO.JPG")]
        [InlineData("photo.PnG")]
        public async Task Allowed_extensions_are_accepted_regardless_of_case(string fileName)
        {
            ImageSaveResult result = await storage.SaveAsync(new FakeFormFile(fileName, Bytes(64)), ImageFolders.Blogs);

            Assert.True(result.Ok, result.Error);
        }

        [Theory]
        [InlineData("payload.svg")]
        [InlineData("payload.gif")]
        [InlineData("payload.exe")]
        [InlineData("payload.php")]
        [InlineData("payload")]
        [InlineData("payload.jpg.exe")]
        public async Task Other_extensions_are_rejected(string fileName)
        {
            ImageSaveResult result = await storage.SaveAsync(new FakeFormFile(fileName, Bytes(64)), ImageFolders.Blogs);

            Assert.False(result.Ok);
            Assert.Null(result.Path);
        }

        [Fact]
        public async Task A_stream_that_returns_short_reads_still_yields_the_whole_file()
        {
            // Carried over from the I6 regression: the original code called Read once and
            // ignored the return value. Writing has the same obligation as reading did.
            byte[] expected = Bytes(2048);

            ImageSaveResult result = await storage.SaveAsync(
                new DribbleFormFile("photo.jpg", expected), ImageFolders.Products);

            Assert.True(result.Ok, result.Error);
            Assert.Equal(expected, await File.ReadAllBytesAsync(Absolute(result.Path)));
        }

        // ---- storing to disk ----

        [Fact]
        public async Task The_file_lands_on_disk_with_the_uploaded_content()
        {
            byte[] expected = Bytes(4096);

            ImageSaveResult result = await storage.SaveAsync(
                new FakeFormFile("photo.png", expected), ImageFolders.Doctors);

            Assert.True(result.Ok, result.Error);
            Assert.True(File.Exists(Absolute(result.Path)));
            Assert.Equal(expected, await File.ReadAllBytesAsync(Absolute(result.Path)));
        }

        [Fact]
        public async Task The_returned_path_is_web_relative_and_names_its_folder()
        {
            ImageSaveResult result = await storage.SaveAsync(
                new FakeFormFile("photo.jpg", Bytes(32)), ImageFolders.Homepage);

            Assert.StartsWith("/" + ImageStorage.RootFolder + "/" + ImageFolders.Homepage + "/", result.Path);
            Assert.DoesNotContain("\\", result.Path);
        }

        [Theory]
        [InlineData("photo.jpg", ".jpg")]
        [InlineData("photo.jpeg", ".jpeg")]
        [InlineData("PHOTO.PNG", ".png")]
        public async Task The_original_extension_is_preserved_in_lower_case(string fileName, string expected)
        {
            ImageSaveResult result = await storage.SaveAsync(new FakeFormFile(fileName, Bytes(32)), ImageFolders.Advices);

            Assert.Equal(expected, Path.GetExtension(result.Path));
        }

        [Fact]
        public async Task The_uploaders_file_name_never_reaches_disk()
        {
            // Both a collision risk and, for a crafted name, a traversal risk.
            ImageSaveResult result = await storage.SaveAsync(
                new FakeFormFile("../../evil name.jpg", Bytes(32)), ImageFolders.Advices);

            Assert.True(result.Ok, result.Error);
            Assert.DoesNotContain("evil", result.Path);
            Assert.DoesNotContain("..", result.Path);
            Assert.True(File.Exists(Absolute(result.Path)));
        }

        [Fact]
        public async Task Two_uploads_of_the_same_file_name_do_not_collide()
        {
            ImageSaveResult first = await storage.SaveAsync(new FakeFormFile("photo.jpg", Bytes(16)), ImageFolders.Products);
            ImageSaveResult second = await storage.SaveAsync(new FakeFormFile("photo.jpg", Bytes(32)), ImageFolders.Products);

            Assert.NotEqual(first.Path, second.Path);
            Assert.True(File.Exists(Absolute(first.Path)));
            Assert.True(File.Exists(Absolute(second.Path)));
            Assert.Equal(16, new FileInfo(Absolute(first.Path)).Length);
            Assert.Equal(32, new FileInfo(Absolute(second.Path)).Length);
        }

        [Fact]
        public async Task Folders_are_created_on_demand()
        {
            Assert.False(Directory.Exists(Path.Combine(root, ImageStorage.RootFolder)));

            await storage.SaveAsync(new FakeFormFile("photo.jpg", Bytes(16)), ImageFolders.Blogs);

            Assert.True(Directory.Exists(Path.Combine(root, ImageStorage.RootFolder, ImageFolders.Blogs)));
        }

        // ---- deleting ----

        [Fact]
        public async Task Delete_removes_the_stored_file()
        {
            ImageSaveResult result = await storage.SaveAsync(new FakeFormFile("photo.jpg", Bytes(16)), ImageFolders.Homepage);
            Assert.True(File.Exists(Absolute(result.Path)));

            storage.Delete(result.Path);

            Assert.False(File.Exists(Absolute(result.Path)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("/uploads/advices/never-existed.jpg")]
        [InlineData("/layout/img/banner/banner-1.jpg")]
        public void Delete_is_safe_for_paths_it_did_not_produce(string path)
        {
            // Replacing an image must not fail because the previous one is missing, and a
            // path outside the upload folder must simply be ignored.
            storage.Delete(path);
        }

        [Fact]
        public async Task Delete_refuses_to_escape_the_upload_folder()
        {
            string outside = Path.Combine(root, "layout", "keep-me.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(outside));
            await File.WriteAllTextAsync(outside, "still here");

            storage.Delete("/uploads/../layout/keep-me.txt");

            Assert.True(File.Exists(outside));
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(root, recursive: true);
            }
            catch (IOException)
            {
            }
        }

        /// Minimal IWebHostEnvironment so the service writes into a temporary directory.
        sealed class TestWebHostEnvironment : IWebHostEnvironment
        {
            public TestWebHostEnvironment(string webRoot)
            {
                WebRootPath = webRoot;
                ContentRootPath = webRoot;
            }

            public string WebRootPath { get; set; }
            public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
            public string ContentRootPath { get; set; }
            public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
            public string EnvironmentName { get; set; } = "Test";
            public string ApplicationName { get; set; } = "MedAdvice.Tests";
        }
    }
}
