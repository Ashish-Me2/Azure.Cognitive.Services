using OCR.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harness
{
    class Program
    {
        static void Main(string[] args)
        {
            GetData();
            Console.WriteLine("Press ENTER to quit");
            Console.ReadLine();
        }

        static async void GetData()
        {
            OCRHelper ocr = new OCRHelper();
            string menu = await ocr.GetMenuByDay("");
            Console.WriteLine(menu);
        }

    }
}
