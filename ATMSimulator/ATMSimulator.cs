using ATMSimulator.Models;
using ATMSimulator.Services;
using ATMSimulator.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ATMSimulator
{
    public partial class ATMSimulator : Form
    {
        private readonly IAtmService _atmService;

        public ATMSimulator(IAtmService atmService)
        {
            InitializeComponent();
            _atmService = atmService;
        }


        private void Btn_Request_Click(object sender, EventArgs e)
        {
            if (!ValidateFields())
            {
                return;
            }
            decimal amount;
            Decimal.TryParse(TB_TxnAmt.Text.Trim(), out amount);
            if (!getMTINumber(Cbo_MTI.Text, out string MTI))
            {
                MessageBox.Show("Select valid MTI");
                return;
            }
            RequestModel request = new RequestModel
            {
                MTI = MTI,
                PCODE = TB_PCODE.Text,
                ACCOUNTNO = TB_AccNo.Text,
                TRACE = TB_Trace.Text,
                ACQUIRER = TB_Accquirer.Text,
                MERCHANTID = TB_MerchantId.Text,
                TERMINALID = TB_TerminalId.Text,
                TERMINALNAME = TB_TerminalName.Text,
                TXNAMOUNT = AmountFormat(amount),
                PAN = TB_PAN.Text,
                CURRENCYCODE = TB_CurrencyCode.Text,
                REVERSAL_DATA = TB_ReversalData.Text,
                TRN_FEE = TB_TRNFee.Text,
                RETRIEVALREFNO = TB_RetrivelRefNo.Text
            };
            var response = _atmService.AtmRequest(request);
            LB_log.Items.Add(response.Message);

            DGV_Reponse.Rows.Add(response.RESPCODE, response.AUTHID, response.RETRIEVALREFNO, response.LedgerBalance, response.AvailableBalance, response.DATA);
            //Clipboard.SetText(response);
        }

        private bool getMTINumber(string Name, out string MTI)
        {
            if (Name == "Enquiry" || Name == "POS" || Name == "Withdrawal" || Name == "Statement")
            {
                MTI = "0200";
                return true;
            }
            else if (Name == "Reversal")
            {
                MTI = "0420";
                return true;
            }
            else
            {
                MTI = "0000";
                return false;
            }
        }
        private bool ValidateFields()
        {
            string output;
            if (!Validator.IsValid(TB_Trace, 6, out output) || !Validator.IsValidNumeric(TB_Trace))
            {
                MessageBox.Show("Enter valid Trace");
                return false;
            }
            else
            {
                TB_Trace.Text = output;
            }
            if (!Validator.IsValid(TB_PCODE, 6, out output) || !Validator.IsValidNumeric(TB_PCODE))
            {
                MessageBox.Show("Enter valid Processing Code");
                return false;
            }
            else
            {
                TB_PCODE.Text = output;
            }
            if (!Validator.IsValid(TB_AccNo, 0, out output))
            {
                MessageBox.Show("Enter valid Account Number");
                return false;
            }
            else
            {
                TB_AccNo.Text = output;
            }
            if (!Validator.IsValid(TB_Accquirer, 10, out output))
            {
                MessageBox.Show("Enter valid Accquirer");
                return false;
            }
            else
            {
                TB_Accquirer.Text = output;
            }
            if (!Validator.IsValid(TB_MerchantId, 4, out output) || !Validator.IsValidNumeric(TB_MerchantId))
            {
                MessageBox.Show("Enter valid Merchant Id");
                return false;
            }
            else
            {
                TB_MerchantId.Text = output;
            }
            if (!Validator.IsValid(TB_TerminalId, 8, out output))
            {
                MessageBox.Show("Enter valid Terminal Id");
                return false;
            }
            else
            {
                TB_TerminalId.Text = output;
            }
            if (!Validator.IsValid(TB_TerminalName, 0, out output))
            {
                MessageBox.Show("Enter valid Terminal Name");
                return false;
            }
            else
            {
                TB_TerminalName.Text = output;
            }
            if (!Validator.IsValid(TB_TxnAmt, 0, out output) || !Validator.IsValidDecimal(TB_TxnAmt))
            {
                MessageBox.Show("Enter valid TXN Amount");
                return false;
            }
            if (!Validator.IsValid(TB_PAN, 19, out output) || !Validator.IsValidLong(TB_PAN))
            {
                MessageBox.Show("Enter valid PAN");
                return false;
            }
            else
            {
                TB_PAN.Text = output;
            }
            if (!Validator.IsValid(TB_CurrencyCode, 0, out output) || !Validator.IsValidNumeric(TB_CurrencyCode))
            {
                MessageBox.Show("Enter valid Currency Code");
                return false;
            }
            else
            {
                TB_CurrencyCode.Text = output;
            }
            //if (!Validator.IsValid(TB_ReversalData, 42) || !Validator.IsValidNumeric(TB_ReversalData))
            //{
            //    MessageBox.Show("Enter valid Reversal Data");
            //    return false;
            //}
            if (!Validator.IsValid(TB_TRNFee, 8, out output))
            {
                MessageBox.Show("Enter valid TRN Fee");
                return false;
            }
            else
            {
                TB_TRNFee.Text = output;
            }
            if (!Validator.IsValid(TB_RetrivelRefNo, 12, out output))
            {
                MessageBox.Show("Enter valid Retrivel Ref no");
                return false;
            }
            else
            {
                TB_RetrivelRefNo.Text = output;
            }
            return true;
        }


        private string AmountFormat(decimal amount)
        {
            var amountString = string.Concat(amount.ToString().Where(char.IsDigit));
            return amountString.PadLeft(12, char.Parse("0"));
        }

        private void Btn_Connect_Click(object sender, EventArgs e)
        {
            string error;
            if (!_atmService.IsConnected())
            {              
                if (ValidateConnectionRequest())
                {
                    if (!_atmService.Connect(TB_IpAddress.Text.Trim(), int.Parse(TB_Port.Text.Trim()), out error))
                    {
                        MessageBox.Show(error);
                        return;
                    }
                    Btn_Connect.Text = "Disconnect";
                    MessageBox.Show("Connected successfully");
                    return;
                }
                else
                {
                    MessageBox.Show("Invalid Entered Ip Address or Port. Connecting to personal ip Address");
                }
                if (!_atmService.Connect(out error))
                {
                    MessageBox.Show(error);
                    return;
                }
              
                MessageBox.Show("Connected successfully");
                
            }
            else
            {
                if (!_atmService.Disconnect(out error))
                {
                    MessageBox.Show(error);
                    return;
                }
                Btn_Connect.Text = "Connect";
                MessageBox.Show("Disconnected");
            }


      
        }

        private bool ValidateConnectionRequest()
        {
            if (Validator.IsValid(TB_IpAddress, 0, out string output) && Validator.IsValid(TB_Port, 0, out string output2) && Validator.IsValidNumeric(TB_Port))
            {
                bool ValidateIP = IPAddress.TryParse(TB_IpAddress.Text, out IPAddress ip);
                if (ValidateIP)
                {
                    return true;
                }
            }
            return false;
        }

        private void Btn_Disconnect_Click(object sender, EventArgs e)
        {
            if (!_atmService.Disconnect(out string err))
            {
                MessageBox.Show(err);
                return;
            }
            MessageBox.Show("Disconnected successfully");
        }

        private void Btn_Echo_Click(object sender, EventArgs e)
        {
            string response = _atmService.EchoRequest();
            LB_log.Items.Add(response);
        }

        private void ATMSimulator_Load(object sender, EventArgs e)
        {
            var dataSource = new List<ComboboxItem>();
            dataSource.Add(new ComboboxItem() { Name = "Enquiry", Value = "310000" });
            dataSource.Add(new ComboboxItem() { Name = "POS", Value = "020000" });
            dataSource.Add(new ComboboxItem() { Name = "Withdrawal", Value = "010000" });
            dataSource.Add(new ComboboxItem() { Name = "Statement", Value = "350000" });
            dataSource.Add(new ComboboxItem() { Name = "Reversal", Value = "010000" });

            this.Cbo_MTI.DataSource = dataSource;
            this.Cbo_MTI.DisplayMember = "Name";
            this.Cbo_MTI.ValueMember = "Value";

            Cbo_MTI.DropDownStyle = ComboBoxStyle.DropDownList;
            TB_PCODE.Text = Cbo_MTI.SelectedValue.ToString();

        }

        private void Btn_Trace_Click(object sender, EventArgs e)
        {
            TB_Trace.Text = TraceGenerator.TraceGen();
        }

        private void Cbo_MTI_SelectedIndexChanged(object sender, EventArgs e)
        {
            TB_PCODE.Text = Cbo_MTI.SelectedValue.ToString();
        }
    }
}
