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
            GetData("Sunday");
            Console.WriteLine("Press ENTER to quit");
            Console.ReadLine();
        }

        static async void GetData(string WeekDay)
        {
            List<string> weekDayNames = new List<string> { "SUNDAY", "MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY", "SATURDAY" };
            string _weekDay = String.Empty;
            if (!String.IsNullOrEmpty(WeekDay))
            {
                if (!weekDayNames.Contains(WeekDay.ToUpper()))
                {
                    WeekDay = null;
                }
            }
            if (String.IsNullOrEmpty(WeekDay))
            {
                DateTime today = DateTime.Now;
                _weekDay = today.AddDays(1).DayOfWeek.ToString().ToUpper();
                if ((_weekDay.Equals("SATURDAY") || (_weekDay.Equals("SUNDAY"))))
                {
                    _weekDay = "MONDAY";
                }
            }
            else
            {
                _weekDay = WeekDay;
            }

            OCRHelper ocr = new OCRHelper();
            string dayMenu = await ocr.GetMenuByDay(_weekDay.Substring(0, 3));
             Console.WriteLine(dayMenu);
        }

    }
}
