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

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace ImageManipulation
{
    public class Image
    {
        protected Bitmap _Bitmap;

        public Image() { }

        public Image(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            if (!File.Exists(filename))
                throw new FileNotFoundException("File not found", filename);

            LoadImage(filename);
        }

        public Image(Bitmap bitmap)
        {
            _Bitmap = bitmap;
        }

        public bool ResizeImage(float scaleFactor)
        {
            if (scaleFactor == 1f)
                return true;

            if (scaleFactor < float.Epsilon)
                return false;

            int new_width = (int)(scaleFactor * _Bitmap.Width);
            int new_height = (int)(scaleFactor * _Bitmap.Height);

            return ResizeImage(new_width, new_height);
        }

        public bool ResizeImage(int width, int height)
        {
            if (width < 1 || height < 1)
                return false;

            try
            {
                var new_bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                new_bitmap.SetResolution(_Bitmap.HorizontalResolution, _Bitmap.VerticalResolution);

                using (Graphics graphics = Graphics.FromImage(new_bitmap))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    graphics.DrawImage(_Bitmap,
                                      new Rectangle(0, 0, width, height),
                                      new Rectangle(0, 0, _Bitmap.Width, _Bitmap.Height),
                                      GraphicsUnit.Pixel);
                }

                _Bitmap = new_bitmap;
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Scales down the image if it is larger than the arguments passed in.
        /// scalling is not preformed if the max value is 0 or less.
        /// </summary>
        public bool ScaleToMaxSize(int maxWidth, int maxHeight)
        {
            if (_Bitmap == null || _Bitmap.Width == 0 || _Bitmap.Height == 0)
                return false;

            float scaleX = 1.0f;
            float scaleY = 1.0f;

            if (_Bitmap.Width > 0 && _Bitmap.Width > maxWidth)
                scaleX = (maxWidth / _Bitmap.Width);

            if (_Bitmap.Height > 0 && _Bitmap.Height > maxHeight)
                scaleY = (maxHeight / _Bitmap.Height);

            if (scaleX != 1f || scaleY != 1f)
            {
                var scaleFactor = (scaleX < scaleY) ? scaleX : scaleY;
                return ResizeImage(scaleFactor);
            }
            else
            {
                return true;
            }
        }

        public bool SaveImage(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            if (_Bitmap == null)
                return false;

            try
            {
                // wierd fucking accrobatics in code to get the bitmap object to release its handels...
                var buffer = new Bitmap(_Bitmap);
                _Bitmap.Dispose();
                GC.Collect();

                _Bitmap = buffer;
                _Bitmap.Save(filename);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool SaveImage(ImageFormat imageFormat, string filename)
        {
            filename = filename.Replace(Path.GetExtension(filename), "." + imageFormat.ToString().ToLower());
            return SaveImage(filename);
        }

        public bool LoadImage(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            if (!File.Exists(filename))
                throw new FileNotFoundException("File not found", filename);

            try
            {
                _Bitmap = new Bitmap(filename);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
