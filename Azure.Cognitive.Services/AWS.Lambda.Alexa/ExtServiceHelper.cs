using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Threading.Tasks;

namespace AWS.Lambda.Alexa
{
    public class ExtServiceHelper
    {
        public async Task<string> GetDataFromService(string ServiceURI, List<object> Parameters)
        {
            string retVal = String.Empty;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ServiceURI);
                    string parameter = (Parameters[0] != null) ? Parameters[0].ToString() : String.Empty;
                    retVal = await client.GetStringAsync(ServiceURI + parameter);
                }
            }
            catch (Exception exp)
            {
                retVal = "There was an error processing your request. " + exp.Message;
            }
            return retVal;
        }
    }
}
