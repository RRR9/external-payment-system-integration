using System;

namespace ZudamalZetMobileServices
{
    struct Payment
    {
        public string PaymentId { get; set; }

        private string _number;
        public string Number
        {
            get
            {
                return _number;
            }
            set
            {
                if (value.Length == 9)
                {
                    _number = "992" + value;
                }
                else if (value.Length == 12)
                {
                    _number = value;
                }
                else
                {
                    throw new ZetMobileBadNumberException("Wrong number");
                }
            }
        }
        public string ProvSum { get; set; }

        private string _regDateTime;
        public string RegDateTime
        {
            get
            {
                return _regDateTime;
            }
            set
            {
                if (!DateTime.TryParse(value, out DateTime dateTime))
                {
                    throw new ZetMobileException("Can not parse RegDateTime from SQL Server");
                }
                _regDateTime = dateTime.ToString("yyyy-MM-ddTHH:mm:ss") + "+00:00";
            }
        }

        private string _statusDateTime;
        public string StatusDateTime
        {
            get
            {
                return _statusDateTime;
            }
            set
            {
                if (!DateTime.TryParse(value, out DateTime dateTime))
                {
                    throw new ZetMobileException("Can not parse StatusDateTime from SQL Server");
                }
                _statusDateTime = dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
            }
        }
        public string ProvPaymentId { get; set; }
        public string AgentId { get; set; }
    }
}
