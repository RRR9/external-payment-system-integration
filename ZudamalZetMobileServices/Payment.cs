using System;

namespace ZudamalZetMobileServices
{
    struct Payment
    {
        public string PaymentId { get; set; }
        public string Number
        {
            get
            {
                return Number;
            }
            set
            {
                if (value.Length == 9)
                {
                    Number = "992" + value;
                }
                else
                {
                    throw new ZetMobileException("Wrong number");
                }
            }
        }
        public string ProvSum { get; set; }
        public string RegDateTime
        {
            get
            {
                return RegDateTime;
            }
            set
            {
                if (!DateTime.TryParse(value, out DateTime dateTime))
                {
                    throw new ZetMobileException("Can not parse RegDateTime from SQL Server");
                }
                RegDateTime = dateTime.ToString("yyyy-MM-ddTHH:mm:ss") + "+00:00";
            }
        }
        public string StatusDateTime
        {
            get
            {
                return StatusDateTime;
            }
            set
            {
                if (!DateTime.TryParse(value, out DateTime dateTime))
                {
                    throw new ZetMobileException("Can not parse StatusDateTime from SQL Server");
                }
                StatusDateTime = dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
            }
        }
        public string ProvPaymentId { get; set; }
        public string AgentId { get; set; }
    }
}
