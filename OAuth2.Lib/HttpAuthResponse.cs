using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace OfficeClip.OpenSource.OAuth2.Lib
{
    public class HttpAuthResponse
    {
        public string ResponseString { get; private set; }
        public Dictionary<string, string> Headers {get; }
        public int StatusCode { get; }

        internal HttpAuthResponse(WebRequest request)
        {
            try
            {
                ResponseString = null;
                var response = request.GetResponse();
                var httpres = response as HttpWebResponse;

                if (httpres != null)
                {
                    StatusCode = (int)httpres.StatusCode;
                }
                else
                {
                    throw new Exception("Null response is returned from the website");
                }

                if (StatusCode != 200)
                {
                    throw new Exception($"Status code returned is {StatusCode}");
                }

                Headers = new Dictionary<string, string>();
                for (int i = 0; i < httpres.Headers.Count; ++i)
                {
                    Headers.Add(httpres.Headers.Keys[i], httpres.Headers[i]);
                }

                var stream = response.GetResponseStream();

                if (stream == null)
                {
                    throw new Exception("Response Stream is null");
                }

                var reader = new StreamReader(stream);
                ResponseString = reader.ReadToEnd();

                reader.Close();
                stream.Close();

            }
            catch (Exception ex)
            {
               throw new Exception($"Error: {ex.Message}");
            }
        }
    }
}
