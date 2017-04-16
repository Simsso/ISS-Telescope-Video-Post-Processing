using Accord.Video.FFMPEG;
using System;
using System.Drawing;
using System.IO;

namespace ISSTelescopeVideoPostProcessing
{
    class Program
    {
        private static readonly float scalingFactor = 0.1f;

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("First argument must be an input path.");
                Console.WriteLine("Second argument must be an output path.");
                Console.ReadKey();
                return;
            }

            string inputPath = args[0], outputPath = args[1];

            FileAttributes fileAttributes = File.GetAttributes(inputPath);
            if (fileAttributes.HasFlag(FileAttributes.Directory))
            {
                FrameCentering(inputPath, outputPath);
            }
            else
            {
                VideoProcessing(inputPath, outputPath);
            }
        }

        private static void FrameCentering(string inputPath, string outputPath)
        {
            string[] files = Directory.GetFiles(inputPath);
            for (int i = 0; i < files.Length; i++)
            {
                DrawTextProgressBar(i + 1, files.Length);
                using (Bitmap image = new Bitmap(files[i]))
                {
                    PointF brightnessCenterVec = image.GetBrightnessCenter(0.1f),
                        centerVec = new PointF(image.Width / 2, image.Height / 2);
                    Point shiftVec = new Point((int)Math.Round(centerVec.X - brightnessCenterVec.X), (int)Math.Round(centerVec.Y - brightnessCenterVec.Y));
                    using (Bitmap shifted = image.Shift(shiftVec, Color.FromArgb(0, 0, 0)))
                    {
                        shifted.Save(outputPath + @"\" + Path.GetFileName(files[i]));
                    }
                }
            }

            Console.WriteLine("Done");
        }

        private static void VideoProcessing(string videoPath, string outputFolder)
        {
            VideoFileReader videoFileReader = new VideoFileReader();
            videoFileReader.Open(videoPath);
            Console.WriteLine("Video loaded (" + videoFileReader.FrameCount + " frames)");

            Size scaledSize = new Size((int)Math.Round(videoFileReader.Width * scalingFactor), (int)Math.Round(videoFileReader.Height * scalingFactor));

            Console.WriteLine("Analyzing frames");

            for (int i = 0; i < videoFileReader.FrameCount; i++)
            {
                DrawTextProgressBar(i + 1, (int)videoFileReader.FrameCount);
                using (Bitmap frame = videoFileReader.ReadVideoFrame())
                {
                    using (Bitmap scaled = new Bitmap(frame, scaledSize))
                    {
                        float brightness = scaled.BrightnessOfBrightestPixels(0.02);
                        if (brightness > 0.09f)
                        {
                            frame.Save(outputFolder + @"\" + i.ToString().PadLeft(5, '0') + ".bmp");
                        }
                    }
                }
            }
            videoFileReader.Close();

            Console.WriteLine("Done");
        }


        // http://stackoverflow.com/questions/24918768/progress-bar-in-console-application
        private static void DrawTextProgressBar(int progress, int total)
        {
            // draw empty progress bar
            float onechunk = 30.0f / total;

            // draw filled part
            int position = 0;
            for (int i = 0; i < onechunk * progress; i++)
            {
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            // draw unfilled part
            for (int i = position; i <= 31; i++)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            // draw totals
            Console.CursorLeft = 35;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
        }
    }
}
