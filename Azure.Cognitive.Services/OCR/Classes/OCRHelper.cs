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
using System.Threading;

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
                Dictionary<string, Word> boundingboxWords = new Dictionary<string, Word>();

                lunchMenu.Regions.ForEach(r =>
                {
                    r.Lines.ForEach(l =>
                    {
                        //--- Bounding Box Processing ---

                        l.Words.ForEach(w => {
                            boundingboxWords.Add(w.BoundingBox, w);
                        });
                        
                        
                        
                        //------------------------------------------------------------------------------------------
                        string result = string.Join(" ", (l.Words.Select(s => s.Text))).Replace("•", "").Trim();
                        if (result.Length > 0)
                        {
                            WeekMenu.Add(result);
                        }
                        //------------------------------------------------------------------------------------------
                      //  Console.WriteLine("-- LINE --");
                    });
                   // Console.WriteLine("-- REGION --");
                });
                //WeekMenu.RemoveAt(WeekMenu.Count - 1);

                //--- Bounding Box Processing ---
                Dictionary<Tuple<int,int>, Object> BBWordsArranged = new Dictionary<Tuple<int, int>, Object>();
                List<int> arrangedLines = new List<int>();

                boundingboxWords.Keys.ToList().ForEach(k =>
                {
                    Tuple<int,int> boxCoords = new Tuple<int, int>(Convert.ToInt32(k.Split(",".ToCharArray())[0].ToString()), Convert.ToInt32(k.Split(",".ToCharArray())[1].ToString()));
                    if (BBWordsArranged.ContainsKey(boxCoords))
                    {
                        BBWordsArranged[boxCoords] = boundingboxWords[k];
                    }
                    else
                    {
                        BBWordsArranged.Add(boxCoords, boundingboxWords[k]);
                    }
                });


                Dictionary<int, string> Lines = new Dictionary<int, string>();

                BBWordsArranged.Keys.Select(k => k).OrderBy(j => j.Item2).ThenBy(j=>j.Item1).ToList().ForEach(f =>
                {
                    bool assimilated = false;
                    Lines.Keys.ToList().ForEach(l => {
                        if (Math.Abs(l- f.Item2) <= 5)
                        {
                            Lines[l] = Lines[l] + "-" + ((Word)BBWordsArranged[f]).Text;
                            assimilated = true;
                            
                        }
                    });
                    if (!assimilated)
                    {
                        Lines.Add(f.Item2, ((Word)BBWordsArranged[f]).Text);
                    }
                    //Console.WriteLine(((Word)BBWordsArranged[f]).Text + " - (" + f.Item1 + "," + f.Item2 + ")");
                });

                //-------------------------------
            }
            catch (Exception exp)
            {
                throw;
            }
            return WeekMenu;
        }

        public async void ResolveMenu()
        {
            while (true)
            {
                await ResolveTextAsync();
                Thread.Sleep(14400 * 1000);
            }
        }

        private async Task<LunchMenuModel> ResolveTextAsync()
        {
            LunchMenuModel retVal = null;
            CacheManager cache = CacheManager.GetInstance();
            try
            {
                if ((cache.GetItem("MENU_OBJECT") != null) && (DateTime.Now.Subtract((DateTime)cache.GetItem("MENU_OBJECT_REFRESH_TIME")).Hours <= 6))
                {
                    retVal = (LunchMenuModel)cache.GetItem("MENU_OBJECT");
                }
                else
                {
                    retVal = await QueryLiveMenu(retVal, cache);
                }
            }
            catch (Exception e)
            {
                throw;
            }
            return retVal;
        }

        private async Task<LunchMenuModel> QueryLiveMenu(LunchMenuModel retVal, CacheManager cache)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                string requestParameters = "language=unk&detectOrientation=true";
                string uri = uriBase + "?" + requestParameters;
                HttpResponseMessage response;

                byte[] byteData = CheckAndDownloadMenu().Result;

                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    response = await client.PostAsync(uri, content);
                }
                string contentString = await response.Content.ReadAsStringAsync();
                retVal = JsonConvert.DeserializeObject<LunchMenuModel>(JToken.Parse(contentString).ToString());
            }
            //Set the final resolved entity into the Cache
            cache.SetItem("MENU_OBJECT", retVal);
            cache.SetItem("MENU_OBJECT_REFRESH_TIME", DateTime.Now);
            return retVal;
        }

        private async Task<byte[]> CheckAndDownloadMenu()
        {
            byte[] byteData = null;
            Utility util = new Utility();
            byteData = await util.DownloadImage(lunchMenuUri).ConfigureAwait(false);
            return byteData;
        }

        private byte[] GetImageAsByteArray(string imageFilePath)
        {
            using (FileStream fileStream = new FileStream(System.Web.Hosting.HostingEnvironment.MapPath(imageFilePath), FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
    }
}