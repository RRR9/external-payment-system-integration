using System.IO;
using System.Net;
using log4net;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Text;

namespace ZudamalZetMobileServices
{
    static class HttpConnect
    {
        static private readonly ILog _log = LogManager.GetLogger(typeof(HttpConnect));

        static private string _connection = ConfigurationManager.AppSettings["connectionToProvider"];
        static public string Response { get; set; }
        static public bool RequestPassed { get; set; }

        static public bool AcceptAllCertifications(object sender, X509Certificate certification, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        static public void SendRequest(string body, string paymentId)
        {
            Response = null;
            RequestPassed = false;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls13;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);

            byte[] data = Encoding.UTF8.GetBytes(body);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_connection);
            request.ContentType = "application/xml";
            request.Method = "POST";
            request.ContentLength = data.Length;
            _log.Info($"Request[{paymentId}]: \n\n{body}\n");
            using(Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        Response = streamReader.ReadToEnd();
                        _log.Info($"Response[{paymentId}]: \n\n{Response}\n");
                        RequestPassed = true;
                    }
                }
            }
        }
    }
}
