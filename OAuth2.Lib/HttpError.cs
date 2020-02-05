using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace OfficeClip.OpenSource.OAuth2.Lib
{
    public class HttpError
    {
        public HttpStatusCode StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public string ResponseStream { get; set; }

        public HttpError()
        {

        }

        public HttpError(WebResponse webResponse)
        {
            try {
                HttpWebResponse httpResponse = (HttpWebResponse)webResponse;
                StatusCode = httpResponse.StatusCode;
                StatusDescription = httpResponse.StatusDescription;
                using (Stream data = webResponse.GetResponseStream())
                using (var reader = new StreamReader(data))
                {
                    ResponseStream = reader.ReadToEnd();
                }
            }
            catch
            {

            }
        }
    }
}
