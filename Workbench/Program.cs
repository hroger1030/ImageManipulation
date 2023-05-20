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
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;

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
