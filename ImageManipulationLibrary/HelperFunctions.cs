using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace ImageManipulation
{
    public static class HelperFunctions
    {
        /// <summary>
        /// Returns a collection of image encoders in dictionary format with the key being the mime type. ("image/jpeg")
        /// </summary>
        public static Dictionary<string, ImageCodecInfo> GetEncoderInfo()
        {
            var output = new Dictionary<string, ImageCodecInfo>();

            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

            foreach (var codec in encoders)
                output.Add(codec.MimeType, codec);

            return output;
        }

        /// <summary>
        /// Method to resize images given a max height and width, while maintaing aspect ratio.
        /// </summary>
        public static Bitmap ResizeImageToMaximumSize(Bitmap image, int max_width, int max_height)
        {
            if (image == null)
                throw new ArgumentException("Cannot resize a null image");

            // resizing to less than 1 is pointless... 
            // (get it? pointless? bwhahaha...)

            if (max_width < 1)
                throw new ArgumentException($"Cannot scale an image to a width of {max_width}");

            if (max_height < 1)
                throw new ArgumentException($"Cannot scale an image to a height of {max_height}");

            // Figure out the ratio
            double ratio_x = (double)max_width / (double)image.Width;
            double ratio_y = (double)max_height / (double)image.Height;

            // use smaller value
            double ratio = ratio_x < ratio_y ? ratio_x : ratio_y;

            int resize_width = Convert.ToInt32(image.Width * ratio);
            int resize_height = Convert.ToInt32(image.Height * ratio);

            return ResizeImage(image, resize_width, resize_height);
        }

        /// <summary>
        /// Rescales an image in byte format by a fixed factor given a fixed scale ratio
        /// </summary>
        public static byte[] ResizeImage(byte[] image_bytes, double scale_factor)
        {
            if (image_bytes == null)
                throw new ArgumentException("Image bytes cannot be null");

            if (image_bytes.Length < 1)
                throw new ArgumentException("Image bytes cannot empty");

            using (var input_stream = new MemoryStream(image_bytes))
            {
                using (Bitmap input_image = new Bitmap(input_stream))
                {
                    using (Bitmap output_bitmap = ResizeImage(input_image, scale_factor))
                    {
                        using (var output_stream = new MemoryStream())
                        {
                            output_bitmap.Save(output_stream, ImageFormat.Png);
                            return output_stream.ToArray();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Rescales an image by a fixed factor given a fixed scale ratio
        /// </summary>
        public static Bitmap ResizeImage(Bitmap image, double scale_factor)
        {
            if (scale_factor == 1.0)
                return image;

            if (scale_factor < double.Epsilon)
                throw new ArgumentException($"Cannot scale an image by {scale_factor}");

            int new_width = (int)Math.Round(scale_factor * image.Width);
            int new_height = (int)Math.Round(scale_factor * image.Height);

            return ResizeImage(image, new_width, new_height);
        }

        /// <summary>
        /// resizes an image without preserving aspect ratio
        /// </summary>
        public static byte[] ResizeImage(byte[] image_bytes, int width, int height)
        {
            if (image_bytes == null)
                throw new ArgumentException("Image bytes cannot be null");

            if (image_bytes.Length < 1)
                throw new ArgumentException("Image bytes cannot empty");

            using (var input_stream = new MemoryStream(image_bytes))
            {
                using (var input_image = new Bitmap(input_stream))
                {
                    using (var output_image = ResizeImage(input_image, width, height))
                    {
                        using (var output_stream = new MemoryStream())
                        {
                            output_image.Save(output_stream, ImageFormat.Png);
                            return output_stream.ToArray();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// resizes an image without preserving aspect ratio
        /// </summary>
        public static Bitmap ResizeImage(Bitmap input_image, int width, int height)
        {
            if (width < 1)
                throw new ArgumentException($"Cannot scale an image to a width of {width}");

            if (height < 1)
                throw new ArgumentException($"Cannot scale an image to a height of {height}");

            Bitmap output_image = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            output_image.SetResolution(input_image.HorizontalResolution, input_image.VerticalResolution);

            using (var graphics = Graphics.FromImage(output_image))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // we do this to prevent alias lines from being drawn around the image borders.
                // this is probably better than putting down a background flod fill.
                using (var image_attributes = new ImageAttributes())
                {
                    image_attributes.SetWrapMode(WrapMode.TileFlipXY);

                    graphics.DrawImage(input_image,
                                        new Rectangle(0, 0, width, height),             // target image area
                                        0, 0, input_image.Width, input_image.Height,    // source image area
                                        GraphicsUnit.Pixel,
                                        image_attributes);
                }
            }

            return output_image;
        }

        /// <summary>
        /// Writes text to an exiting image.
        /// </summary>
        public static Bitmap WriteTextToImage(Bitmap image, string font_name, float font_size, FontStyle font_style, string text, Color text_color)
        {
            if (image == null)
                throw new ArgumentException("Cannot write text to a null image");

            if (string.IsNullOrWhiteSpace(font_name))
                throw new ArgumentException("Cannot write text with a null font");

            if (font_size < float.Epsilon)
                throw new ArgumentException("Font size must be greater than 0");

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Cannot render a null or empty string");

            if (text_color == null)
                throw new ArgumentException("Text color cannot be null");

            RectangleF write_area = new RectangleF(0, 0, image.Width, image.Height);

            using (Font font = new Font(font_name, font_size, FontStyle.Regular, GraphicsUnit.Pixel))
            {
                using (var graphics = Graphics.FromImage(image))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                    graphics.DrawString(text, font, new SolidBrush(text_color), write_area);
                }
            }

            return image;
        }

        /// <summary>
        /// Creates new image from text string. Output image is sized to fit input text.
        /// </summary>
        public static Bitmap WriteTextToImage(string font_name, float font_size, FontStyle font_style, string text, Color text_color, Color background_color, bool transparent)
        {
            if (string.IsNullOrWhiteSpace(font_name))
                throw new ArgumentException("Cannot write text with a null font");

            if (font_size < float.Epsilon)
                throw new ArgumentException("Font size must be greater than 0");

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Cannot render a null or empty string");

            if (text_color == null)
                throw new ArgumentException("Text color cannot be null");

            using (Font font = new Font(font_name, font_size, font_style, GraphicsUnit.Pixel))
            {
                SizeF estimated_size = EstimateTextWidth(font, text);
                Bitmap output_image = new Bitmap((int)estimated_size.Width, (int)estimated_size.Height);

                using (var graphics = Graphics.FromImage(output_image))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                    graphics.Clear(background_color);
                    graphics.DrawString(text, font, new SolidBrush(text_color), new RectangleF(0, 0, output_image.Width, output_image.Height));
                }

                if (transparent)
                    output_image.MakeTransparent(background_color);

                return output_image;
            }
        }

        public static Dictionary<char, Bitmap> GenerateFontMappedCharacterSet(string font_name, float font_size, FontStyle font_style, char[] character_set, Color text_color, Color background_color, bool transparent)
        {
            if (string.IsNullOrWhiteSpace(font_name))
                throw new ArgumentException("Cannot write text with a null font");

            if (font_size < float.Epsilon)
                throw new ArgumentException("Font size must be greater than 0");

            if (character_set == null || character_set.Length < 1)
                throw new ArgumentException("Cannot render a null or empty character set");

            if (text_color == null)
                throw new ArgumentException("Text color cannot be null");

            if (background_color == null)
                throw new ArgumentException("Background color cannot be null");

            var output = new Dictionary<char, Bitmap>();

            foreach (var character in character_set)
            {
                if (!output.ContainsKey(character))
                {
                    var character_image = WriteTextToImage(font_name, font_size, font_style, character.ToString(), text_color, background_color, transparent);
                    output.Add(character, character_image);
                }
            }

            return output;
        }

        /// <summary>
        /// Attempts to calculate the size of a rendered text block.
        /// </summary>
        public static SizeF EstimateTextWidth(Font font, string text)
        {
            if (font == null)
                throw new ArgumentException("Cannot write text with a null font");

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Cannot render a null or empty string");

            if (font.Size < float.Epsilon)
                throw new ArgumentException("Font size must be greater than 0");

            using (Bitmap buffer = new Bitmap(1, 1))
            {
                using (var graphics = Graphics.FromImage(buffer))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                    SizeF render_size = graphics.MeasureString(text, font);

                    // hack, we always seem to clip the last character.
                    // If we add the width of 1 character, it might just work?
                    render_size.Width *= (1 / (float)text.Length + 1f);

                    return render_size;
                }
            }
        }

        /// <summary>
        /// renders an image on top of another image. Overlayed image will be centered on background image.
        /// </summary>
        public static Bitmap OverlayImage(Bitmap backgound_image, Bitmap overlay_image)
        {
            Point center_point = new Point((backgound_image.Width - overlay_image.Width) / 2, (backgound_image.Height - overlay_image.Height) / 2);
            return OverlayImage(backgound_image, overlay_image, center_point);
        }

        /// <summary>
        /// renders an image on top of another image. Overlayed image will be placed at location of point.
        /// </summary>
        public static Bitmap OverlayImage(Bitmap backgound_image, Bitmap overlay_image, Point overlay_point)
        {
            if (backgound_image == null)
                throw new ArgumentException("Background image cannot be null");

            if (overlay_image == null)
                throw new ArgumentException("Overlay image cannot be null");

            using (var graphics = Graphics.FromImage(backgound_image))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                graphics.DrawImage(overlay_image, overlay_point.X, overlay_point.Y, overlay_image.Width, overlay_image.Height);
            }

            return backgound_image;
        }

        /// <summary>
        /// Draws a border around an image. The image will be resized to: 
        /// current width + (2 * border thickness), current height + (2 * border thickness) 
        /// </summary>
        public static Bitmap DrawBorderAroundImage(Bitmap image, int border_width, Color border_color)
        {
            if (image == null)
                throw new ArgumentException("Image cannot be null");

            if (border_color == null)
                throw new ArgumentException("Border Color cannot be null");

            if (border_width < 1)
                throw new ArgumentException("Border width cannot be less than 1");

            Bitmap output = new Bitmap(image.Width + 2 * border_width, image.Height + 2 * border_width);

            using (var graphics = Graphics.FromImage(output))
            {
                graphics.FillRectangle(new SolidBrush(border_color), new Rectangle(0, 0, output.Width, output.Height));

                graphics.DrawImage(image,
                    new Rectangle(border_width, border_width, image.Width, image.Height),
                    new Rectangle(0, 0, image.Width, image.Height),
                    GraphicsUnit.Pixel);
            }

            return output;
        }

        /// <summary>
        /// Converts an image from one format to another in memory. 
        /// </summary>
        public static Bitmap ConvertImageFormat(Bitmap image, ImageFormat new_format)
        {
            if (image == null)
                throw new ArgumentException("Image cannot be null");

            if (new_format == null)
                throw new ArgumentException("Image format cannot be null");

            if (new_format.Equals(image.RawFormat))
            {
                return image;
            }
            else
            {
                using (var memory_stream = new MemoryStream())
                {
                    image.Save(memory_stream, new_format);
                    return (Bitmap)Image.FromStream(memory_stream);
                }
            }
        }

        /// <summary>
        /// Converts an image from one format to another in memory. 
        /// </summary>
        public static byte[] ConvertImageFormat(byte[] image_bytes, ImageFormat new_format)
        {
            if (image_bytes == null)
                throw new ArgumentException("Image bytes cannot be null");

            if (image_bytes.Length < 1)
                throw new ArgumentException("Image bytes cannot empty");

            using (var input_stream = new MemoryStream(image_bytes))
            {
                using (var image = new Bitmap(input_stream))
                {
                    using (var output_stream = new MemoryStream())
                    {
                        image.Save(output_stream, new_format);
                        return output_stream.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Fills an entire image with a single color. 
        /// </summary>
        public static Bitmap FloodFillImage(Bitmap image, Color text_color)
        {
            if (image == null)
                throw new ArgumentException("Image cannot be null");

            if (text_color == null)
                throw new ArgumentException("Text color cannot be null");

            using (var graphics = Graphics.FromImage(image))
            {
                graphics.Clear(text_color);
            }

            return image;
        }

        /// <summary>
        /// Changes the opacity of an image. oacity can be a value between 0 and 1 inclusive.
        /// </summary>
        public static Bitmap AlterImageOpacity(Bitmap image, float opacity)
        {
            if (image == null)
                throw new ArgumentException("Image cannot be null");

            if (opacity < 0)
                throw new ArgumentException("Image opacity cannot be less than 0");

            if (opacity > 1)
                throw new ArgumentException("Image opacity cannot be greater than 1");

            var output = new Bitmap(image.Width, image.Height);

            using (Graphics graphics = Graphics.FromImage(output))
            {
                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix33 = opacity;

                ImageAttributes image_attributes = new ImageAttributes();

                image_attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                graphics.DrawImage(
                    image,
                    new Rectangle(0, 0, output.Width, output.Height),
                    0,
                    0,
                    image.Width,
                    image.Height,
                    GraphicsUnit.Pixel,
                    image_attributes);
            }

            return output;
        }

        /// <summary>
        /// In the image module we converted signature images into transparent gifs, this is intended to be a replacement.
        /// This is a much better / safe method, as it doesn't use any unnmanaged code pointers.
        /// </summary>
        public static Bitmap MakeImageTransparent(Bitmap image, Color transparency_color)
        {
            image.MakeTransparent(transparency_color);
            return image;
        }

        /// <summary>
        /// Converts and image into a greyscale image. Matrix math is faster and safer than per pixel manipulation, but still not cheap.
        /// </summary>
        public static Bitmap ConvertImageToGrayscale(Bitmap input)
        {
            var output = new Bitmap(input.Width, input.Height);

            using (Graphics graphics = Graphics.FromImage(output))
            {
                ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);

                //draw the original image on the new image using the grayscale color matrix
                graphics.DrawImage(input,
                    new Rectangle(0, 0, input.Width, input.Height),
                    0, 0, input.Width, input.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }

            return output;
        }

        /// <summary>
        /// This method was created because the Bitmap.MakeTransparent() call sucks...It backfills the transparency
        /// with black, wich creates a problem later if the image is then flattened. This method was written to 
        /// allow you to flatten an image with transparency.
        /// </summary>
        public static byte[] FlattenTransparentImage(byte[] image_bytes, Color new_background_color)
        {
            if (image_bytes == null)
                throw new ArgumentException("Image bytes cannot be null");

            if (image_bytes.Length < 1)
                throw new ArgumentException("Image bytes cannot empty");

            using (var input_stream = new MemoryStream(image_bytes))
            {
                using (var input_bitmap = new Bitmap(input_stream))
                {
                    using (Bitmap output = FlattenTransparentImage(input_bitmap, new_background_color))
                    {
                        using (var output_stream = new MemoryStream())
                        {
                            output.Save(output_stream, input_bitmap.RawFormat);
                            return output_stream.ToArray();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Flattens an image over a colored background to replace the transparency with a solid color.
        /// </summary>
        public static Bitmap FlattenTransparentImage(Bitmap image, Color new_background_color)
        {
            var output_image = new Bitmap(image.Width, image.Height);

            using (var graphics = Graphics.FromImage(output_image))
            {
                graphics.FillRectangle(new SolidBrush(new_background_color), 0, 0, output_image.Width, output_image.Height);
                graphics.DrawImage(image, Point.Empty);
            }

            return output_image;
        }

        /// <summary>
        /// Convert image to grayscale image.
        /// </summary>
        public static Bitmap ConvertToGrayscale(Bitmap input)
        {
            var output = new Bitmap(input.Width, input.Height);

            using (Graphics graphics = Graphics.FromImage(output))
            {
                ColorMatrix color_matrix = new ColorMatrix(new float[][]
                {
                    new float[] {0.30f, 0.30f, 0.30f, 0.00f, 0.00f},
                    new float[] {0.59f, 0.59f, 0.59f, 0.00f, 0.00f},
                    new float[] {0.11f, 0.11f, 0.11f, 0.00f, 0.00f},
                    new float[] {0.00f, 0.00f, 0.00f, 1.00f, 0.00f},
                    new float[] {0.00f, 0.00f, 0.00f, 0.00f, 1.00f}
                });

                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(color_matrix);

                graphics.DrawImage(input,
                    new Rectangle(0, 0, input.Width, input.Height),
                    0,
                    0,
                    input.Width,
                    input.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }

            return output;
        }

        /// <summary>
        /// Convert image to negative image.
        /// </summary>
        public static Bitmap ConvertToNegative(Bitmap input)
        {
            var output = new Bitmap(input.Width, input.Height);

            using (Graphics graphics = Graphics.FromImage(output))
            {
                ColorMatrix color_matrix = new ColorMatrix(new float[][]
                {
                    new float[] {-1, 0, 0, 0, 0},
                    new float[] {0, -1, 0, 0, 0},
                    new float[] {0, 0, -1, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(color_matrix);

                graphics.DrawImage(input,
                    new Rectangle(0, 0, input.Width, input.Height),
                    0,
                    0,
                    input.Width,
                    input.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }

            return output;
        }

        public static byte[] ComputeThumbprint(Bitmap input)
        {
            // http://www.hackerfactor.com/blog/?/archives/432-Looks-Like-It.html

            // 1) resize
            Bitmap thumbnail = ResizeImage(input, 8, 8);

            // 2) convert to greyscale
            thumbnail = ConvertImageToGrayscale(thumbnail);

            // 3) get average pixel color
            int average_value = 0;

            for (int i = 0; i < 63; i++)
                average_value += thumbnail.GetPixel(i % 8, i / 8).R;

            average_value /= 64;

            // 4) set the bits.  bits are set if above average
            ulong image_hash = 0;

            for (int i = 0; i < 63; i++)
            {
                bool buffer = thumbnail.GetPixel(i % 8, i / 8).R > average_value;

                if (buffer)
                    image_hash |= ((ulong)1 << i);
            }

            if (thumbnail != null)
                thumbnail.Dispose();

            return BitConverter.GetBytes(image_hash);
        }
    }
}
