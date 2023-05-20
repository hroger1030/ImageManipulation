using ImageManipulation;
using NUnit.Framework;
using System;
using System.Drawing;

namespace GeometryTests
{
    [TestFixture]
    public class BitmapTests
    {
        private Bitmap _TestObject;

        [SetUp]
        public void Init()
        {
            _TestObject = new Bitmap(200, 200);
        }

        [TearDown]
        public void Dispose()
        {
            if (_TestObject != null)
                _TestObject.Dispose();
        }

        [Test]
        [Category("ResizeImage")]
        public void TestResizing()
        {
            var new_image = HelperFunctions.ResizeImage(_TestObject, 400, 400);

            Assert.IsTrue(new_image.Width == 400, "Failed width check");
            Assert.IsTrue(new_image.Height == 400, "Failed height check");
        }

        [Test]
        [Category("ResizeImage")]
        public void TestIllegalResizing()
        {
            Assert.Throws<ArgumentException>(() => HelperFunctions.ResizeImage(_TestObject, -1, -1));
        }
    }
}
