using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using log4net;

namespace ZudamalZetMobileServices
{
    static class Service
    {
        static private readonly int _provId = -1; // NEED TO CHANGE

        static private readonly ILog _log = LogManager.GetLogger(typeof(Service));

        static private readonly HashSet<int> _accept = new HashSet<int>() { };
        static private readonly HashSet<int> _cancel = new HashSet<int>() { };

        static private void GetPayments()
        {
            DataTable dt = SqlServer.GetData("GetPaymentsToSend", new SqlParameter[] { new SqlParameter("@ProviderID", _provId) });

            foreach (DataRow row in dt.Rows)
            {
                try
                {
                    Payment payment = new Payment();
                    if (row["ProviderPaymentID"].ToString() == "")
                    {
                        payment.PaymentId = row["PaymentID"].ToString();
                        payment.Number = row["Number"].ToString();
                        payment.ProvSum = row["ProviderSum"].ToString().Replace(",", ".");
                        payment.RegDateTime = row["RegDateTime"].ToString();
                        payment.StatusDateTime = row["RegDatetime"].ToString();
                        payment.AgentId = "123";

                        Pay(payment);
                    }
                    else
                    {
                        payment.PaymentId = row["PaymentID"].ToString();
                        payment.ProvPaymentId = row["ProviderPaymentID"].ToString();
                        RequestToCheckPayment(payment);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            }
        }

        static private void ModifyPaymentStatus(StatusInDataBase status, Payment payment)
        {
            SqlServer.ExecSP("ModifyPaymentStatus", new SqlParameter[]
            {
                new SqlParameter("@PaymentID", payment.PaymentId),
                new SqlParameter("@ErrorCode", status),
                new SqlParameter("@ProviderPaymentID", payment.ProvPaymentId),
                new SqlParameter("@ProviderID", _provId)
            });
        }

        static private StatusInDataBase GetPaymentStatusDb(int code)
        {
            if(_accept.Contains(code))
            {
                return StatusInDataBase.Accept;
            }
            else if(_cancel.Contains(code))
            {
                return StatusInDataBase.Cancel;
            }

            return StatusInDataBase.Awaiting;
        }

        private enum StatusInDataBase : int
        {
            Awaiting = 0,
            Accept = 1,
            Cancel = 2
        }

        static public void Start()
        {
            GetPayments();
        }

        static private void RequestToCheckPayment(Payment payment)
        {
            string req;
            req = Espp.ESPP_0104085(payment.ProvPaymentId);

            HttpConnect.SendRequest(req);
            if (!HttpConnect.RequestPassed)
            {
                throw new ZetMobileException("Can not get response");
            }

            string rsp = HttpConnect.Response;
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(rsp);
            XmlElement xmlRoot = xmlDocument.DocumentElement;
            if(xmlRoot == null)
            {
                throw new ZetMobileException("Xml file dont have root");
            }

            if(xmlRoot.Name == "ESPP_2204085")
            {
                throw new ZetMobileException("Received ESPP_2204085");
            }

            if(xmlRoot.Name != "ESPP_1204085")
            {
                throw new ZetMobileException("Received incorrect ESPP");
            }

            int code = Convert.ToInt32(xmlDocument.GetElementsByTagName("f_01")[0].InnerText);
            StatusInDataBase status = GetPaymentStatusDb(code);

            ModifyPaymentStatus(status, payment);
        }

        static private void Pay(Payment payment)
        {
            RequestToPayment(ref payment);

            SendToPayment(ref payment);
        }

        static private void RequestToPayment(ref Payment payment)
        {
            string req;
            req = Espp.ESPP_0104010(payment.Number, payment.ProvSum, payment.AgentId);

            HttpConnect.SendRequest(req);
            if (!HttpConnect.RequestPassed)
            {
                throw new ZetMobileException("Can not get response");
            }

            string rsp = HttpConnect.Response;
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(rsp);
            XmlElement xmlRoot = xmlDocument.DocumentElement;

            if (xmlRoot == null)
            {
                throw new ZetMobileException("Xml file dont have root");
            }

            if (xmlRoot.Name == "ESPP_2204010")
            {
                throw new ZetMobileException("Received ESPP_2204085");
            }

            if (xmlRoot.Name != "ESPP_1204010")
            {
                throw new ZetMobileException("Received incorrect ESPP");
            }

            payment.ProvPaymentId = xmlDocument.GetElementsByTagName("f_05")[0].InnerText;
        }

        static private void SendToPayment(ref Payment payment)
        {
            string req;
            req = Espp.ESPP_0104090(
                payment.Number,
                payment.ProvSum,
                payment.AgentId,
                payment.PaymentId,
                payment.RegDateTime,
                payment.ProvPaymentId,
                payment.StatusDateTime
            );

            HttpConnect.SendRequest(req);
            if (!HttpConnect.RequestPassed)
            {
                throw new ZetMobileException("Can not get response");
            }

            string rsp = HttpConnect.Response;
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(rsp);
            XmlElement xmlRoot = xmlDocument.DocumentElement;

            if (xmlRoot == null)
            {
                throw new ZetMobileException("Xml file dont have root");
            }

            if (xmlRoot.Name == "ESPP_2204090")
            {
                throw new ZetMobileException("Received ESPP_2204090");
            }

            if (xmlRoot.Name != "ESPP_1204090")
            {
                throw new ZetMobileException("Received incorrect ESPP");
            }

            ModifyPaymentStatus(StatusInDataBase.Awaiting, payment);
        }
    }
}
