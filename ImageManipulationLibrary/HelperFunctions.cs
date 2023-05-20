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
        public static Bitmap ResizeImageToMaximumSize(Bitmap image, int maxWidth, int maxHeight)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            // resizing to less than 1 is pointless... 
            // (get it? pointless? bwhahaha...)

            if (maxWidth < 1)
                throw new ArgumentException($"Cannot scale an image to a width of {maxWidth}");

            if (maxHeight < 1)
                throw new ArgumentException($"Cannot scale an image to a height of {maxHeight}");

            // Figure out the ratio
            double ratio_x = maxWidth / (double)image.Width;
            double ratio_y = maxHeight / (double)image.Height;

            // use smaller value
            double ratio = ratio_x < ratio_y ? ratio_x : ratio_y;

            int resize_width = Convert.ToInt32(image.Width * ratio);
            int resize_height = Convert.ToInt32(image.Height * ratio);

            return ResizeImage(image, resize_width, resize_height);
        }

        /// <summary>
        /// Rescales an image in byte format by a fixed factor given a fixed scale ratio
        /// </summary>
        public static byte[] ScaleImage(byte[] imageBytes, double scaleFactor)
        {
            if (imageBytes == null || imageBytes.Length < 1)
                throw new ArgumentNullException(nameof(imageBytes), "ImageBytes cannot be null or empty");

            using (var inputStream = new MemoryStream(imageBytes))
            {
                using (var inputImage = new Bitmap(inputStream))
                {
                    using (var outputBitmap = ScaleImage(inputImage, scaleFactor))
                    {
                        using (var outputStream = new MemoryStream())
                        {
                            outputBitmap.Save(outputStream, ImageFormat.Png);
                            return outputStream.ToArray();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Rescales an image by a fixed factor given a fixed scale ratio
        /// </summary>
        public static Bitmap ScaleImage(Bitmap image, double scaleFactor)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (scaleFactor == 1.0)
                return image;

            if (scaleFactor < double.Epsilon)
                throw new ArgumentException($"Cannot scale an image by {scaleFactor}");

            int new_width = (int)Math.Round(scaleFactor * image.Width);
            int new_height = (int)Math.Round(scaleFactor * image.Height);

            return ResizeImage(image, new_width, new_height);
        }

        /// <summary>
        /// resizes an image without preserving aspect ratio
        /// </summary>
        public static byte[] ResizeImage(byte[] imageBytes, int width, int height)
        {
            if (imageBytes == null || imageBytes.Length < 1)
                throw new ArgumentNullException(nameof(imageBytes), "ImageBytes cannot be null or empty");

            if (imageBytes.Length < 1)
                throw new ArgumentException("Image bytes cannot empty");

            using (var input_stream = new MemoryStream(imageBytes))
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
        public static Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (width < 1)
                throw new ArgumentException($"Cannot scale an image to a width of {width}");

            if (height < 1)
                throw new ArgumentException($"Cannot scale an image to a height of {height}");

            var outputImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            outputImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(outputImage))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // we do this to prevent alias lines from being drawn around the image borders.
                // this is probably better than putting down a background flood fill.
                using (var image_attributes = new ImageAttributes())
                {
                    image_attributes.SetWrapMode(WrapMode.TileFlipXY);

                    graphics.DrawImage(image,
                                        new Rectangle(0, 0, width, height),             // target image area
                                        0, 0, image.Width, image.Height,    // source image area
                                        GraphicsUnit.Pixel,
                                        image_attributes);
                }
            }

            return outputImage;
        }

        /// <summary>
        /// Writes text to an exiting image.
        /// </summary>
        public static Bitmap WriteTextToImage(Bitmap image, string fontName, float font_size, FontStyle fontStyle, string text, Color textColor)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (string.IsNullOrWhiteSpace(fontName))
                throw new ArgumentNullException(nameof(fontName));

            if (font_size < float.Epsilon)
                throw new ArgumentException("Font size must be greater than 0");

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(text), "Cannot render a null or empty string");

            var write_area = new RectangleF(0, 0, image.Width, image.Height);

            using (var font = new Font(fontName, font_size, FontStyle.Regular, GraphicsUnit.Pixel))
            {
                using (var graphics = Graphics.FromImage(image))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                    graphics.DrawString(text, font, new SolidBrush(textColor), write_area);
                }
            }

            return image;
        }

        /// <summary>
        /// Creates new image from text string. Output image is sized to fit input text.
        /// </summary>
        public static Bitmap WriteTextToImage(string fontName, float fontSize, FontStyle fontStyle, string text, Color textColor, Color backgroundColor, bool transparent)
        {
            if (string.IsNullOrWhiteSpace(fontName))
                throw new ArgumentNullException(nameof(fontName), "Cannot write text with a null font");

            if (fontSize < float.Epsilon)
                throw new ArgumentException("Font size must be greater than 0");

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(text), "Cannot render a null or empty string");

            using (var font = new Font(fontName, fontSize, fontStyle, GraphicsUnit.Pixel))
            {
                SizeF estimated_size = EstimateTextWidth(font, text);
                var output_image = new Bitmap((int)estimated_size.Width, (int)estimated_size.Height);

                using (var graphics = Graphics.FromImage(output_image))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                    graphics.Clear(backgroundColor);
                    graphics.DrawString(text, font, new SolidBrush(textColor), new RectangleF(0, 0, output_image.Width, output_image.Height));
                }

                if (transparent)
                    output_image.MakeTransparent(backgroundColor);

                return output_image;
            }
        }

        public static Dictionary<char, Bitmap> GenerateFontMappedCharacterSet(string fontName, float fontSize, FontStyle fontStyle, char[] characterSet, Color textColor, Color backgroundColor, bool transparent)
        {
            if (string.IsNullOrWhiteSpace(fontName))
                throw new ArgumentNullException(nameof(fontName), "Cannot write text with a null font");

            if (fontSize < float.Epsilon)
                throw new ArgumentException("Font size must be greater than 0");

            if (characterSet == null || characterSet.Length < 1)
                throw new ArgumentNullException(nameof(characterSet), "Cannot render a null or empty character set");

            var output = new Dictionary<char, Bitmap>();

            foreach (var character in characterSet)
            {
                if (!output.ContainsKey(character))
                {
                    var character_image = WriteTextToImage(fontName, fontSize, fontStyle, character.ToString(), textColor, backgroundColor, transparent);
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
                throw new ArgumentNullException(nameof(font));

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Cannot render a null or empty string");

            if (font.Size < float.Epsilon)
                throw new ArgumentException("Font size must be greater than 0");

            using (var buffer = new Bitmap(1, 1))
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
        public static Bitmap OverlayImage(Bitmap backgoundImage, Bitmap overlayImage)
        {
            if (backgoundImage == null)
                throw new ArgumentNullException(nameof(backgoundImage));

            var center_point = new Point((backgoundImage.Width - overlayImage.Width) / 2, (backgoundImage.Height - overlayImage.Height) / 2);
            return OverlayImage(backgoundImage, overlayImage, center_point);
        }

        /// <summary>
        /// renders an image on top of another image. Overlayed image will be placed at location of point.
        /// </summary>
        public static Bitmap OverlayImage(Bitmap backgoundImage, Bitmap overlayImage, Point overlayPoint)
        {
            if (backgoundImage == null)
                throw new ArgumentNullException(nameof(backgoundImage));

            if (overlayImage == null)
                throw new ArgumentNullException(nameof(overlayImage));

            using (var graphics = Graphics.FromImage(backgoundImage))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                graphics.DrawImage(overlayImage, overlayPoint.X, overlayPoint.Y, overlayImage.Width, overlayImage.Height);
            }

            return backgoundImage;
        }

        /// <summary>
        /// Draws a border around an image. The image will be resized to: 
        /// current width + (2 * border thickness), current height + (2 * border thickness) 
        /// </summary>
        public static Bitmap DrawBorderAroundImage(Bitmap image, int borderWidth, Color borderColor)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (borderWidth < 1)
                throw new ArgumentException("Border width cannot be less than 1");

            var output = new Bitmap(image.Width + 2 * borderWidth, image.Height + 2 * borderWidth);

            using (var graphics = Graphics.FromImage(output))
            {
                graphics.FillRectangle(new SolidBrush(borderColor), new Rectangle(0, 0, output.Width, output.Height));

                graphics.DrawImage(image,
                    new Rectangle(borderWidth, borderWidth, image.Width, image.Height),
                    new Rectangle(0, 0, image.Width, image.Height),
                    GraphicsUnit.Pixel);
            }

            return output;
        }

        /// <summary>
        /// Converts an image from one format to another in memory. 
        /// </summary>
        public static Bitmap ConvertImageFormat(Bitmap image, ImageFormat newFormat)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (newFormat == null)
                throw new ArgumentException("Image format cannot be null");

            if (newFormat.Equals(image.RawFormat))
            {
                return image;
            }
            else
            {
                using (var memory_stream = new MemoryStream())
                {
                    image.Save(memory_stream, newFormat);
                    return (Bitmap)System.Drawing.Image.FromStream(memory_stream);
                }
            }
        }

        /// <summary>
        /// Converts an image from one format to another in memory. 
        /// </summary>
        public static byte[] ConvertImageFormat(byte[] imageBytes, ImageFormat newFormat)
        {
            if (imageBytes == null || imageBytes.Length < 1)
                throw new ArgumentNullException(nameof(imageBytes), "ImageBytes cannot be null or empty");

            using (var input_stream = new MemoryStream(imageBytes))
            {
                using (var image = new Bitmap(input_stream))
                {
                    using (var output_stream = new MemoryStream())
                    {
                        image.Save(output_stream, newFormat);
                        return output_stream.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Fills an entire image with a single color. 
        /// </summary>
        public static Bitmap FloodFillImage(Bitmap image, Color textColor)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            using (var graphics = Graphics.FromImage(image))
            {
                graphics.Clear(textColor);
            }

            return image;
        }

        /// <summary>
        /// Changes the opacity of an image. oacity can be a value between 0 and 1 inclusive.
        /// </summary>
        public static Bitmap AlterImageOpacity(Bitmap image, float opacity)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (opacity < 0)
                throw new ArgumentException("Image opacity cannot be less than 0");

            if (opacity > 1)
                throw new ArgumentException("Image opacity cannot be greater than 1");

            var output = new Bitmap(image.Width, image.Height);

            using (var graphics = Graphics.FromImage(output))
            {
                var matrix = new ColorMatrix() { Matrix33 = opacity };

                var image_attributes = new ImageAttributes();

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
        public static Bitmap MakeImageTransparent(Bitmap input, Color transparencyColor)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            input.MakeTransparent(transparencyColor);
            return input;
        }

        /// <summary>
        /// Converts and image into a greyscale image. Matrix math is faster and safer than per pixel manipulation, but still not cheap.
        /// </summary>
        public static Bitmap ConvertImageToGrayscale(Bitmap input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var output = new Bitmap(input.Width, input.Height);

            using (var graphics = Graphics.FromImage(output))
            {
                var colorMatrix = new ColorMatrix(
                new float[][]
                {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

                var attributes = new ImageAttributes();
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
        public static byte[] FlattenTransparentImage(byte[] imageBytes, Color newBackgroundColor)
        {
            if (imageBytes == null || imageBytes.Length < 1)
                throw new ArgumentNullException(nameof(imageBytes), "ImageBytes cannot be null or empty");

            using (var input_stream = new MemoryStream(imageBytes))
            {
                using (var input_bitmap = new Bitmap(input_stream))
                {
                    using (Bitmap output = FlattenTransparentImage(input_bitmap, newBackgroundColor))
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
        public static Bitmap FlattenTransparentImage(Bitmap input, Color newBackgroundColor)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var output = new Bitmap(input.Width, input.Height);

            using (var graphics = Graphics.FromImage(output))
            {
                graphics.FillRectangle(new SolidBrush(newBackgroundColor), 0, 0, output.Width, output.Height);
                graphics.DrawImage(input, Point.Empty);
            }

            return output;
        }

        /// <summary>
        /// Convert image to grayscale image.
        /// </summary>
        public static Bitmap ConvertToGrayscale(Bitmap input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var output = new Bitmap(input.Width, input.Height);

            using (var graphics = Graphics.FromImage(output))
            {
                var color_matrix = new ColorMatrix(new float[][]
                {
                    new float[] {0.30f, 0.30f, 0.30f, 0.00f, 0.00f},
                    new float[] {0.59f, 0.59f, 0.59f, 0.00f, 0.00f},
                    new float[] {0.11f, 0.11f, 0.11f, 0.00f, 0.00f},
                    new float[] {0.00f, 0.00f, 0.00f, 1.00f, 0.00f},
                    new float[] {0.00f, 0.00f, 0.00f, 0.00f, 1.00f}
                });

                var attributes = new ImageAttributes();
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
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var output = new Bitmap(input.Width, input.Height);

            using (var graphics = Graphics.FromImage(output))
            {
                var colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] {-1, 0, 0, 0, 0},
                    new float[] {0, -1, 0, 0, 0},
                    new float[] {0, 0, -1, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

                var attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);

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

        public static Bitmap ConvertToSepiaTone(Bitmap input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            //http://blogs.techrepublic.com.com/howdoi/?p=120 for full details

            int height = input.Size.Height;
            int width = input.Size.Width;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = input.GetPixel(x, y);

                    double outputRed = (color.R * .393) + (color.G * .769) + (color.B * .189);
                    double outputGreen = (color.R * .349) + (color.G * .686) + (color.B * .168);
                    double outputBlue = (color.R * .272) + (color.G * .534) + (color.B * .131);

                    if (outputRed > 255) outputRed = 255;
                    if (outputGreen > 255) outputGreen = 255;
                    if (outputBlue > 255) outputBlue = 255;

                    input.SetPixel(x, y, Color.FromArgb((int)outputRed, (int)outputGreen, (int)outputBlue));
                }
            }

            return input;
        }

        public static Bitmap ConvertToToMonochrome(System.Drawing.Image input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var cm = new ColorMatrix(new float[][]
            {
                new float[] {0.299f, 0.299f, 0.299f, 0, 0},
                new float[] {0.587f, 0.587f, 0.587f, 0, 0},
                new float[] {0.114f, 0.114f, 0.114f, 0, 0},
                new float[] { 0, 0, 0, 1, 0},
                new float[] { 0, 0, 0, 0, 1}
            });

            var attributes = new ImageAttributes();
            attributes.SetColorMatrix(cm);

            Point[] points =
            {
                new Point(0, 0),
                new Point(input.Width, 0),
                new Point(0, input.Height),
            };

            var rect = new Rectangle(0, 0, input.Width, input.Height);

            var output = new Bitmap(input.Width, input.Height);
            using (Graphics gr = Graphics.FromImage(output))
            {
                gr.DrawImage(input, points, rect, GraphicsUnit.Pixel, attributes);
            }

            return output;
        }

        public static byte[] ComputeHash(Bitmap input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            // http://www.hackerfactor.com/blog/?/archives/432-Looks-Like-It.html
            // https://jenssegers.com/61/perceptual-image-hashes

            // 1) resize
            Bitmap thumbnail = ResizeImage(input, 8, 8);

            // 2) convert to gray scale
            thumbnail = ConvertImageToGrayscale(thumbnail);

            // 3) get average pixel color
            int averageValue = 0;

            for (int i = 0; i < 63; i++)
                averageValue += thumbnail.GetPixel(i % 8, i / 8).R;

            averageValue /= 64;

            // 4) set the bits.  bits are set if above average
            ulong image_hash = 0;

            for (int i = 0; i < 63; i++)
            {
                bool buffer = thumbnail.GetPixel(i % 8, i / 8).R > averageValue;

                if (buffer)
                    image_hash |= ((ulong)1 << i);
            }

            if (thumbnail != null)
                thumbnail.Dispose();

            return BitConverter.GetBytes(image_hash);
        }

        /// <summary>
        /// Method to determine how similar two image hash string are.
        /// takes in byte arrays and computes Hamming distance by counting 
        /// the number of bits that differ between them.
        /// </summary>
        public static int ComputeHammingDistance(byte[] array1, byte[] array2)
        {
            if (array1 == null)
                throw new ArgumentNullException(nameof(array1));

            if (array2 == null)
                throw new ArgumentNullException(nameof(array2));

            if (array1.Length != array2.Length)
                throw new ArgumentException("Arrays are unequal lengths");

            int diffCount = 0;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                    diffCount++;
            }

            return diffCount;
        }

        public static int ComputeSumByteDistance(byte[] array1, byte[] array2)
        {
            if (array1 == null)
                throw new ArgumentNullException(nameof(array1));

            if (array2 == null)
                throw new ArgumentNullException(nameof(array2));

            if (array1.Length != array2.Length)
                throw new ArgumentException("Arrays are unequal lengths");

            int diffCount = 0;

            for (int i = 0; i < array1.Length; i++)
                diffCount += Math.Abs(array1[i] - array2[i]);

            return diffCount;
        }

        public static string ByteWriter(byte[] array)
        {
            if (array == null || array.Length < 1)
                throw new ArgumentNullException(nameof(array), "array cannot be null or empty");

            return BitConverter.ToString(array).Replace("-", string.Empty).ToLower();
        }
    }
}
