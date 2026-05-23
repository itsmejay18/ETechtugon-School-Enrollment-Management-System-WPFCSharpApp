using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NUnit.Framework;
using School_Management_System.Common;

namespace School_Management_System.Tests
{
    [TestFixture]
    public sealed class ImageFileReaderTests
    {
        [Test]
        public void TryReadImageBytes_AllowsSmallValidImage()
        {
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "valid-photo.png");
            using (var bitmap = new Bitmap(4, 4))
            {
                bitmap.SetPixel(0, 0, Color.Blue);
                bitmap.Save(path, ImageFormat.Png);
            }

            byte[] bytes;
            string errorMessage;
            var ok = ImageFileReader.TryReadImageBytes(path, out bytes, out errorMessage);

            Assert.That(ok, Is.True);
            Assert.That(bytes, Is.Not.Null.And.Not.Empty);
            Assert.That(errorMessage, Is.Null);
        }

        [Test]
        public void TryReadImageBytes_RejectsNonImageWithImageExtension()
        {
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "not-an-image.jpg");
            File.WriteAllText(path, "not really an image");

            byte[] bytes;
            string errorMessage;
            var ok = ImageFileReader.TryReadImageBytes(path, out bytes, out errorMessage);

            Assert.That(ok, Is.False);
            Assert.That(bytes, Is.Null);
            Assert.That(errorMessage, Is.EqualTo("The selected file is not a valid image."));
        }

        [Test]
        public void TryReadImageBytes_RejectsOversizedImage()
        {
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "large-photo.png");
            File.WriteAllBytes(path, new byte[ImageFileReader.MaxImageBytes + 1]);

            byte[] bytes;
            string errorMessage;
            var ok = ImageFileReader.TryReadImageBytes(path, out bytes, out errorMessage);

            Assert.That(ok, Is.False);
            Assert.That(bytes, Is.Null);
            Assert.That(errorMessage, Is.EqualTo("The selected image is too large. Use an image up to 2 MB."));
        }
    }
}
