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

        public string Filename
        {
            get { return _FileName; }
            set { _FileName = value; }
        }
        public Bitmap bitmap
        {
            get { return _Bitmap; }
            set { _Bitmap = value; }
        }

        public Image() { }

        public Image(string file_name)
        {
            _FileName = file_name;
            LoadImage();
        }

        public Image(string file_name, Bitmap image)
        {
            _FileName = file_name;
            _Bitmap = image;
        }

        public bool ResizeImage(float scale_factor)
        {
            if (scale_factor == 1f)
                return true;

            if (scale_factor < float.Epsilon)
                return false;

            int new_width = (int)(scale_factor * _Bitmap.Width);
            int new_height = (int)(scale_factor * _Bitmap.Height);

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
        public bool ScaleToMaxSize(int max_width, int max_height)
        {
            if (_Bitmap == null || _Bitmap.Width == 0 || _Bitmap.Height == 0)
                return false;

            float scale_factor = 1.0f;
            float scale_x = 1.0f;
            float scale_y = 1.0f;

            if (_Bitmap.Width > 0 && _Bitmap.Width > max_width)
                scale_x = ((float)max_width / (float)_Bitmap.Width);

            if (_Bitmap.Height > 0 && _Bitmap.Height > max_height)
                scale_y = ((float)max_height / (float)_Bitmap.Height);

            if (scale_x != 1f || scale_y != 1f)
            {
                scale_factor = (scale_x < scale_y) ? scale_x : scale_y;
                return ResizeImage(scale_factor);
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
                Bitmap buffer = new Bitmap(_Bitmap);
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
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
