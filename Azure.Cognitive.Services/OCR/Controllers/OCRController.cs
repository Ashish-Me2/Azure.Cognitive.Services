using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using OCR.Classes;

namespace OCR.Controllers
{
    public class OCRController : ApiController
    {
        private Thread menuThread; 
        public async System.Threading.Tasks.Task<string> GetAsync(string WeekDay)
        {
            List<string> weekDayNames = new List<string> {"SUNDAY","MONDAY","TUESDAY","WEDNESDAY","THURSDAY","FRIDAY","SATURDAY"};
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
            menuThread = new Thread(new ThreadStart(ocr.ResolveMenu));
            menuThread.IsBackground = true;
            menuThread.Start();
            string dayMenu = await ocr.GetMenuByDay(_weekDay.Substring(0,3));
            return dayMenu;
        }
    }
}