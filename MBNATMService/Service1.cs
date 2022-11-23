/*
 * MicroBanker Nepal Pvt. Ltd.
 * ATM ISO8583 Service
 * Service to receive and response to ISO request from ATM Switch
 * Bikram Gurung
 * 2019-02-01 
 * 2019-02=06 Bikram: Added Echo 0800 and 0100 MTI's 
 * 2019-02-14 Bikram: Added Clear Codes to clear the few unncessary fields. 
 * 2019-03-01 1.0.9 Bikram: Set Response Code for Echo Message 
 * 2021-04-19 1.0.10: BG: DE38 Issue fixed and License validation added.
 * */

using SimpleTCP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace MBNATMService
{
    public partial class Service1 : ServiceBase
    {
        Services.Userfunctions uf = new Services.Userfunctions();
        SimpleTcpServer server;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (!uf.CheckStartup())
            {
                throw new System.ArgumentException("Error", "Error");
                uf.Log("Startup Failed.Incorrect database connection arragments.");
            }

            if (!uf.ValidateLicense())
            {
                throw new System.ArgumentException("Error", "Error");
                uf.Log("Startup Failed.Invalid License.");
            }

            server = new SimpleTcpServer();
            server.DataReceived += Server_DataReceived;
           
            try
            {
                System.Net.IPAddress ip = System.Net.IPAddress.Parse(uf.SERVICE_IP);
                server.Start(ip, uf.PORT);
            }
            catch(Exception e)
            {
                uf.Log(e.Message);
            }           
            uf.Log("Server started..");
        }

        private void Server_DataReceived(object sender, SimpleTCP.Message e)
        {
            uf.Log(e.MessageString);
            string responseMessage=uf.ProcessRequest(e.Data);
            uf.Log("ResponseMsg:"+responseMessage);
            byte[] myByte = System.Text.ASCIIEncoding.Default.GetBytes(responseMessage);
            e.TcpClient.Client.Send(myByte);
        }

        protected override void OnStop()
        {
            server.Stop();
        }

    }
}
