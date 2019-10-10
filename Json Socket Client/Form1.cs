using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;

namespace Json_Socket_Client
{
    public partial class Form1 : Form
    {
        Socket socket;
        int[] atto_mVolt = new int[] { 30000, 30000, 30000 };
        int[] atto_mVoltDC = new int[] { 0, 0, 0 };
        int[] atto_mHz = new int[] { 1001000, 1001000, 1001000 };

        List<string> commands = new List<string> {
            //"com.attocube.amc.description.getPositionersList",
            //"com.attocube.amc.description.getControlAmplitude",

            "com.attocube.amc.status.getStatusConnected",       // axis
            "com.attocube.amc.status.getStatusMoving",
            "com.attocube.amc.status.getStatusReference",

            "com.attocube.amc.control.getActorName",
            "com.attocube.amc.control.getActorType",

            //"com.attocube.amc.control.getActorParameters",
            "com.attocube.amc.control.getControlAmplitude",
            "com.attocube.amc.control.getControlFrequency",

            "com.attocube.amc.move.getPosition",
            "com.attocube.amc.control.getReferencePosition",

            "com.attocube.amc.move.getNSteps",
            "com.attocube.amc.move.getControlTargetPosition",

            "com.attocube.amc.move.getControlEotOutputDeactive",
            "com.attocube.amc.control.getControlOutput",

            "com.attocube.amc.control.getControlMove",
            "com.attocube.amc.move.getControlContinousFwd",
            "com.attocube.amc.move.getControlContinousBkwd",

            "com.attocube.amc.control.getControlReferenceAutoUpdate",
            "com.attocube.amc.control.getControlAutoReset",
            "com.attocube.amc.control.getControlTargetRange",
            "com.attocube.amc.control.getControlFixOutputVoltage",

            "com.attocube.amc.rtin.getControlMoveGPIO"
            };

        public Form1()
        {
            InitializeComponent();

            conn_btn.Click += (s, e) =>
            {
                Button btn = (Button)s;
                if (btn.Text == "Connect") { atto_conn(true); btn.Text = "Disconnect"; }
                else { atto_conn(false); btn.Text = "Connect"; }
            };
            set_btn.Click += (s, e) => { send(); };
            check_btn.Click += (s, e) => { atto_check(); };
            init_btn.Click += (s, e) => { atto_init(); };
            calib_btn.Click += (s, e) => { atto_calibrate(); };
            wait_btn.Click += (s, e) => { atto_wait_sample(); };
            lift_btn.Click += (s, e) => { atto_lift_sample(); };
            center_btn.Click += (s, e) => { atto_center_sample(); };
        }

        #region high level atto comm
        private void atto_conn(bool connect)
        {
            if (connect)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect("192.168.1.1", 9090);

                if (socket.Connected) richTextBox1.AppendText("connected..." + "\n");
                else richTextBox1.AppendText("connection fail..." + "\n");
            }
            else
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();

                if (socket.Connected) richTextBox1.AppendText("still connected..." + "\n");
                else richTextBox1.AppendText("disconnected..." + "\n");
            }
        }

        private bool atto_check()
        {
            // ensure all actuators are up
            string comm_check = "com.attocube.amc.status.getStatusConnected";
            bool ok = true;

            for (int i = 0; i < 3; i++)
                if (!str_to_bool(transmit_receive(comm_check, 0, new int[] { i })[1]))
                    ok = false;

            return ok;
        }

        private bool atto_init()
        {
            // initialize params for all actuators. volt, dc level, frequency
            string mvolt_init = "com.attocube.amc.control.setControlAmplitude";
            //string mvoltDC_init = "com.attocube.amc.control.setControlFixOutputVoltage";
            string mHz_init = "com.attocube.amc.control.setControlFrequency";
            bool ok = true;

            for (int i = 0; i < 3; i++)
                if (str_to_int(transmit_receive(mvolt_init, 0, new int[] { i, atto_mVolt[i] })[0]) != 0)
                    ok = false;

            //for (int i = 0; i < 3; i++)
            //    if (str_to_int(transmit_receive(mvoltDC_init, 0, new int[] { i, atto_mVoltDC[i] })[0]) != 0)
            //        ok = false;

            for (int i = 0; i < 3; i++)
                if (str_to_int(transmit_receive(mHz_init, 0, new int[] { i, atto_mHz[i] })[0]) != 0)
                    ok = false;

            return ok;
        }

        private bool atto_calibrate()
        {
            // routine of movements to find absolute positions


            return true;
        }

        private bool atto_wait_sample()
        {
            // routine of movements to position holder safely away from shaft


            return true;
        }

        private bool atto_lift_sample()
        {
            // routine of movements to position holder safely away from shaft


            return true;
        }

        private bool atto_center_sample()
        {
            // routine of movements to position holder at the center of optical axis


            return true;
        }

        #endregion

        private void send()
        {
            int idx = Convert.ToInt32(textBox1.Text);
            int[] axis = new int[] { 2 };

            string[] response = transmit_receive(commands[idx], 0, axis);

            if (response == null) return;

            richTextBox1.AppendText(commands[idx] + "\n");
            foreach (string str in response) richTextBox1.AppendText(str + "\t");
            richTextBox1.AppendText("\n");
        }


        #region low Level atto comm
        private string[] transmit_receive(string method, int id, int[] parameters = null)
        {
            // 1. transmit command
            byte[] cmd_bytes = generate_command(method, id, parameters);
            socket.Send(cmd_bytes);

            // 2. get response
            byte[] buffer = new byte[1024 * 4];
            int readBytes = socket.Receive(buffer);
            MemoryStream memoryStream = new MemoryStream();
            while (readBytes > 0)
            {
                memoryStream.Write(buffer, 0, readBytes);
                if (socket.Available > 0)
                    readBytes = socket.Receive(buffer);
                else break;
            }
            byte[] resp_bytes = memoryStream.ToArray();
            memoryStream.Close();

            // 3. resolve
            string[] response = resolve_response(resp_bytes);
            if (response == null) MessageBox.Show("Error!!!");
            return response;
        }

        private byte[] generate_command(string methods, int id, int[] parameters = null)
        {
            Command cmd = new Command { jsonrpc = "2.0", method = methods, @params = parameters, id = id };
            string cmd_str = JsonConvert.SerializeObject(cmd);
            byte[] cmd_bytes = Encoding.Default.GetBytes(cmd_str);
            return cmd_bytes;
        }

        private string[] resolve_response(byte[] resp_bytes)
        {
            string resp_str = Encoding.Default.GetString(resp_bytes);
            Response response = JsonConvert.DeserializeObject<Response>(resp_str);
            return response.result;
        }

        public class Command
        {
            public string jsonrpc { get; set; }
            public string method { get; set; }
            public int[] @params { get; set; }
            public int id { get; set; }
        }

        public class Response
        {
            public string jsonrpc { get; set; }
            public string[] result { get; set; }
            public int id { get; set; }
        }

        #endregion


        #region general helpers
        private bool str_to_bool(string str)
        {
            return Convert.ToBoolean(str);
        }

        private int str_to_int(string str)
        {
            return Convert.ToInt32(str);
        }

        #endregion

    }



    #region Notes

    #endregion
}
