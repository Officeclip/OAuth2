using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Configuration;
using OfficeClip.OpenSource.OAuth2.Lib.Configuration;

namespace OfficeClip.OpenSource.OAuth2.Lib
{
    public class Utils
    {
        static readonly object thisLock = new object();

        /// <summary>
        /// Compiles a list of parameters into a working query-string.
        /// </summary>
        /// <param name="parameters">Parameters to compile.</param>
        /// <returns>Compilled query-string.</returns>
        public static string BuildQueryString(Dictionary<string, string> parameters)
        {
            return parameters?.Aggregate(
                                    "",
                                    (current, parameter) => current + ("&" + parameter.Key + "=" + HttpUtility.UrlEncode(parameter.Value)))
                              .Substring(1);
        }

        /// <summary>
        /// Perform a HTTP web request to a given URL.
        /// </summary>
        /// <param name="url">URL to request.</param>
        /// <param name="parameters"></param>
        /// <param name="isPost"></param>
        /// <param name="accessToken"></param>
        /// <returns>String of response.</returns>
        public static HttpAuthResponse MakeWebRequest(
            string url,
            Dictionary<string, string> parameters,
            bool isPost,
            string accessToken = null)
        {      
            lock(thisLock)
            {
                WebRequest request;
                string parameterData = BuildQueryString(parameters) ?? string.Empty;
                if (isPost)
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    request = WebRequest.Create(url);
                    byte[] data = Encoding.ASCII.GetBytes(parameterData);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = data.Length;

                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(data, 0, data.Length);
                    requestStream.Close();
                }
                else // get request
                {
                    request = WebRequest.Create($"{url}?{parameterData}");
                    request.Method = "GET";
                }
                if (accessToken != null)
                {
                    AddSecurityHeader(request, accessToken);
                }
                return new HttpAuthResponse(request);
            }
        }

        private static void AddSecurityHeader(WebRequest webRequest, string accessToken)
        {
            webRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
        }

        public static Element LoadConfigurationFromWebConfig(string name)
        {
            var ccRoot =
                ConfigurationManager.GetSection("oAuth2Settings") as Section;
            if (ccRoot != null)
            {
                IEnumerator configurationReader = ccRoot.OAuthVClientConfigurations.GetEnumerator();

                while (configurationReader.MoveNext())
                {
                    var currentOauthElement = configurationReader.Current as Element;
                    if (currentOauthElement.Name == name)
                    {
                        return currentOauthElement;
                    }
                }

            }
            return null;
        }

        public static bool IsJson(string response)
        {
            return
                response.StartsWith("{") &&
                response.EndsWith("}");
        }
    }
}
