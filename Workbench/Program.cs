using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using ImageManipulation;

namespace Workbench
{
    public class Program
    {
        private const string image1 = "D:\\SourceCode\\ImageManipulation\\Workbench\\bin\\Debug\\test1.png";
        private const string image2 = "D:\\SourceCode\\ImageManipulation\\Workbench\\bin\\Debug\\test2.png";

        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += Application_Error;

            Stopwatch timer = Stopwatch.StartNew();

            Console.Title = "TestApp";
            Console.WindowWidth = 160;
            Console.BufferWidth = 160;
            Console.WindowHeight = 30;

            //////////////////////////////////////////////////////////////////

            if (!File.Exists(image1))
                Console.WriteLine($"Cannot locate file {image1}");

            if (!File.Exists(image2))
                Console.WriteLine($"Cannot locate file {image2}");

            var bitmap1 = new Bitmap(image1);
            var bitmap2 = new Bitmap(image2);

            var hash1 = HelperFunctions.ComputeHash(bitmap1);
            var hash2 = HelperFunctions.ComputeHash(bitmap2);

            Console.WriteLine($"Image 1 hash: {HelperFunctions.ByteWriter(hash1)}");
            Console.WriteLine($"Image 2 hash: {HelperFunctions.ByteWriter(hash2)}");

            var count = HelperFunctions.ComputeHammingDistance(hash1, hash2);
            Console.WriteLine($"The two images differ by {count} bytes.");

            count = HelperFunctions.ComputeSumByteDistance(hash1, hash2);
            Console.WriteLine($"The two images differ by {count} values.");

            //////////////////////////////////////////////////////////////////

            Console.WriteLine($"Process completed in {timer.ElapsedMilliseconds} Ms");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void Application_Error(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;

                Console.WriteLine($"TestApp encountered a fatal error, '{ex.Message}', cannot continue");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();

                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unhandled application error: {ex}");
            }
        }
    }
}
