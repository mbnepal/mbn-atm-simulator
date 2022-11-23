/*
 * 2019-02-15 BG: Responses as mentioned in the document.
 * 2019-03-01 BG: Set Response Code for Echo Message
 * 2021-04-19 BG: Added AuthID output param from SATM_PutInTxnLog Procedure to return it to DE38 Authorization Identity Field.
 *                Added function to validate branch wise licensing.
 * 2021-08-05 BG: 1.0.13 Updated new standard reversal. Used field 90 to capture reversals original data. 
 * 2022-02-22 BG: 1.0.16 Returned DE37 Retrieval Reference Number as requested by SCT Team.
 *                       Passed DE30 Processing Fee to SATM_PutInTxnLog Parameter @PROCESSINGFEE.
 * 2022-03-23 BG: 1.0.7 Returned DE38. Trimed first 2 characters from RESPCODE as SCT Only requires 2 chars, however we use 4 chars in our system                      
 * */


using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MBNATMService.Services
{
    public class Userfunctions 
    {

        int[] clear100 = {  8,9,14,16,17,20,21,22,23,24,25,26,27,28,29,30,31,34,35,36,40,42,43,44,45,46,47,50,52,53,55,56,57,58,59,60,61,62,63,64,65,66,67,68,
                            69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,104,105,106,107,108,109,110,111,112,
                            113,114,115,116,117,118,119,120,121,122,123,124,126,127,128};
        int[] clear200 = {  8,9,14,16,17,20,21,22,23,24,25,26,27,28,29,30,31,34,35,36,40,43,44,45,46,47,50,52,53,55,56,57,58,59,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,
                            87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,104,105,106,107,108,109,110,111,112,113,114,115,116,117,118,119,120,121,122,123,124,126,127,128};

        int[] clear220 = {  8,9,14,16,17,20,21,22,23,24,25,26,27,28,29,30,31,34,35,36,40,44,45,46,47,50,52,53,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,
                            87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,104,105,106,107,108,109,110,111,112,113,114,115,116,117,118,119,120,121,122,123,124,125,126,127,128};

        int[] clear420 = {  8,9,10,14,16,17,20,21,22,23,24,25,26,27,28,29,30,31,34,35,36,40,44,45,46,47,50,51,52,53,55,56,57,58,59,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,
                            87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,104,105,106,107,108,109,110,111,112,113,114,115,116,117,118,119,120,121,122,123,124,125,126,127,128};


        string APP_PATH = AppDomain.CurrentDomain.BaseDirectory;
        string UDL_PATH;
        public string SERVICE_IP;
        public int PORT;
        public bool CheckStartup()
        {
            UDL_PATH = APP_PATH + @"Udl\MBNATMISOService.udl";
            Log(UDL_PATH);
            if (!File.Exists(UDL_PATH))
            {
                Log("UDL file not found");
                return false;
            }

            LoadParameters();
            return true;
        }

        public void LoadParameters()
        {
            OleDbConnection conn = new OleDbConnection("FILE NAME=" + UDL_PATH);
            try
            {
                conn.Open();
                string sql = "Select ATMIPAddress, ATMPort from ATMConfig";
                OleDbCommand command = new OleDbCommand(sql, conn);
                OleDbDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        SERVICE_IP = reader.GetString(0);
                        PORT = reader.GetInt32(1);
                    }
                }
                Log("Service IP:" + SERVICE_IP + ",PORT=" + PORT.ToString());
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        public string CreateMD5Hash(string input)
        {
            // Step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }


        public bool ValidateLicenseKey(string orgName,string br,string licenseKey)
        {
            try
            {
                string preKey = orgName + "ATM" + br + "dcq5hjXhRo";
                string license = CreateMD5Hash(preKey).Substring(0,10);
                return (licenseKey.Equals(license));
            }
            catch
            {
                return false;
            }
        }

        public bool ValidateLicense()
        {
            int brCount=0, validLicenseCount = 0;
            bool validLicense = false;
            OleDbConnection conn = new OleDbConnection("FILE NAME=" + UDL_PATH);
            try
            {
                conn.Open();
                string sql = "Select O.OrgName,M.Br,M.LicenseKey,M.ProductCode from OrgParms O, MBNLicenses M Where M.ProductCode='ATM' ";
                OleDbCommand command = new OleDbCommand(sql, conn);
                OleDbDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        brCount++;
                        if (ValidateLicenseKey(reader.GetString(0), reader.GetString(1), reader.GetString(2)))
                        {
                            validLicenseCount++;
                        }
                        else
                        {
                            Log("License validation failed for branch="+ reader.GetString(1));
                        }
                    }
                }
                validLicense = (brCount > 0) && (brCount == validLicenseCount);  
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return validLicense;
        }

        public void Log(string message)
        {
            if (!Directory.Exists(APP_PATH + "Logs"))
            {
               Directory.CreateDirectory(APP_PATH + "Logs");
            }

            string fileName ="ATMLog_"+ DateTime.Now.ToString("yyyyMMdd")+".txt";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(APP_PATH +@"Logs\"+fileName, true))
            {
                file.WriteLine(DateTime.Now.ToString("hh:mm:ss")+": "+message);
            }
        }

        public string BinToStr(byte[] data)
        {
            string retValue = "";
            for (int i = 0; i < data.Length; i++)
            {
                string hexValue = data[i].ToString("X");
                string value = hexValue;
                if (i < 8 || i >= 24)
                {
                    uint hexInt = Convert.ToUInt32(hexValue, 16);
                    value = Convert.ToChar(hexInt).ToString();

                }
                else
                {
                    if (value.Length == 1)
                        value = "0" + value;
                }
                retValue += value;
            }
            return retValue;
        }

        public string BinToStr1(byte[] data)
        {
            string retValue = "";
            for (int i = 0; i < data.Length; i++)
            {
                string hexValue = data[i].ToString("X");
                string value = hexValue;
                if (true)
                {
                    uint hexInt = Convert.ToUInt32(hexValue, 16);
                    value = Convert.ToChar(hexInt).ToString();

                }
                /*
                else
                {
                    if (value.Length == 1)
                        value = "0" + value;
                }*/
                retValue += value;
            }
            return retValue;
        }


        public string BinToHex(byte[] data)
        {
            string retValue = "";
            for (int i=0;i<data.Length;i++)
            {
                retValue+= data[i].ToString("X");
            }
            return retValue;
        }


        public string ProcessRequest(byte[] data)
        {
            string breakedData = "";
            string responseMessage = null;
            string MTI = "", PAN = "", PCODE = "", TXNDATE = "", TXNTIME = "", ACCOUNTNO = "", TRACE = "", ACQUIRER = "", MERCHANTID = "", TERMINALID = "", TERMINALNAME = "", TXNAMOUNT = "", LOCALDATE = "", LOCALTIME = "",
                   RESPCODE = "", BALAMT = "", DATA = null,CURRENCYCODE="",AUTHID="XXXXXX", REVERSAL_DATA = "",TRN_FEE="",RETRIEVALREFNO= "";
            

            string requestData = BinToStr1(data);
            string processData = requestData.Substring(4, requestData.Length - 4);
            string binData = BinToHex(data);
            ATMLog(requestData, binData);
            Log(processData.Substring(0, 4));
          
            BIM_ISO8583.NET.ISO8583 isoBreaker = new BIM_ISO8583.NET.ISO8583();
            OleDbConnection conn = new OleDbConnection("File Name=" + UDL_PATH);
            string[] message = null;
            try
            {
                message = isoBreaker.Parse(processData);                
                for (int i = 1; i < message.Length - 1; i++)
                {
                    try
                    {                        
                        if (!string.IsNullOrEmpty(message[i]))
                            breakedData +="[" + i.ToString() + "]: " + message[i] + ",";
                    }
                    catch { }
                }
                Log("Request:"+breakedData);

                MTI = message[129];
                PCODE = message[3];
                string txnDateTime = message[7];
                TXNDATE = txnDateTime.Substring(0, 6);
                TXNTIME = txnDateTime.Substring(6, 4);
                ACCOUNTNO = message[102];
                TRACE = message[11];
                ACQUIRER = message[32];
                MERCHANTID = message[18];
                TERMINALID = message[41];
                TERMINALNAME = message[43];
                TXNAMOUNT = message[4];
                LOCALDATE = message[13];
                LOCALTIME = message[12];
                PAN = message[2];
                CURRENCYCODE = message[49];
                REVERSAL_DATA= message[90];
                TRN_FEE = message[30]; //Not implenting now,  D Debit C Credit.
                RETRIEVALREFNO = message[37];

                if (string.IsNullOrEmpty(ACCOUNTNO))
                {
                    ACCOUNTNO = "LORO";
                }

                Log("Requested Parameters>> MTI=" + MTI + ",PCODE" + PCODE + ",TXNDATE=" + TXNDATE + ",TXNTIME=" + TXNTIME + ",ACCOUNTNO=" + ACCOUNTNO + 
                    ",TRACE=" + TRACE + ",ACQUIRER=" + ACQUIRER + ",MERCHANTID=" + MERCHANTID + ",TERMINALID=" + TERMINALID + ",TXNAMOUNT" + TXNAMOUNT + ",LOCALDATE=" +
                    LOCALDATE + ",LOCALTIME=" + LOCALTIME + ",PAN=" + PAN+",TERMINAL NAME="+TERMINALNAME+ ",CURRENCYCODE="+ CURRENCYCODE+ "REVERSAL_MSG="+ REVERSAL_DATA+
                    "PROCESSINGFEE="+TRN_FEE);


                //In reversal case, original data are packed in Field90
                if (MTI.Equals("0420"))
                {
                    /*
                    "MTI:" + REVERSAL_DATA.Substring(0, 4);
                    "TRACE:" + REVERSAL_DATA.Substring(4, 6);
                    "DATE:" + REVERSAL_DATA.Substring(10, 6);
                    "TIME:" + REVERSAL_DATA.Substring(16, 4);
                    "BIN:" + REVERSAL_DATA.Substring(23, 8);
                    */             

                    string REVERSAL_TRACE = REVERSAL_DATA.Substring(4, 6);
                    string REVERSAL_DATE = REVERSAL_DATA.Substring(10, 6);
                    string REVERSAL_TIME = REVERSAL_DATA.Substring(16, 4);
                    //string REVERSAL_BIN = REVERSAL_DATA.Substring(23, 8);

                    TRACE = REVERSAL_TRACE;
                    TXNDATE = REVERSAL_DATE;
                    TXNTIME = REVERSAL_TIME;
                    //ACCOUNTNO = GetOriginalAccount(REVERSAL_TRACE, REVERSAL_DATE);
                }


                if (MTI=="0100" || MTI == "0200" || MTI == "0220" || MTI == "0420" || MTI=="0100")
                {
                    OleDbCommand command = new OleDbCommand("SATM_PutInTxnLog", conn);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@MTI", MTI);
                    command.Parameters.AddWithValue("@PAN", PAN);
                    command.Parameters.AddWithValue("@TXNDATE", TXNDATE);
                    command.Parameters.AddWithValue("@TXNTIME", TXNTIME);
                    command.Parameters.AddWithValue("@ACCOUNTNO", ACCOUNTNO);
                    command.Parameters.AddWithValue("@TRACE", TRACE);
                    command.Parameters.AddWithValue("@ACQUIRER", ACQUIRER);
                    command.Parameters.AddWithValue("@MERCHANTID", MERCHANTID);
                    command.Parameters.AddWithValue("@TERMID", TERMINALID);
                    command.Parameters.AddWithValue("@TERMNAME", TERMINALNAME);
                    command.Parameters.AddWithValue("@TXNAMOUNT", TXNAMOUNT);
                    command.Parameters.AddWithValue("@PCODE", PCODE);
                    command.Parameters.AddWithValue("@LOCALDATE", LOCALDATE);
                    command.Parameters.AddWithValue("@LOCALTIME", LOCALTIME);
                    command.Parameters.AddWithValue("@CURRENCYCODE", CURRENCYCODE);
                    command.Parameters.AddWithValue("@PROCESSINGFEE", TRN_FEE);
                    command.Parameters.AddWithValue("@RETRIEVALREFNO", RETRIEVALREFNO);

                    OleDbParameter outputParamRespCode = new OleDbParameter("@RESPCODE", OleDbType.VarChar, 4);
                    outputParamRespCode.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(outputParamRespCode);

                    OleDbParameter outputParamBalamt = new OleDbParameter("@BALANCE", OleDbType.VarChar, 120);
                    outputParamBalamt.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(outputParamBalamt);

                    OleDbParameter outputParamData = new OleDbParameter("@STMT", OleDbType.VarChar, 350);
                    outputParamData.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(outputParamData);

                    OleDbParameter outputParamAuthId = new OleDbParameter("@AUTHID", OleDbType.VarChar, 6);
                    outputParamAuthId.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(outputParamAuthId);

                    /*
                    OleDbParameter outputParamRetrievalRefNo = new OleDbParameter("@RETRIEVALREFNO", OleDbType.VarChar, 12);
                    outputParamRetrievalRefNo.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(outputParamRetrievalRefNo);
                    */

                  


                    conn.Open();
                    command.ExecuteNonQuery();
                    conn.Close();
                    RESPCODE = outputParamRespCode.Value.ToString().Substring(2,2);
                    BALAMT = outputParamBalamt.Value.ToString();
                    DATA = outputParamData.Value.ToString();
                    AUTHID = outputParamAuthId.Value.ToString();
                }            
            }
            catch (Exception ex)
            {
                Log("Error[ProcessRequest][Entry]:" + ex.Message);
                RESPCODE = "96";
                BALAMT = "0000000000000";
                DATA = null;
            }
            finally
            {
                conn.Close();  
            }

            try
            {
                if (MTI == "0200" || MTI == "0220" || MTI == "0420")
                {            
                    if (MTI == "0100")
                    {
                        MTI = "0110";
                        for (int i = 0; i < clear100.Length; i++)
                        {
                            message[clear100[i]] = null;
                        }                    
                    }

                    if (MTI == "0200")
                    {
                        MTI = "0210";
                        for (int i = 0; i < clear200.Length; i++)
                        {
                            message[clear200[i]] = null;
                        }
                    }

                    if (MTI == "0220")
                    {
                        MTI = "0230";
                        for (int i = 0; i < clear220.Length; i++)
                        {
                            message[clear220[i]] = null;
                        }
                    }

                    if (MTI == "0420")
                    {
                        MTI = "0430";
                        for (int i = 0; i < clear420.Length; i++)
                        {
                            message[clear420[i]] = null;
                        }
                    }

                    Log("RESPCODE=" + RESPCODE + ",BALANCE=" + BALAMT + ",MINISTMT=" + DATA+",AUTHID="+AUTHID+",RETRIEVAL REF NO:"+ RETRIEVALREFNO);
                    message[39] = RESPCODE;
                    message[52] = null;
                    message[38] = AUTHID;
                    message[37] = RETRIEVALREFNO; //Retreival Reference Number
                    //message[38] = AUTHID; //Authorization Id
                    if (RESPCODE == "00" && (MTI == "0210" || MTI== "0110" /*|| MTI == "0430"*/))
                    {
                        Log("Balance"+ BALAMT);
                        message[54] = BALAMT;
                        if (PCODE.Substring(0, 2) == "38") //Mini statement.
                        { 
                            message[48] = DATA;
                            Log("Mini statement:" + DATA);
                        }
                    }
                }
                else if (MTI == "0800")
                {
                    MTI = "0810";
                    message[39] = "00";
                }           

            }catch(Exception e)
            {
                Log(e.Message);
            }

            try
            {
                responseMessage = isoBreaker.Build(message, MTI);
            }
            catch (Exception e)
            {
                Log("Error[ProcessRequest][Build]:" + e.Message);
            }

            breakedData = "";
            for (int i = 1; i < message.Length - 1; i++)
            {
                try
                {

                    if (!string.IsNullOrEmpty(message[i]))
                        breakedData += "[" + i.ToString() + "]: " + message[i] + ",";
                }
                catch { }
            }
            Log("Response:" + breakedData);
            //Log("Raw Response>>" + responseMessage);
            responseMessage = PrepareResponseMsg1(responseMessage);
            return responseMessage;
        }
           
        public static byte[] HexStringToBytes(string hexString)
        {
            if (hexString == null)
                throw new ArgumentNullException("hexString");
            if (hexString.Length % 2 != 0)
                throw new ArgumentException("hexString must have an even length", "hexString");
            var bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                string currentHex = hexString.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(currentHex, 16);
            }
            return bytes;
        }

        public string PrepareResponseMsg(string message)
        {
            try
            {
                string bitmapValues = message.Substring(4, 32);
                string strValue = System.Text.Encoding.UTF8.GetString(HexStringToBytes(bitmapValues));
                message = message.Substring(0, 4) + strValue + message.Substring(36, message.Length - 36);
                string messageLen = message.Length.ToString();
                messageLen = "0000".Substring(0, 4 - messageLen.Length) + messageLen;
                message = messageLen + message;
            }
            catch(Exception ex)
            {
                Log("Error [PrepareResponseMsg:]" + ex.Message);
            }
            return message;            
        }

        public string PrepareResponseMsg1(string message)
        {
            try
            {
                string bitmapValues = message.Substring(4, 32);
              //  string strValue = System.Text.Encoding.UTF8.GetString(HexStringToBytes(bitmapValues));
               // message = message.Substring(0, 4) + strValue + message.Substring(36, message.Length - 36);
                string messageLen = message.Length.ToString();
                messageLen = "0000".Substring(0, 4 - messageLen.Length) + messageLen;
                message = messageLen + message;
            }
            catch (Exception ex)
            {
                Log("Error [PrepareResponseMsg1]:" + ex.Message);
            }
            return message;
        }


        public string GetOriginalAccount(string traceId,string date)
        {
            string accountNo = "";
            OleDbConnection conn = new OleDbConnection("File Name=" + UDL_PATH);
            try
            {
                string sql = "Select Rtrim(R.Acc)+R.Chd from T_ATMTrn A,T_Relacc R Where A.SeqNo = '"+ traceId + "' and Right(Replace(Convert(varchar, A.RecDateTime,102),'.',''),4)= '"+ 
                             date + "' and A.Br = R.Br and A.Acc1 = R.Acc";

                OleDbCommand command = new OleDbCommand(sql, conn);
                conn.Open();
                OleDbDataReader reader=command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        accountNo = reader.GetString(0);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Error[GetOriginalAccount]:" + ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return accountNo;
        }

        public void ATMLog(string message,string bMessage)
        { 
            OleDbConnection conn = new OleDbConnection("File Name=" + UDL_PATH);
            try
            {
                string sql = "Insert into ATMLog(MessageDate, Message, BMessage, ErrorCode) Values(GetDate(),'"+message+"',0x"+bMessage+",'0000')";
                OleDbCommand command = new OleDbCommand(sql, conn);
                conn.Open();
                command.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                Log("Error[ATMLog]:" + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }



    }
}
