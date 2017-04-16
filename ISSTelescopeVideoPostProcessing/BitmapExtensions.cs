using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISSTelescopeVideoPostProcessing
{
    static class BitmapExtensions
    {
        public static float BrightnessOfBrightestPixels(this Bitmap bitmap, double pixelPercentage)
        {
            if (pixelPercentage < 0 || pixelPercentage > 1)
            {
                throw new ArgumentException("Pixel percentage must be in [0,1].");
            }

            int percentagePixelCount = (int)Math.Ceiling(bitmap.Width * bitmap.Height * pixelPercentage);

            Color[] brightestPixels = new Color[percentagePixelCount];

            int bufferIndex = 0;

            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    if (bufferIndex >= brightestPixels.Length)
                    {
                        Color darkest = DarkestColor(brightestPixels);
                        for (int i = 0; i < brightestPixels.Length; i++)
                        {
                            if (brightestPixels[i] == darkest)
                            {
                                if (pixel.GetBrightness() > brightestPixels[i].GetBrightness())
                                {
                                    brightestPixels[i] = pixel;
                                }
                            }
                        }
                    }
                    else
                    {
                        brightestPixels[bufferIndex++] = pixel;
                    }
                }
            }

            float sum = 0;
            for (int i = 0; i < brightestPixels.Length; i++)
            {
                sum += brightestPixels[i].GetBrightness();
            }
            return sum / brightestPixels.Length;
        }

        public static PointF GetBrightnessCenter(this Bitmap bitmap, float brightnessThreshold)
        {
            double xSum = 0, ySum = 0;
            int count = 0;
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    if (bitmap.GetPixel(x, y).GetBrightness() >= brightnessThreshold)
                    {
                        xSum += x;
                        ySum += y;
                        count++;
                    }
                }
            }

            if (count == 0)
            {
                return new PointF(0, 0);
            }
            return new PointF((float)(xSum / count), (float)(ySum / count));
        }

        public static Bitmap Shift(this Bitmap bitmap, Point vec, Color fillColor)
        {
            Bitmap output = new Bitmap(bitmap.Width, bitmap.Height);
            output.FillWithColor(fillColor);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    int targetX = vec.X + x,
                        targetY = vec.Y + y;
                    if (targetX >= 0 && targetX < bitmap.Width && targetY >= 0 && targetY < bitmap.Height)
                    {
                        output.SetPixel(targetX, targetY, bitmap.GetPixel(x, y));
                    }
                }
            }
            return output;
        }

        public static void FillWithColor(this Bitmap bitmap, Color color)
        {
            using (Graphics gfx = Graphics.FromImage(bitmap))
            {
                using (SolidBrush brush = new SolidBrush(color))
                {
                    gfx.FillRectangle(brush, 0, 0, bitmap.Width, bitmap.Height);
                }
            }
        }

        private static Color DarkestColor(Color[] array)
        {
            if (array.Length == 0)
            {
                throw new ArgumentException("Array must have length of at least 1");
            }

            Color darkest = array[0];
            for (int i = 1; i < array.Length; i++)
            {
                if (darkest.GetBrightness() > array[i].GetBrightness())
                {
                    darkest = array[i];
                }
            }

            return darkest;
        }
    }
}
