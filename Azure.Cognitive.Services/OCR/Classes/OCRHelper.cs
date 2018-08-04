using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using OCR.Models;
using Newtonsoft.Json;
using System.Text;

namespace OCR.Classes
{
    public class OCRHelper
    {
        string uriBase = ConfigurationManager.AppSettings.Get("URIBase");
        string subscriptionKey = ConfigurationManager.AppSettings.Get("SubscriptionKey");
        string lunchMenuUri = ConfigurationManager.AppSettings.Get("lunchMenuUri");

        public async Task<string> GetMenuByDay(string WeekDay)
        {
            WeekDay = WeekDay.ToUpper();
            List<string> dayMenu = new List<string>();
            bool startLogging = false;
            List<string> weekDayNames = new List<string> {"MON", "TUE", "WED", "THU", "FRI"};
            string retVal = String.Empty;
            try
            {
                List<string> fullMenu = await GetLunchMenuAsync();
                fullMenu.ForEach(m => {
                    if (m.Equals(WeekDay, StringComparison.CurrentCultureIgnoreCase))
                    {
                        startLogging = true;
                        weekDayNames.Remove(WeekDay);
                    }
                    if (weekDayNames.Contains(m.ToUpper()))
                    {
                        startLogging = false;
                    }
                    if (startLogging)
                    {
                        dayMenu.Add(m);
                    }
                });
                retVal = string.Join(",", dayMenu.Select(o => o));
                retVal = retVal.Substring(retVal.IndexOf(",") + 1,retVal.Length- retVal.IndexOf(",")-1);
                retVal = retVal.Substring(retVal.IndexOf(",") + 1, retVal.Length - retVal.IndexOf(",") - 1);
            }
            catch (Exception exp)
            {
                retVal = "An error occurred in processing this request. " + exp.Message;
            }
            return retVal;
        }

        private async Task<List<string>> GetLunchMenuAsync()
        {
            string retVal = null;
            List<string> WeekMenu = new List<string>();

            try
            {
                LunchMenuModel lunchMenu = await ResolveTextAsync().ConfigureAwait(false);

                lunchMenu.Regions.ForEach(r => {
                    r.Lines.ForEach(l => {
                        string result = string.Join(" ", (l.Words.Select(s => s.Text))).Replace("•", "").Trim();
                        if (result.Length > 0)
                        {
                            WeekMenu.Add(result);
                        }
                    });
                });
                WeekMenu.RemoveAt(WeekMenu.Count - 1);
            }
            catch (Exception exp)
            {
                throw;
            }
            return WeekMenu;
        }

        private async Task<LunchMenuModel> ResolveTextAsync()
        {
            LunchMenuModel retVal = null;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                    string requestParameters = "language=unk&detectOrientation=true";
                    string uri = uriBase + "?" + requestParameters;
                    HttpResponseMessage response;

                    byte[] byteData = await DownloadImage(lunchMenuUri);

                    using (ByteArrayContent content = new ByteArrayContent(byteData))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        response = await client.PostAsync(uri, content);
                    }

                    string contentString = await response.Content.ReadAsStringAsync();
                    retVal = JsonConvert.DeserializeObject<LunchMenuModel>(JToken.Parse(contentString).ToString());
                }
            }
            catch (Exception e)
            {
                throw;
            }
            return retVal;
        }

        private byte[] GetImageAsByteArray(string imageFilePath)
        {
            using (FileStream fileStream = new FileStream(System.Web.Hosting.HostingEnvironment.MapPath(imageFilePath), FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }

        private async Task<byte[]> DownloadImage(string ImageUri)
        {
            byte[] byteData = null;
            using (HttpClient client = new HttpClient())
            {
                byteData = await client.GetByteArrayAsync(ImageUri);
            }
            return byteData;
        }
    }
}