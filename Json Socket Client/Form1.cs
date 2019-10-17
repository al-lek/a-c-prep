using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

using System.IO;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;

namespace Json_Socket_Client
{
    public partial class Form1 : Form
    {
        Socket socket;
        bool connected = false;
        private static readonly Object _lock = new Object();

        bool[] atto_connected = new bool[] { false, false, false };
        bool[] atto_ref = new bool[] { false, false, false };

        string[] atto_ref_pos = new string[] { "N/A", "N/A", "N/A" };
        string[] atto_positions = new string[] { "0", "0", "0" };
        TextBox[] atto_rdbUI, atto_refUI;

        int[] atto_safe_EOT = new int[] { 30000000, -20000000, -20000000 };
        int[] atto_mVolt = new int[] { 40000, 35000, 35000 };
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

            atto_rdbUI = new TextBox[] { udPos_txtBox, fbPos_txtBox, lrPos_txtBox };
            atto_refUI = new TextBox[] { udRef_txtBox, fbRef_txtBox, lrRef_txtBox };

            conn_btn.Click += (s, e) =>
            {
                Button btn = (Button)s;
                if (btn.Text == "Connect")
                {
                    atto_conn(true); btn.Text = "Disconnect"; connected = true;

                    Thread rdb = new Thread(readback_loop);
                    rdb.Start();
                }
                else { connected = false; atto_conn(false); btn.Text = "Connect"; }
            };

            check_btn.Click += (s, e) => { richTextBox1.AppendText("Check status: " + atto_check() + "\n"); };
            init_btn.Click += (s, e) => { richTextBox1.AppendText("Initialization status: " + atto_init() + "\n"); };
            calib_btn.Click += (s, e) => 
            {
                Thread calib = new Thread(atto_calibrate);
                calib.Start();
            };
            wait_btn.Click += (s, e) => { atto_wait_sample(); };
            lift_btn.Click += (s, e) => { atto_lift_sample(); };
            center_btn.Click += (s, e) => { atto_center_sample(); };


            move_btn.Click += (s, e) =>
            {
                richTextBox1.AppendText("Move: " + atto_move(str_to_int(axis_txt.Text), Convert.ToInt32(str_to_double(pos_txtBox.Text) * 1e3)) + "\n");
            };
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

                atto_check();
                atto_init();
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
            bool ok = true;

            for (int i = 0; i < 3; i++)
                if (!axis_online(i)) ok = false;

            return ok;
        }

        private bool atto_init()
        {
            // initialize params for all actuators. volt, dc level, frequency
            string mvolt_init = "com.attocube.amc.control.setControlAmplitude";
            //string mvoltDC_init = "com.attocube.amc.control.setControlFixOutputVoltage";
            string mHz_init = "com.attocube.amc.control.setControlFrequency";
            string enable_axis = "com.attocube.amc.control.setControlOutput";
            string enable_axis_move = "com.attocube.amc.control.setControlMove";

            bool ok = true;

            for (int i = 0; i < 3; i++)
                if (str_to_int(transmit_receive(mvolt_init, 0, new object[] { i, atto_mVolt[i] })[0]) != 0)
                    ok = false;

            //for (int i = 0; i < 3; i++)
            //    if (str_to_int(transmit_receive(mvoltDC_init, 0, new int[] { i, atto_mVoltDC[i] })[0]) != 0)
            //        ok = false;

            for (int i = 0; i < 3; i++)
                if (str_to_int(transmit_receive(mHz_init, 0, new object[] { i, atto_mHz[i] })[0]) != 0)
                    ok = false;

            for (int i = 0; i < 3; i++)
                if (str_to_int(transmit_receive(enable_axis, 0, new object[] { i, true })[0]) != 0)
                    ok = false;

            for (int i = 0; i < 3; i++)
            {
                atto_move(i, 0);
                if (str_to_int(transmit_receive(enable_axis_move, 0, new object[] { i, true })[0]) != 0)
                    ok = false;
            }
            return ok;
        }

        private void atto_calibrate()
        {
            MessageBox.Show("Make sure sample arm is not near sapmle holder!!!");

            // reset refs????
            atto_ref = new bool[] { false, false, false };

            // routine of movements to find absolute positions
            for (int i = 0; i < 3; i++)
            {
                // 1. start moving towards a safe direction
                atto_move(i, atto_safe_EOT[i]);

                while (true)
                {
                    // 2. check if reference is crossed
                    if (atto_ref[i])
                    {
                        Console.WriteLine("found ref!");
                        atto_enable_axis(i, false); break;
                    }

                    // 3. if you find end of travel point move to the oposite direction
                    if (atto_end_of_travel(i)) atto_move(i, -atto_safe_EOT[i]);
                }

                // 4. goto reference mark and set this as the new zero
                //atto_move(i, str_to_int(atto_ref_pos[i]));

                Console.WriteLine(atto_ref_pos[i]);

                // 5. set this position as zero
                axis_ref_reset(i);

                atto_move(i, 0);
                atto_enable_axis(i, true);
            }
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


        #region mid-level atto comm
        private void readback_loop()
        {
            // mail readback loop
            int i_error = -1;

            while (connected)
            {
                try
                {
                    // ensure all actuators are up
                    for (int i = 0; i < 3; i++)
                    {
                        // 1. check actuators connection
                        if (!axis_online(i)) { i_error = i; break; }

                        // 2. check cloosed loop position
                        atto_positions[i] = axis_rbd(i);

                        // 3. check if reference is crossed and get value
                        atto_ref[i] = axis_ref(i);

                        // 4. get reference position
                        if (atto_ref[i]) atto_ref_pos[i] = axis_ref_rbd(i);
                    }
                }
                catch { Console.WriteLine("readback loop ex!"); }

                rdb_to_UI();
                Thread.Sleep(50);
            }
            if (i_error > -1) MessageBox.Show("Actuator: " + i_error + " has lost connection!\nCheck power and cabling. Terminating connection with attocubes!");
        }

        private void rdb_to_UI()
        {
            // connected
            for (int i = 0; i < 3; i++)
            {
                atto_rdbUI[i].Invoke((Action)(()=> { atto_rdbUI[i].Text = (Convert.ToDouble(atto_positions[i]) * 1e-3).ToString("0.#"); }));
                if (atto_ref_pos[i] != "N/A")
                    atto_refUI[i].Invoke((Action)(() => { atto_refUI[i].Text = (Convert.ToDouble(atto_ref_pos[i]) * 1e-3).ToString("0.#"); }));
            }

        }

        #endregion

        #region low Level atto comm
        private bool axis_online(int axis)
        {
            string comm_check = "com.attocube.amc.status.getStatusConnected";
            return str_to_bool(transmit_receive(comm_check, 0, new object[] { axis })[1]);
        }

        private string axis_rbd(int axis)
        {
            string position_rdb = "com.attocube.amc.move.getPosition";
            return transmit_receive(position_rdb, 0, new object[] { axis })[1];
        }

        private bool axis_ref(int axis)
        {
            string ref_rdb = "com.attocube.amc.status.getStatusReference";
            return str_to_bool(transmit_receive(ref_rdb, 0, new object[] { axis })[1]);
        }

        private string axis_ref_rbd(int axis)
        {
            string ref_pos_rdb = "com.attocube.amc.control.getReferencePosition";
            return transmit_receive(ref_pos_rdb, 0, new object[] { axis })[1];
        }

        private bool atto_move(int axis, int pos)
        {
            string move_to_pos = "com.attocube.amc.move.setControlTargetPosition";
            bool ok = true;

            if (str_to_int(transmit_receive(move_to_pos, 0, new object[] { axis, pos })[0]) != 0)
                ok = false;

            return ok;
        }

        private bool atto_end_of_travel(int axis)
        {
            string end_of_travel_fwd = "com.attocube.amc.status.getStatusEotFwd";
            string end_of_travel_bkwd = "com.attocube.amc.status.getStatusEotBkwd";
            bool eot_reached_f = false;
            bool eot_reached_b = false;

            eot_reached_f = str_to_bool(transmit_receive(end_of_travel_fwd, 0, new object[] { axis })[1]);
            eot_reached_b = str_to_bool(transmit_receive(end_of_travel_bkwd, 0, new object[] { axis })[1]);

            //Console.WriteLine("f:" + eot_reached_f.ToString() + " b:" + eot_reached_b.ToString());

            return eot_reached_f | eot_reached_b;
        }

        private void atto_get_eotSetinngs(int axis)
        {
            string get_eotSetting = "com.attocube.amc.move.getControlEotOutputDeactive";
            bool output = false;

            output = str_to_bool(transmit_receive(get_eotSetting, 0, new object[] { axis })[1]);

        }

        private bool atto_enable_axis(int axis, bool enable)
        {
            string enable_axis = "com.attocube.amc.control.setControlMove";
            bool ok = true;

            if (str_to_int(transmit_receive(enable_axis, 0, new object[] { axis, enable })[0]) != 0)
                ok = false;

            return ok;
        }

        private int axis_ref_reset(int axis)
        {
            string ref_reset = "com.attocube.amc.control.setReset";
            return str_to_int(transmit_receive(ref_reset, 0, new object[] { axis })[0]);
        }

        

        private string[] transmit_receive(string method, int id, object[] parameters = null)
        {
            lock (_lock)
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
        }

        private byte[] generate_command(string methods, int id, object[] parameters = null)
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
            public object[] @params { get; set; }
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

        private double str_to_double(string str)
        {
            return Convert.ToDouble(str);
        }

        #endregion

    }



    #region Notes
    //int starting_pos = str_to_int(atto_positions[i]);
    //int j = 0;
    //int step = 100000;

    //while (true)
    //{
    //    if (atto_ref[i])
    //    {
    //        Console.WriteLine("found ref!");
    //        atto_enable_axis(i, false); break;
    //    }

    //    j++;
    //    int next_pos = starting_pos + j * step;

    //    atto_move(i, next_pos);
    //    Thread.Sleep(250);

    //    int dx = Math.Abs(next_pos - str_to_int(atto_positions[i]));
    //    Console.WriteLine("dx: " + dx.ToString());

    //    if (dx > 500)
    //    {
    //        Console.WriteLine("eot found!");
    //        starting_pos = str_to_int(atto_positions[i]);
    //        j = 0;
    //        step = -step;
    //    }                    
    //}

    #endregion
}
