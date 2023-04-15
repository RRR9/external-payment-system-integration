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

        static private readonly HashSet<int> _accept = new HashSet<int>() {  };
        static private readonly HashSet<int> _cancel = new HashSet<int>() {  };

        static private string _paymentId;
        static private string _number;
        static private string _provSum;
        static private string _regDateTime;
        static private string _statusDateTime;
        static private string _provPaymentId;
        static private string _agentId;

        static private void Initialize()
        {
            _paymentId = "";
            _number = "";
            _provSum = "";
            _regDateTime = "";
            _statusDateTime = "";
            _provPaymentId = "";
            _agentId = "";
        }

        static private void GetPayments()
        {
            DataTable dt = SqlServer.GetData("GetPaymentsToSend", new SqlParameter[] { new SqlParameter("@ProviderID", _provId) });

            foreach (DataRow row in dt.Rows)
            {
                Initialize();
                try
                {
                    if (row["ProviderPaymentID"].ToString() == "")
                    {
                        _paymentId = row["PaymentID"].ToString();
                        _number = row["Number"].ToString();
                        _provSum = row["ProviderSum"].ToString().Replace(",", ".");
                        _regDateTime = row["RegDateTime"].ToString();
                        _statusDateTime = row["StatusDateTime"].ToString();
                        _provPaymentId = row["ProviderPaymentID"].ToString();
                        _agentId = row["AgentID"].ToString();
                        Pay();
                    }
                    else
                    {
                        _paymentId = row["PaymentID"].ToString();
                        _provPaymentId = row["ProviderPaymentID"].ToString();
                        RequestToCheckPayment();
                    }
                }
                catch(Exception ex)
                {
                    _log.Error(ex);
                }
            }
        }

        static private void ModifyPaymentStatus(StatusInDataBase status)
        {
            SqlServer.ExecSP("ModifyPaymentStatus", new SqlParameter[]
            {
                new SqlParameter("@PaymentID", _paymentId),
                new SqlParameter("@ErrorCode", status),
                new SqlParameter("@ProviderPaymentID", _provPaymentId),
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

        static private void RequestToCheckPayment()
        {
            string req;
            req = Espp.ESPP_0104085(_provPaymentId);

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

            ModifyPaymentStatus(status);
        }

        static private void Pay()
        {
            RequestToPayment();

            SendToPayment();
        }

        static private void RequestToPayment()
        {
            string req;
            req = Espp.ESPP_0104010(_number, _provSum, _agentId);

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

            _provPaymentId = xmlDocument.GetElementsByTagName("f_02")[0].InnerText;
        }

        static private void SendToPayment()
        {
            string req;
            req = Espp.ESPP_0104090(_number, _provSum, _agentId, _paymentId, _regDateTime, _provPaymentId, _statusDateTime);

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
        }
    }
}
