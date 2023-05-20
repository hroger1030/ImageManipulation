/*
The MIT License (MIT)

Copyright (c) 2007 Roger Hill

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files 
(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, 
publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do 
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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
