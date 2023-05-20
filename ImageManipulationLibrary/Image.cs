using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace ImageManipulation
{
    public class Image
    {
        protected string _FileName;
        protected Bitmap _Bitmap;

        public Image() { }

        public Image(string fileName)
        {
            _FileName = fileName;
            LoadImage();
        }

        public Image(string fileName, Bitmap bitmap)
        {
            _FileName = fileName;
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

        public bool SaveImage()
        {
            if (_Bitmap == null)
                return false;

            try
            {
                // wierd fucking accrobatics in code to get the bitmap object to release its handels...
                var buffer = new Bitmap(_Bitmap);
                _Bitmap.Dispose();
                GC.Collect();

                _Bitmap = buffer;
                _Bitmap.Save(_FileName);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool SaveImage(ImageFormat image_format)
        {
            _FileName = _FileName.Replace(Path.GetExtension(_FileName), "." + image_format.ToString().ToLower());
            return SaveImage();
        }

        public bool LoadImage()
        {
            try
            {
                if (File.Exists(_FileName))
                    _Bitmap = new Bitmap(_FileName);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
