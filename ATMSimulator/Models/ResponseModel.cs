using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATMSimulator.Models
{
    public class ResponseModel
    {
        public bool Status { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public string RESPCODE { get; set; } = string.Empty;    
        public string LedgerBalance { get; set; } = string.Empty;
        public string AvailableBalance { get; set; } = string.Empty;
        public string DATA { get; set; } = string.Empty;
        public string AUTHID { get; set; } = string.Empty;
        public string RETRIEVALREFNO { get; set; } = string.Empty;
    }
}
