using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;
namespace uart_recv
{
    public partial class Form1 : Form
    {
        public int dataRate;  // 量化器发数频率
        public int flag = 1;  // 发送指令对应的框（1，2，3）
        System.DateTime currentTime = new System.DateTime();
        BeijingTime beijingtime = new BeijingTime();

        reciveData reciveData = new reciveData();
        compsys compsys = new compsys();
        SerialData serialData = new SerialData();

        StreamWriter sw_data1s;
        StreamWriter sw_data;
        StreamWriter sw_dataHex;

        delegate void UpdateTableFrmEventHandle(bool isinfoTbox, int num, double ax_1s, double ay_1s, double az_1s,
            double wx_1s, double wy_1s, double wz_1s, double lat, double lon, double alt, double pitch, double roll, double yaw, string text = "");
        UpdateTableFrmEventHandle updateTableFrmdata;

        delegate void DrawDataEventHandle(double datax, double datay, double dataz);
        DrawDataEventHandle drawDataEventHandle;

        public System.IO.Ports.Parity[] parity = new System.IO.Ports.Parity[5];
        public Form1()
        {
            InitializeComponent();
            Rectangle ScreenArea = Screen.GetWorkingArea(this);
            if (ScreenArea.Height < this.Height)
            {
                this.Height = ScreenArea.Height;
            }


            comboBox_COM.Items.AddRange(SerialPort.GetPortNames());
            if (comboBox_COM.Items.Count > 0)
            {
                comboBox_COM.SelectedIndex = 0;
                parity[0] = System.IO.Ports.Parity.None;
                parity[1] = System.IO.Ports.Parity.Odd;
                parity[2] = System.IO.Ports.Parity.Even;
                parity[3] = System.IO.Ports.Parity.Mark;
                parity[4] = System.IO.Ports.Parity.Space;
                updateTableFrmdata += new UpdateTableFrmEventHandle(showData);
            }
            else
            {
                // 若计算机未识别任何串口，将禁止采集按钮
                button_openSerialPort.Enabled = false;
                button_closeSerialPort.Enabled = false;
            }


        }

        public void initiate()
        {
            //
            if (dataGridView.Rows.Count > 0)
            {
                dataGridView.Rows.Clear();
            }
            dataRate = Convert.ToInt32(textBox_dataFreq.Text);
            reciveData.receive_count = 0;
            reciveData.count   = 0;
            reciveData.ax      = 0;
            reciveData.ay      = 0;
            reciveData.az      = 0;
            reciveData.wx      = 0;
            reciveData.wy      = 0;
            reciveData.wz      = 0;
            reciveData.a_tem   = 0;
            reciveData.wx_tem  = 0;
            reciveData.wy_tem  = 0;
            reciveData.wz_tem  = 0;

            reciveData.ax_1s     = 0;
            reciveData.ay_1s     = 0;
            reciveData.az_1s     = 0;
            reciveData.wx_1s     = 0;
            reciveData.wy_1s     = 0;
            reciveData.wz_1s     = 0;
            reciveData.a_tem_1s  = 0;
            reciveData.wx_tem_1s = 0;
            reciveData.wy_tem_1s = 0;
            reciveData.wz_tem_1s = 0;


            reciveData.num = 0;
            reciveData.showed_index = 0;


            
            compsys.fogx_bias = 3.318643021;
            compsys.fogy_bias = 10.06184615;
            compsys.fogz_bias = 31.89001078;
            compsys.accx_bias = -2069.02926565978;
            compsys.accy_bias = -2142.90629398741;
            compsys.accz_bias = -1372.88859778185;

            compsys.fog11 = -3.18339442817989e-06; compsys.fog12 = -1.15381224157161e-08; compsys.fog13 = -1.12747006482440e-10;
            compsys.fog21 = 1.69355169770619e-10 ; compsys.fog22 = -2.46814328415990e-06; compsys.fog23 = -7.42947901342422e-11;
            compsys.fog31 = -2.37962362229835e-09; compsys.fog32 = -3.92264337472851e-10; compsys.fog33 = -2.45933997506271e-06;

            compsys.acc11 = 2.26705328775564e-06 ; compsys.acc12 = 3.48865449725969e-09; compsys.acc13 = -2.69941229766415e-10;
            compsys.acc21 = -3.80977180254624e-09; compsys.acc22 = 2.39736672084350e-06; compsys.acc23 = -1.69720412268235e-10;
            compsys.acc31 = -7.69781130743313e-10; compsys.acc32 = 6.86693376403385e-10; compsys.acc33 = -2.39574034505637e-06;

            compsys.fogx_lp_xz = 0;
            compsys.fogy_lp_xz = 0;
            compsys.fogz_lp_xz = 0;
            compsys.accx_lp_xz = 0;
            compsys.accy_lp_xz = 0;
            compsys.accz_lp_xz = 0;
        }


        private void button_openSerialPort_Click(object sender, EventArgs e)
        {
            //
            initiate();
            OpenSerial();
        }
        private void button_closeSerialPort_Click(object sender, EventArgs e)
        {
            serialPort.Close();
            if (sw_data1s != null)
            {
                sw_data1s.Dispose();
                button_openSerialPort.Enabled = false;
            }
            if (sw_data != null)
            {
                sw_data.Dispose();
                button_openSerialPort.Enabled = false;
            }
            if (sw_dataHex != null)
            {
                sw_dataHex.Dispose();
                button_openSerialPort.Enabled = false;
            }

        }

        public void OpenSerial()
        {
            //
            if (serialPort.IsOpen == false)
            {
                serialPort.PortName = comboBox_COM.Text;
                serialPort.BaudRate = Convert.ToInt32(comboBox_bitRate.Text);
                serialPort.DataBits = Convert.ToInt16(comboBox_DataBits.Text);
                serialPort.Parity = parity[comboBox_Parity.SelectedIndex];
                serialPort.StopBits = (System.IO.Ports.StopBits)int.Parse(comboBox_StopBits.Text);
                try
                {
                    serialPort.Open();
                }
                catch (Exception)
                {
                    MessageBox.Show("串口无法打开!");
                    return;
                }

                currentTime = System.DateTime.Now;
                beijingtime.year = currentTime.Year;
                beijingtime.month = currentTime.Month;
                beijingtime.day = currentTime.Day;
                beijingtime.hour = currentTime.Hour;
                beijingtime.min = currentTime.Minute;
                beijingtime.sec = currentTime.Second;
                beijingtime.msec = currentTime.Millisecond;
                beijingtime.time = System.Convert.ToString(beijingtime.year) + "-" + System.Convert.ToString(beijingtime.month) + "-" + System.Convert.ToString(beijingtime.day);
                beijingtime.time = beijingtime.time + "," + System.Convert.ToString(beijingtime.hour) + "-" + System.Convert.ToString(beijingtime.min) + "-" + System.Convert.ToString(beijingtime.sec) + "-" + System.Convert.ToString(beijingtime.msec);
                string SavePath_data1s = Environment.CurrentDirectory + '\\' + beijingtime.time + @"\data1s.txt";
                string SavePath_data = Environment.CurrentDirectory + '\\' + beijingtime.time + @"\data.txt";
                string SavePath_dataHex = Environment.CurrentDirectory + '\\' + beijingtime.time + @"\dataHex.txt";
                Directory.CreateDirectory(Environment.CurrentDirectory + '\\' + beijingtime.time);


                sw_data1s = new StreamWriter(@SavePath_data1s);
                sw_data = new StreamWriter(@SavePath_data);
                sw_dataHex = new StreamWriter(@SavePath_dataHex);
                serialPort.DataReceived += new SerialDataReceivedEventHandler(Serialport_DataReceived);
            }
            else
            {
                MessageBox.Show("串口已打开，无法重复开启！");
            }


        }


     
        private void Serialport_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int n = serialPort.BytesToRead;
            byte[] readBuffer = new byte[n];
            byte[] buf = new byte[n];

            serialPort.Read(readBuffer, 0, n);
            serialData.buffer.AddRange(readBuffer);



            while (serialData.buffer.Count >= 77)
            {
                if (serialData.buffer[0] == 0x55 && serialData.buffer[1] == 0xAA)
                {
                    UInt32 CheckSumA = 0;
                    UInt32 CheckSumB = 0;

                    for (int i = 2; i < 46; i++)
                    {
                        CheckSumA += serialData.buffer[i];
                    }
                    CheckSumB = serialData.buffer[46];
                    if(true) //(CheckSumA & 0xFF) == CheckSumB
                    {
                        serialData.buffer.CopyTo(0, reciveData.command_receive, 0, 77);

                        int startloc = 2;
                        int multiplier = 0;
                        reciveData.wx    = (256 * 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 3] + 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 2] + 256 * reciveData.command_receive[startloc + 4 * multiplier + 1] + reciveData.command_receive[startloc + 4 * multiplier + 0]) / 2097152.0 * 180 / 3.14159; multiplier++;
                        reciveData.wy    = (256 * 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 3] + 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 2] + 256 * reciveData.command_receive[startloc + 4 * multiplier + 1] + reciveData.command_receive[startloc + 4 * multiplier + 0]) / 2097152.0 * 180 / 3.14159; multiplier++;
                        reciveData.wz    = (256 * 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 3] + 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 2] + 256 * reciveData.command_receive[startloc + 4 * multiplier + 1] + reciveData.command_receive[startloc + 4 * multiplier + 0]) / 2097152.0 * 180 / 3.14159; multiplier++;
                        reciveData.ax    = (256 * 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 3] + 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 2] + 256 * reciveData.command_receive[startloc + 4 * multiplier + 1] + reciveData.command_receive[startloc + 4 * multiplier + 0]) / 2097152.0; multiplier++;
                        reciveData.ay    = (256 * 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 3] + 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 2] + 256 * reciveData.command_receive[startloc + 4 * multiplier + 1] + reciveData.command_receive[startloc + 4 * multiplier + 0]) / 2097152.0; multiplier++;
                        reciveData.az    = (256 * 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 3] + 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 2] + 256 * reciveData.command_receive[startloc + 4 * multiplier + 1] + reciveData.command_receive[startloc + 4 * multiplier + 0]) / 2097152.0; multiplier++;
                        reciveData.lat   = (256 * 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 3] + 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 2] + 256 * reciveData.command_receive[startloc + 4 * multiplier + 1] + reciveData.command_receive[startloc + 4 * multiplier + 0]) / 2097152.0; multiplier++;
                        reciveData.lon   = (256 * 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 3] + 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 2] + 256 * reciveData.command_receive[startloc + 4 * multiplier + 1] + reciveData.command_receive[startloc + 4 * multiplier + 0]) / 2097152.0; multiplier++;
                        reciveData.alt   = (256 * 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 3] + 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 2] + 256 * reciveData.command_receive[startloc + 4 * multiplier + 1] + reciveData.command_receive[startloc + 4 * multiplier + 0]) / 2097152.0; multiplier++;
                        reciveData.vx    = (256 * 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 3] + 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 2] + 256 * reciveData.command_receive[startloc + 4 * multiplier + 1] + reciveData.command_receive[startloc + 4 * multiplier + 0]) / 2097152.0; multiplier++;
                        reciveData.vy    = (256 * 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 3] + 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 2] + 256 * reciveData.command_receive[startloc + 4 * multiplier + 1] + reciveData.command_receive[startloc + 4 * multiplier + 0]) / 2097152.0; multiplier++;
                        reciveData.vz    = (256 * 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 3] + 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 2] + 256 * reciveData.command_receive[startloc + 4 * multiplier + 1] + reciveData.command_receive[startloc + 4 * multiplier + 0]) / 2097152.0; multiplier++;
                        reciveData.pitch = (256 * 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 3] + 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 2] + 256 * reciveData.command_receive[startloc + 4 * multiplier + 1] + reciveData.command_receive[startloc + 4 * multiplier + 0]) / 2097152.0; multiplier++;
                        reciveData.roll  = (256 * 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 3] + 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 2] + 256 * reciveData.command_receive[startloc + 4 * multiplier + 1] + reciveData.command_receive[startloc + 4 * multiplier + 0]) / 2097152.0; multiplier++;
                        reciveData.yaw   = (256 * 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 3] + 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 2] + 256 * reciveData.command_receive[startloc + 4 * multiplier + 1] + reciveData.command_receive[startloc + 4 * multiplier + 0]) / 2097152.0; multiplier++;
                        
                        reciveData.exint = (256 * 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 3] + 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 2] + 256 * reciveData.command_receive[startloc + 4 * multiplier + 1] + reciveData.command_receive[startloc + 4 * multiplier + 0]) / 1000.0 * 180 / 3.14159; multiplier++;
                        reciveData.eyint = (256 * 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 3] + 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 2] + 256 * reciveData.command_receive[startloc + 4 * multiplier + 1] + reciveData.command_receive[startloc + 4 * multiplier + 0]) / 1000.0 * 180 / 3.14159; multiplier++;
                        reciveData.ezint = (256 * 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 3] + 256 * 256 * reciveData.command_receive[startloc + 4 * multiplier + 2] + 256 * reciveData.command_receive[startloc + 4 * multiplier + 1] + reciveData.command_receive[startloc + 4 * multiplier + 0]) / 1000.0 * 180 / 3.14159; multiplier++;


                        reciveData.count = reciveData.command_receive[74];

                        reciveData.receive_count++;
                        if (reciveData.receive_count > 2)
                        {
                            reciveData.ax_1s     = reciveData.ax_1s     + reciveData.ax    ;
                            reciveData.ay_1s     = reciveData.ay_1s     + reciveData.ay    ;
                            reciveData.az_1s     = reciveData.az_1s     + reciveData.az    ;
                            reciveData.a_tem_1s  = reciveData.a_tem_1s  + reciveData.a_tem ;
                            reciveData.wx_1s     = reciveData.wx_1s     + reciveData.wx    ;
                            reciveData.wy_1s     = reciveData.wy_1s     + reciveData.wy    ;
                            reciveData.wz_1s     = reciveData.wz_1s     + reciveData.wz    ;
                            reciveData.wx_tem_1s = reciveData.wx_tem_1s + reciveData.wx_tem;
                            reciveData.wy_tem_1s = reciveData.wy_tem_1s + reciveData.wy_tem;
                            reciveData.wz_tem_1s = reciveData.wz_tem_1s + reciveData.wz_tem;



                            if (sw_data != null)
                            {
                                StringBuilder dataString = new StringBuilder(); //写入文件
                                dataString.Append(System.Convert.ToString(reciveData.receive_count));       dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.wx           ));       dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.wy           ));       dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.wz           ));       dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.ax           ));       dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.ay           ));       dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.az           ));       dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.lat          ));       dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.lon          ));       dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.alt          ));       dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.vx           ));       dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.vy           ));       dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.vz           ));       dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.pitch        ));       dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.roll         ));       dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.yaw          ));       dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.exint        ));       dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.eyint        ));       dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.ezint        ));       dataString.Append("    ");
                                
                                dataString.Append(System.Convert.ToString(reciveData.count        ));

                                // dataString.Append("    ");dataString.Append(System.Convert.ToString(reciveData.count));
                                SaveData(sw_data, dataString);
                            }
                            //if (sw_dataHex != null)
                            //{
                            //    StringBuilder dataString = new StringBuilder(); //写入文件
                            //    for (int i = 0; i < 36; i++)
                            //    {
                            //        dataString.Append(System.Convert.ToString(reciveData.command_receive[i], 16));
                            //        dataString.Append("    ");
                            //    }
                            //    SaveData(sw_dataHex, dataString);
                            //}
                        }


                        if ((reciveData.receive_count - 2) % dataRate == 0 && reciveData.receive_count > 2)
                        {
                            // 每1s显示数据、保存1s累加数据(温度求均值)
                            reciveData.num++;     // 时间（s）

                            reciveData.ax_1s     = reciveData.ax_1s     / dataRate;
                            reciveData.ay_1s     = reciveData.ay_1s     / dataRate;
                            reciveData.az_1s     = reciveData.az_1s     / dataRate;
                            reciveData.a_tem_1s  = reciveData.a_tem_1s  / dataRate;
                            reciveData.wx_1s     = reciveData.wx_1s     / dataRate;
                            reciveData.wy_1s     = reciveData.wy_1s     / dataRate;
                            reciveData.wz_1s     = reciveData.wz_1s     / dataRate;
                            reciveData.wx_tem_1s = reciveData.wx_tem_1s / dataRate;
                            reciveData.wy_tem_1s = reciveData.wy_tem_1s / dataRate;
                            reciveData.wz_tem_1s = reciveData.wz_tem_1s / dataRate;



                            int num = reciveData.num;

                            double ax_1s     = reciveData.ax_1s    ;
                            double ay_1s     = reciveData.ay_1s    ;
                            double az_1s     = reciveData.az_1s    ;
                            double a_tem_1s  = reciveData.a_tem_1s ;
                            double wx_1s     = reciveData.wx_1s    ;
                            double wy_1s     = reciveData.wy_1s    ;
                            double wz_1s     = reciveData.wz_1s    ;
                            double wx_tem_1s = reciveData.wx_tem_1s;
                            double wy_tem_1s = reciveData.wy_tem_1s;
                            double wz_tem_1s = reciveData.wz_tem_1s;





                            if (sw_data1s != null)
                            {
                                StringBuilder dataString = new StringBuilder(); //写入文件
                                dataString.Append(System.Convert.ToString(reciveData.num));       dataString.Append("    ");

                                dataString.Append(System.Convert.ToString(reciveData.ax_1s    )); dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.ay_1s    )); dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.az_1s    )); dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.a_tem_1s )); dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.wx_1s    )); dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.wy_1s    )); dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.wz_1s    )); dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.wx_tem_1s)); dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.wy_tem_1s)); dataString.Append("    ");
                                dataString.Append(System.Convert.ToString(reciveData.wz_tem_1s)); dataString.Append("    ");

                                SaveData(sw_data1s, dataString);

                            }
                            reciveData.ax_1s     = 0;
                            reciveData.ay_1s     = 0;
                            reciveData.az_1s     = 0;
                            reciveData.a_tem_1s  = 0;
                            reciveData.wx_1s     = 0;
                            reciveData.wy_1s     = 0;
                            reciveData.wz_1s     = 0;
                            reciveData.wx_tem_1s = 0;
                            reciveData.wy_tem_1s = 0;
                            reciveData.wz_tem_1s = 0;


                            this.BeginInvoke(updateTableFrmdata, false, num, ax_1s, ay_1s, az_1s, wx_1s, wy_1s, wz_1s, reciveData.lat, reciveData.lon, reciveData.alt, reciveData.pitch, reciveData.roll, reciveData.yaw, "");  //


                        }
                        serialData.buffer.RemoveRange(0, 65);
                    }
                    else
                    {
                        // 如果校验和不对，移去一整段
                        serialData.buffer.RemoveRange(0, 65);
                    }
                }
                else//如果帧头不对，移去一个字节
                {
                    serialData.buffer.RemoveRange(0, 1);
                }
            }
        }
        

        public void showData(bool isinfoTbox, int num, double ax_1s, double ay_1s, double az_1s, 
            double wx_1s,double wy_1s, double wz_1s, double lat, double lon, double alt, double pitch, double roll, double yaw, string text = "")
        {

            dataGridView.Rows.Add();
            dataGridView.Rows[reciveData.showed_index].Cells[0].Value = num  .ToString();

            dataGridView.Rows[reciveData.showed_index].Cells[1]. Value = ax_1s     .ToString();
            dataGridView.Rows[reciveData.showed_index].Cells[2]. Value = ay_1s     .ToString();
            dataGridView.Rows[reciveData.showed_index].Cells[3]. Value = az_1s     .ToString();
            dataGridView.Rows[reciveData.showed_index].Cells[4]. Value = wx_1s     .ToString();
            dataGridView.Rows[reciveData.showed_index].Cells[5]. Value = wy_1s     .ToString();
            dataGridView.Rows[reciveData.showed_index].Cells[6]. Value = wz_1s     .ToString();
            dataGridView.Rows[reciveData.showed_index].Cells[7]. Value = lat       .ToString();
            dataGridView.Rows[reciveData.showed_index].Cells[8]. Value = lon       .ToString();
            dataGridView.Rows[reciveData.showed_index].Cells[9]. Value = alt       .ToString();
            dataGridView.Rows[reciveData.showed_index].Cells[10].Value = pitch     .ToString();
            dataGridView.Rows[reciveData.showed_index].Cells[11].Value = roll      .ToString();
            dataGridView.Rows[reciveData.showed_index].Cells[12].Value = yaw       .ToString();








            this.dataGridView.FirstDisplayedScrollingRowIndex = this.dataGridView.Rows.Count - 1;
            reciveData.showed_index++;

        }

        public void SaveData(StreamWriter sw, StringBuilder str)
        {
            sw.WriteLine(str);
        }

     
        

        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct Union
        {
            [FieldOffset(0)]
            public Byte b0; // 低位
            [FieldOffset(1)]
            public Byte b1;
            [FieldOffset(2)]
            public Byte b2;
            [FieldOffset(3)]
            public Byte b3; // 高位
            [FieldOffset(0)]
            public Int32 i;
            [FieldOffset(0)]
            public float f;
        }

        private void button_closeSerialPort_Click_1(object sender, EventArgs e)
        {
            //
            serialPort.Close();
            sw_data.Close();
            sw_dataHex.Close();
            sw_data1s.Close();
        }

        private void button_Config_Click(object sender, EventArgs e)
        {
            Form Form2 = new Form2();
            Form2.ShowDialog();
        }
    }
}

