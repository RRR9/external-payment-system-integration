using System.IO;
using System.Net;
using log4net;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace ZudamalZetMobileServices
{
    static class HttpConnect
    {
        static private readonly ILog _log = LogManager.GetLogger(typeof(HttpConnect));

        static private string _connection = ConfigurationManager.AppSettings["connectionToProvider"];
        static public string Response { get; set; }
        static public bool RequestPassed { get; set; }

        static public void SendRequest(string body)
        {
            Response = null;
            RequestPassed = false;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_connection);
            request.Method = "";
            request.Timeout = 10000;
            _log.Info($"Request: \n\n{body}\n");
            using(StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(body);
                request.ContentLength = body.Length;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        Response = streamReader.ReadToEnd();
                        _log.Info($"Response: \n\n{Response}\n");
                        RequestPassed = true;
                    }
                }
            }
        }
    }
}
