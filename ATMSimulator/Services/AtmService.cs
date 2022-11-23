using ATMSimulator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleTCP;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace ATMSimulator.Services
{
    public interface IAtmService
    {
        string EchoRequest();
        ResponseModel AtmRequest(RequestModel request);
        bool Connect(out string error);
        bool Connect(string ipAddress, int port, out string error);
        bool Disconnect(out string error);
        bool IsConnected();
    }
    public class AtmService : IAtmService
    {
        private ResponseModel responseModel = new ResponseModel{};
        private readonly SimpleTcpClient _client = new SimpleTcpClient();
        private readonly string  ipAddress= GetIPAddress.GetLocalIPAddress();
        private const int port = 12182;
        private bool _connected = false;


        public bool IsConnected()
        {
            return _connected;
        }

        public string EchoRequest()
        {
            if (true)
            {
                string EchoRequest = "00390800822000000000000004000000000000002009071404000001301";
                if (!SendRequest(EchoRequest, out string response))
                {
                    return "error";
                }
                return response;
            }
            else
            {
                return "Not connected with server";
            }
        }
        public ResponseModel AtmRequest(RequestModel request)
        {
            if (_connected)
            {
                BIM_ISO8583.NET.ISO8583 isoBreaker = new BIM_ISO8583.NET.ISO8583();
                string[] Message = new string[130];
                Message[1] = "0000000004000000";
                Message[2] = request.PAN;
                Message[3] = request.PCODE;
                Message[4] = request.TXNAMOUNT;
                Message[6] = "000000200000"; // EquivalentAmount n 12
                Message[7] = request.txnDateTime;
                Message[11] = request.TRACE;
                Message[12] = request.LOCALTIME;
                Message[13] = request.LOCALDATE;
                Message[14] = "2602"; //ExpirationDate n 4
                Message[15] = "0501"; //SettlementDate n 4
                Message[18] = request.MERCHANTID;
                Message[22] = "051"; //POSEntryMode n 3
                Message[25] = "02"; //POSConditionCode n 2
                Message[30] = request.TRN_FEE;
                Message[32] = request.ACQUIRER;
                Message[35] = "6234777105002003363=26022202990000000";  //Track 2 Data z ..37
                Message[37] = request.RETRIEVALREFNO;
                Message[41] = request.TERMINALID;
                Message[42] = "1007053"; //CardAcceptorIDCode ans 15
                Message[43] = request.TERMINALNAME;
                Message[49] = request.CURRENCYCODE;
                Message[51] = "524"; //CardHolderCurrencyCode n 3
                Message[52] = "A6AD06BF1DBC6B8D"; //PIN b 64
                //Message[90] = request.REVERSAL_DATA;
                Message[102] = request.ACCOUNTNO;
                Message[129] = request.MTI;

                string NewISOmsg = isoBreaker.Build(Message, request.MTI);
                NewISOmsg = NewISOmsg.Length.ToString().PadLeft(4, char.Parse("0")) + NewISOmsg;
                if (!SendRequest(NewISOmsg, out string response))
                {
                    responseModel.Message = "Error";
                    return responseModel;
                }
                getModel(response);

                return responseModel;

            }
            responseModel.Message = "Connect to the server";
            return responseModel;

        }

        private void getModel(string data)
        {
            BIM_ISO8583.NET.ISO8583 isoBreaker = new BIM_ISO8583.NET.ISO8583();
            string processData = data.Substring(4, data.Length - 4);
            string[] Message = isoBreaker.Parse(processData);
            responseModel = new ResponseModel
            {
                RESPCODE = Message[39],
                AUTHID = Message[38],
                DATA = Message[48],
                RETRIEVALREFNO = Message[37],
                Status = true,
                Message = "Success"
            };
            string balamt = Message[54];
            if (balamt == null)
            {
                responseModel.Status = false;
                return;
            }
            if (balamt.Length == 40)
            {
                var firstBalance = balamt.Substring(0,20);
                var SecondBalance = balamt.Substring(20,20);
                string legderBalance = string.Empty;
                string availableBalance = string.Empty;
                var Pos3to4 = firstBalance.Substring(2,2);
                if (Pos3to4 == "01")
                {
                    legderBalance = getBalance(firstBalance);
                    availableBalance = getBalance(SecondBalance);
                }
                else if (Pos3to4 == "02")
                {
                    availableBalance = getBalance(firstBalance);
                    legderBalance = getBalance(SecondBalance);
                }
                responseModel.LedgerBalance = legderBalance;
                responseModel.AvailableBalance = availableBalance;
            }
            else
            {
                try
                {
                    var Balance = balamt.Substring(0, 20);

                    var Pos3to4 = Balance.Substring(2, 2);
                    var outputBalance = getBalance(Balance);
                    if (Pos3to4 == "01")
                    {
                        responseModel.LedgerBalance = outputBalance;
                    }
                    else
                    {
                        responseModel.AvailableBalance = outputBalance;
                    }
                }
                catch
                {
                    responseModel.LedgerBalance = "00000000000";
                    responseModel.AvailableBalance = "00000000000";
                }
            }
        }

        private string getBalance(string balamt)
        {
            var creditDebit =  balamt.Substring(7,1);
            var amount =  balamt.Substring(8,12);
            amount = amount.TrimStart(new Char[] { '0' });
            amount = amount.Insert(amount.Length - 2, ".");
            if (creditDebit == "C")
            {
                return amount;
            }
            else
            {
                return "-" + amount;
            }
        }

        private bool SendRequest(string data, out string response)
        {
       
            response = string.Empty;
            TimeSpan timeout = new TimeSpan(60,30,00);
            try
            {
                var reply = _client.WriteLineAndGetReply(data, timeout);               
                if (reply != null)
                {
                    response = reply.MessageString;
                    return true;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
                return false;
            }
            return false;
        }

        public bool Connect(out string error)
        {
            error = string.Empty;
            try
            {
                _client.Connect(ipAddress, port);
                _connected = true;            
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _connected = false;
            }
            return _connected;
        }

        public bool Connect(string ipAddress, int port, out string error)
        {
            error = string.Empty;
            try
            {
                _client.Connect(ipAddress, port);
                _connected = true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _connected = false;
            }
            return _connected;
        }

        public bool Disconnect(out string error)
        {
            error = string.Empty;
            try
            {
                _client.Disconnect();
                _connected = false;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

  


    }
}
