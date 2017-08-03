using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace ImageManipulation
{
    public class cImage
    {
        protected string _FileName;
        protected Bitmap _Image;

        public string Filename
        {
            get { return _FileName; }
            set { _FileName = value; }
        }
        public Bitmap Image
        {
            get { return _Image; }
            set { _Image = value; }
        }

        public cImage() { }

        public cImage(string file_name)
        {
            _FileName = file_name;
            LoadImage();
        }

        public cImage(string file_name, Bitmap image)
        {
            _FileName = file_name;
            _Image = image;
        }

        public bool ResizeImage(float scale_factor)
        {
            if (scale_factor == 1f)
                return true;

            if (scale_factor < float.Epsilon)
                return false;

            int new_width = (int)(scale_factor * _Image.Width);
            int new_height = (int)(scale_factor * _Image.Height);

            return ResizeImage(new_width, new_height);
        }

        public bool ResizeImage(int width, int height)
        {
            if (width < 1 || height < 1)
                return false;

            try
            {
                var new_bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                new_bitmap.SetResolution(_Image.HorizontalResolution, _Image.VerticalResolution);

                using (Graphics graphics = Graphics.FromImage(new_bitmap))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    graphics.DrawImage(_Image,
                                      new Rectangle(0, 0, width, height),
                                      new Rectangle(0, 0, _Image.Width, _Image.Height),
                                      GraphicsUnit.Pixel);
                }

                _Image = new_bitmap;
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
            if (_Image == null || _Image.Width == 0 || _Image.Height == 0)
                return false;

            float scale_factor = 1.0f;
            float scale_x = 1.0f;
            float scale_y = 1.0f;

            if (_Image.Width > 0 && _Image.Width > max_width)
                scale_x = ((float)max_width / (float)_Image.Width);

            if (_Image.Height > 0 && _Image.Height > max_height)
                scale_y = ((float)max_height / (float)_Image.Height);

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
            if (_Image == null)
                return false;

            try
            {
                // wierd fucking accrobatics in code to get the bitmap object to release its handels...
                Bitmap buffer = new Bitmap(_Image);
                _Image.Dispose();
                GC.Collect();

                _Image = buffer;
                _Image.Save(_FileName);
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
                    _Image = new Bitmap(_FileName);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
