using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace OCR.Classes
{
    public class Utility
    {
        public async Task<byte[]> DownloadImage(string ImageUri)
        {
            byte[] byteData = null;
            using (HttpClient client = new HttpClient())
            {
                byteData = await client.GetByteArrayAsync(ImageUri).ConfigureAwait(false);
            }
            return byteData;
        }
    }
}