using System;
using System.Linq;
using System.Threading.Tasks;
using MedAdvice.Services;
using MedAdvice.Tests.Infrastructure;
using Xunit;

namespace MedAdvice.Tests.Uploads
{
    /// ImageUpload is the single reader for every upload path. Before phase three, six of
    /// those paths performed no validation at all.
    public class ImageUploadTests
    {
        static byte[] Bytes(int length)
        {
            byte[] data = new byte[length];
            for (int i = 0; i < length; i++)
            {
                data[i] = (byte)(i % 251);
            }
            return data;
        }

        [Fact]
        public async Task A_null_file_is_rejected()
        {
            ImageReadResult result = await ImageUpload.ReadAsync(null);

            Assert.False(result.Ok);
            Assert.False(string.IsNullOrWhiteSpace(result.Error));
            Assert.Null(result.Content);
        }

        [Fact]
        public async Task An_empty_file_is_rejected_rather_than_stored_as_an_empty_blob()
        {
            ImageReadResult result = await ImageUpload.ReadAsync(new FakeFormFile("photo.jpg", 0));

            Assert.False(result.Ok);
            Assert.Null(result.Content);
        }

        [Fact]
        public async Task A_file_over_five_megabytes_is_rejected()
        {
            ImageReadResult result = await ImageUpload.ReadAsync(
                new FakeFormFile("huge.jpg", (int)ImageUpload.MaxBytes + 1));

            Assert.False(result.Ok);
        }

        [Fact]
        public async Task A_file_of_exactly_five_megabytes_is_accepted()
        {
            ImageReadResult result = await ImageUpload.ReadAsync(
                new FakeFormFile("exact.jpg", (int)ImageUpload.MaxBytes));

            Assert.True(result.Ok);
            Assert.Equal((int)ImageUpload.MaxBytes, result.Content.Length);
        }

        [Theory]
        [InlineData("photo.jpg")]
        [InlineData("photo.jpeg")]
        [InlineData("photo.png")]
        [InlineData("PHOTO.JPG")]
        [InlineData("photo.PnG")]
        public async Task Allowed_extensions_are_accepted_regardless_of_case(string fileName)
        {
            ImageReadResult result = await ImageUpload.ReadAsync(new FakeFormFile(fileName, Bytes(64)));

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
            ImageReadResult result = await ImageUpload.ReadAsync(new FakeFormFile(fileName, Bytes(64)));

            Assert.False(result.Ok);
            Assert.Null(result.Content);
        }

        [Fact]
        public async Task The_bytes_read_match_the_bytes_supplied()
        {
            byte[] expected = Bytes(4096);

            ImageReadResult result = await ImageUpload.ReadAsync(new FakeFormFile("photo.png", expected));

            Assert.True(result.Ok);
            Assert.Equal(expected, result.Content);
        }

        [Fact]
        public async Task A_stream_that_returns_short_reads_still_yields_the_whole_file()
        {
            // The I6 defect: the original code called Read once and ignored the return value,
            // so a stream that handed back fewer bytes than asked for silently truncated the
            // stored image. This stream returns one byte at a time.
            byte[] expected = Bytes(2048);

            ImageReadResult result = await ImageUpload.ReadAsync(new DribbleFormFile("photo.jpg", expected));

            Assert.True(result.Ok);
            Assert.Equal(expected.Length, result.Content.Length);
            Assert.Equal(expected, result.Content);
        }

        [Fact]
        public void The_size_cap_is_five_megabytes()
        {
            Assert.Equal(5 * 1024 * 1024, ImageUpload.MaxBytes);
        }
    }
}
