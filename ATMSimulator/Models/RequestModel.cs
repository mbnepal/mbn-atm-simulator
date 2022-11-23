using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATMSimulator.Models
{
    public class RequestModel
    {
        public string MTI { get; set; } = string.Empty;
        public string PCODE { get; set; } = string.Empty;

        public string txnDateTime = DateTime.UtcNow.ToString("MMddhhmmss");
        public string ACCOUNTNO { get; set; } = string.Empty;
        public string TRACE { get; set; } = string.Empty;
        public string ACQUIRER { get; set; } = string.Empty;
        public string MERCHANTID { get; set; } = string.Empty;
        public string TERMINALID { get; set; } = string.Empty;
        public string TERMINALNAME { get; set; } = string.Empty;
        public string TXNAMOUNT { get; set; } = string.Empty;

        public string LOCALDATE = DateTime.Now.ToString("MMdd");
        
        public string LOCALTIME  = DateTime.Now.ToString("hhmmss");
        public string PAN { get; set; } = string.Empty;
        public string CURRENCYCODE { get; set; } = string.Empty;
        public string REVERSAL_DATA { get; set; } = string.Empty;
        public string TRN_FEE { get; set; } = string.Empty;
        public string RETRIEVALREFNO { get; set; } = string.Empty;
    }
}
