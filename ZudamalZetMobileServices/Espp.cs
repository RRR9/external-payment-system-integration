using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ZudamalZetMobileServices
{
    static class Espp
    {
        static public string ESPP_0104010(string number, string provSum, string agentId) // Check possibility to pay
        {
            string s = null;
            using (StreamReader streamReader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ESPP", "ESPP_0104010.xml")))
            {
                s = streamReader.ReadToEnd();
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(s);

                xml.GetElementsByTagName("f_01")[0].InnerText = number;
                xml.GetElementsByTagName("f_02")[0].InnerText = provSum;
                xml.GetElementsByTagName("f_05")[0].InnerText = "zudamalprod.0" + agentId;
                xml.GetElementsByTagName("f_07")[0].InnerText = ConfigurationManager.AppSettings["contractСodeExternalPaymentSystem"];
                xml.GetElementsByTagName("f_08")[0].InnerText = ConfigurationManager.AppSettings["contractCode"];

                s = PrettyXml(xml.InnerXml);
            }

            return s;
        }

        static public string ESPP_0104090(
            string number, 
            string provSum, 
            string agentId, 
            string paymentId, 
            string regDateTime, 
            string provPaymentId, 
            string statusDateTime
        ) // Try to pay
        {
            string s = null;
            using (StreamReader streamReader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ESPP", "ESPP_0104010.xml")))
            {
                s = streamReader.ReadToEnd();
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(s);

                string controlCode = ControlCode(new string[] {
                    number,
                    provSum,
                    xml.GetElementsByTagName("f_03")[0].InnerText,
                    xml.GetElementsByTagName("f_04")[0].InnerText,
                    xml.GetElementsByTagName("f_06")[0].InnerText,
                    paymentId,
                    statusDateTime,
                    ConfigurationManager.AppSettings["contractСodeExternalPaymentSystem"],
                    "zudamalprod.0" + agentId,
                    xml.GetElementsByTagName("f_13")[0].InnerText,
                    regDateTime,
                    xml.GetElementsByTagName("f_21")[0].InnerText
                }, '&'
                );

                xml.GetElementsByTagName("f_01")[0].InnerText = number;
                xml.GetElementsByTagName("f_02")[0].InnerText = provSum;
                xml.GetElementsByTagName("f_07")[0].InnerText = paymentId;
                xml.GetElementsByTagName("f_08")[0].InnerText = statusDateTime;
                xml.GetElementsByTagName("f_10")[0].InnerText = provPaymentId;
                xml.GetElementsByTagName("f_11")[0].InnerText = ConfigurationManager.AppSettings["contractСodeExternalPaymentSystem"];
                xml.GetElementsByTagName("f_12")[0].InnerText = "zudamalprod.0" + agentId;
                xml.GetElementsByTagName("f_14")[0].InnerText = ConfigurationManager.AppSettings["cashRegisterId"];
                xml.GetElementsByTagName("f_16")[0].InnerText = regDateTime;
                xml.GetElementsByTagName("f_18")[0].InnerText = controlCode;
                xml.GetElementsByTagName("f_19")[0].InnerText = ConfigurationManager.AppSettings["contractCode"];
                xml.GetElementsByTagName("f_22")[0].InnerText = number;

                s = PrettyXml(xml.InnerXml);
            }

            return s;
        }

        static public string ESPP_0104085(string provPaymentId) // Check payment status
        {
            string s = null;
            using (StreamReader streamReader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ESPP", "ESPP_0104085.xml")))
            {
                s = streamReader.ReadToEnd();
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(s);

                xml.GetElementsByTagName("f_02")[0].InnerText = provPaymentId;
                xml.GetElementsByTagName("f_03")[0].InnerText = ConfigurationManager.AppSettings["contractСodeExternalPaymentSystem"];
                xml.GetElementsByTagName("f_04")[0].InnerText = ConfigurationManager.AppSettings["contractCode"];

                s = PrettyXml(xml.InnerXml);
            }

            return s;
        }

        static private string PrettyXml(string xml)
        {
            var stringBuilder = new StringBuilder();

            var element = XElement.Parse(xml);

            var settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            settings.NewLineOnAttributes = true;

            using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
            {
                element.Save(xmlWriter);
            }

            return stringBuilder.ToString();
        }

        static private string ControlCode(string[] s, char separator)
        {
            StringBuilder r = new StringBuilder();
            for(int i = 0; i < s.Length; ++i)
            {
                r.Append(s[i]);
                if(i != s.Length - 1)
                {
                    r.Append(separator);
                }
            }

            string md5Str = Md5(r.ToString());
            string base64Str = Convert.ToBase64String(Encoding.UTF8.GetBytes(md5Str));
            return base64Str;
        }

        static private string Md5(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }
    }
}
