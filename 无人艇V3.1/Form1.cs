﻿using System;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;
using System.Timers;
using System.Collections;
namespace whut_ship_control
{
    public partial class Form1 : Form
    {

        #region 全局变量的定义

        //控制指令字符串
        static string control_instruction;

        //第一次接收GPS数据标志
        static bool is_first_receive = true;

        StreamWriter swr = new StreamWriter("E:\\received.txt", true);
        StreamWriter sws = new StreamWriter("E:\\send.txt", true);
        StreamWriter sww_nag;
        StreamReader swr_nag;
        //纬度 经度 构造Javascript函数参数使用此数组
        public static object[] objArray = new object[2];

        //连接计时器
        static int connect_count = 0;

        //丢点计数器
        static int point_counter_for_abandon = 0;

        //TODO
        static double longitude_true = 0;
        static double latitude_true = 0;

        //基站经度校正值
        static double longitude_check =-0.140541040;

        //基站纬度校正值
        static double latitude_check =-0.245311050;

        //TODO
        string GPS_text = "";

        //主串口
        static SerialPort main_sp = new SerialPort();

        //辅助GPS串口
        static SerialPort GPS_sp = new SerialPort();

        //TODO
        bool isopen1;       //端口状态变量

        //避免在事件处理方法中反复的创建，定义到外面
        private StringBuilder builder = new StringBuilder();

        string form_navigation_name = "";

        System.Timers.Timer receive_time = new System.Timers.Timer();
        int time_passed = 0;
        struct latlogtime
        {
            public point latlog;
            public double time;
        };
        public struct point
        {
            public double x;
            public double y;
            public static point operator -(point lhs, point rhs)    //重载-号运算
            {
                point result;
                result.x = lhs.x - rhs.x;
                result.y = lhs.y - rhs.y;
                return result;
            }
        };

        public struct coordinate
        {
            public double x;
            public double y;
        };
        static coordinate[] axis = new coordinate[4];				//四个坐标轴上的单位向量 用初始化函数进行初始化
        Queue latlogtime_queue = new Queue();
        
        #endregion

        //主窗体构造函数
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;        
        }

        //主窗体加载   地图 端口 初始化过程
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                webBrowser1.Navigate("http://99.blog.lc/map_google.html");
                receive_time.Interval = 1;
                receive_time.Elapsed += new System.Timers.ElapsedEventHandler(receive_time_Elapsed);
                init(axis);
                try
                {
                    string[] ports = SerialPort.GetPortNames();
                    Array.Sort(ports);
                    comboBox1.Items.AddRange(ports);
                    comboBox1.SelectedIndex = comboBox1.Items.Count > 0 ? 1 : -1;
                    comboBox2.SelectedIndex = 5;
                    comboBox3.SelectedIndex = 0;
                    comboBox4.SelectedIndex = 3;
                    comboBox5.SelectedIndex = 1;
                    main_sp.DataReceived += comm_DataReceived1;

                    comboBox10.Items.AddRange(ports);
                    comboBox10.SelectedIndex = comboBox10.Items.Count > 0 ? 2 : -1;
                    comboBox9.SelectedIndex = 5;
                    comboBox8.SelectedIndex = 0;
                    comboBox7.SelectedIndex = 3;
                    comboBox6.SelectedIndex = 1;
                    GPS_sp.DataReceived += comm_DataReceived2;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void comm_DataReceived1(object sender, SerialDataReceivedEventArgs e)//串口数据监听器
        {
            connect_count = 0;
            int n = main_sp.BytesToRead;//先记录下来，避免某种原因，人为的原因，操作几次之间时间长，缓存不一致   
            byte[] buf = new byte[n];//声明一个临时数组存储当前来的串口数据     
            main_sp.Read(buf, 0, n);//读取缓冲数据   
            builder.Remove(0, builder.Length);//清除字符串构造器的内容   
            //因为要访问ui资源，所以需要使用invoke方式同步ui。   
            this.Invoke((EventHandler)(delegate
            {       
                    //直接按ASCII规则转换成字符串 
                    string str = Encoding.ASCII.GetString(buf);
                    // builder.Append(str);
                    main_sp_receive(str);
                    point_counter_for_abandon++;
            }));
        }

        private void comm_DataReceived2(object sender, SerialDataReceivedEventArgs e)//串口数据监听器
        {
            GPS_text = GPS_text + GPS_sp.ReadExisting();  // 读取串口数据
            if (GPS_text. Contains  ("$"))      // 如果GPS_text字符串最后一个字符是“回车”&&GPS_text.EndsWith("\n")
            {
                Invoke(new EventHandler(update_data));    // 通过Invoke方法执行update_data函数
            }
            
        }

        private void update_data(object sender, EventArgs e)
        {
            //string[] GPS_info = GPS_text.Split(','); // 按照逗号分隔把$GPRMC各种信号分割到字符串数组

            try
            {
                if (GPS_text.Contains("$GPRMC"))             //TODO: 添加try catch块    
                {
                    string[] str2 = GPS_text.Split('R','M','C','\n');
                    string[] str1 = str2[4].Split(',');
                    Double latitude = Convert.ToDouble(str1[3]) / 100;
                    Double longitude = Convert.ToDouble(str1[5]) / 100;
                    label14.Text = str1[3];
                    label15.Text = str1[5];
                    if (longitude > 1 && latitude > 1)
                    {
                        latitude_check = latitude - latitude_true;
                        longitude_check = longitude - longitude_true;
                    }
                }
                GPS_text = "";
            }
            catch
            {
                return;
            }  // 处理GPS_info字符串数组，完成GPS数据显示、处理等功能
                GPS_text = "";                // 置空GPS_text以便存储新的串口接收到的字符串
        }

        void receive_time_Elapsed(Object sender, ElapsedEventArgs e)
        {
            time_passed += 1;
        }

        public static void init(coordinate[] axis)						//计算速度初始化函数 必须调用一次
        {
            coordinate temp;
            temp.x = 1;
            temp.y = 0;
            axis[0] = temp;		//(1,0)
            temp.x = 0;
            temp.y = 1;
            axis[1] = temp;		//(0,1)
            temp.x = -1;
            temp.y = 0;
            axis[2] = temp;		//(-1,0)
            temp.x = 0;
            temp.y = -1;
            axis[3] = temp;		//(0,-1)
        }

        public static double get_angle(coordinate a, coordinate b)			//计算两个向量的夹角
        {
            double angle = Math.Acos((a.x * b.x + a.y * b.y) / (Math.Sqrt(a.x * a.x + a.y * a.y) * Math.Sqrt(b.x * b.x + b.y * b.y))) / 3.1415926 * 180;
            if (angle < 0.0001)
                angle = 0;
            return angle;
        }

        public static coordinate get_direction(point previous_location, point current_location)
        {
            coordinate temp;
            temp.x = current_location.x - previous_location.x;
            temp.y = current_location.y - previous_location.y;
            return temp;
        }

        //主串口字符串处理函数
        void main_sp_receive(string str)
        {
            try
            {
                if (is_first_receive)       //第一次接收数据
                {
                    is_first_receive = false;
                    timer1.Start();
                    timer2.Start();
                    receive_time.Start();
                    label7.Text = "已连接";
                    label7.BackColor = Color.White;
                }
                else                        //不是第一次接收
                {
                    //显示接收到的数据
                    label12.Text = str;

                    if (str.Contains("$GPRMC") )               //只处理含有$GPRMC的GPS数据&& !str.Contains("GPGSA")
                    {
                        string[] str1 = str.Split(',');
                        string[] th = str1[12].Split('P', 'R', 'H', 'e','S','\r');
                        string humidity = th[4];                //湿度
                        string temperature = th[2];             //温度

                        //温度标签
                        label18.Text = temperature + "摄氏度";
                        progressBar1.Value = Convert.ToInt32(Convert.ToDouble(temperature));

                        //湿度标签
                        label19.Text = humidity + "%";
                        progressBar2.Value = Convert.ToInt32(Convert.ToDouble(humidity));
                        //障碍物标签
                        
                        //红外检测
                        if (str.Contains("!##!"))
                        {
                            label21.Text = "发现目标";
                            label21.BackColor = Color.Red;
                        }
                        else
                        {
                            label21.Text = "正常";
                            label21.BackColor = Color.White;
                        }

                        //纬度转换并计算正确值
                        Double latitude = Convert.ToDouble(str1[3]) / 100 - latitude_check ;

                        //经度转换并计算正确值
                        Double longitude = Convert.ToDouble(str1[5]) / 100 - longitude_check ;


                        /*********************************************************/
                        latlogtime temp;
                        temp.latlog.y = latitude;
                        temp.latlog.x = longitude;
                        temp.time = time_passed;
                        latlogtime_queue.Enqueue(temp);
                        if (latlogtime_queue.Count == 2)
                        {
                            latlogtime temp1 = (latlogtime)latlogtime_queue.Dequeue();
                            latlogtime temp2 = (latlogtime)latlogtime_queue.Dequeue();
                            double distance = Math.Sqrt(Math.Pow(temp1.latlog.x - temp2.latlog.x, 2) + Math.Pow(temp1.latlog.y - temp2.latlog.y, 2));
                            double time = Math.Abs(temp2.time - temp1.time);
                            double speed = distance / time;
                            label33.Text = speed.ToString();
                            coordinate temp_coor = get_direction(temp1.latlog, temp2.latlog);
                            double[] degree = new double[4];
                            for (int i = 0; i < 4; ++i)
                            {
                                degree[i] = get_angle(temp_coor, axis[i]);
                            }
                            if (degree[0] <= 90 && degree[1] <= 90)
                                label35.Text = "北偏东" + degree[1].ToString() + "°";
                            else if (degree[1] <= 90 && degree[2] <= 90)
                                label35.Text = "北偏西" + degree[1].ToString() + "°";
                            else if (degree[2] <= 90 && degree[3] <= 90)
                                label35.Text = "南偏西" + degree[3].ToString() + "°";
                            else if (degree[3] <= 90 && degree[4] <= 90)
                                label35.Text = "南偏东" + degree[3].ToString() + "°";
                        }



                        if (true)           //防止点数量过于密集，每接收三个丢弃一个point_counter_for_abandon % 3 == 0
                        {
                            if (true)//check_ok
                            {
                                label2.Text = latitude.ToString();
                                label3.Text = longitude.ToString();
                                objArray[0] = (object)latitude;
                                objArray[1] = (object)longitude;
                                webBrowser1.Document.InvokeScript("mark", objArray);
                                //check_ok = false;
                            }
                        }
                        if (checkbox1.Checked)
                        {
                            objArray[0] = (object)latitude;
                            objArray[1] = (object)longitude;
                            webBrowser1.Document.InvokeScript("center", objArray);
                        }
                        swr.WriteLine(str);
                    }
                }
                if (str.Contains("DIS"))
                {
                    string[] str3 = str.Split('I', 'S', '$');

                    if (str3[2] == "-1")
                    {
                        label9.Text = "前方无目标";
                        progressBar3.Value=10;
                    }
                    else
                    {
                        label9.Text = Convert.ToDouble(str3[2]) / 100 + "m";
                        progressBar3.Value = Convert.ToInt32(Convert.ToDouble(str3[2]) / 100);
                    }
                }
                    
                if (str.Contains(":") || str.Contains("!"))
                {
                    if (str.Contains(":"))
                    {
                        string[] strr=str.Split(':','$');
                        //order_back(":"+strr[1]);
                    }
                    else if (str.Contains("!"))
                    {
                        string[] strr = str.Split('!','$');
                        //order_back("!" + strr[1]);
                    }
                }
            }
            catch 
            {
                return;
            }               
        }


        private void order_back(string str1)
        {
            switch (str1)
            {
                case "!0": textBox1.Text += "舵角已调整为左5级\r\n"; break;
                case "!1": textBox1.Text += "舵角已调整为左4级\r\n"; break;
                case "!2": textBox1.Text += "舵角已调整为左3级\r\n"; break;
                case "!3": textBox1.Text += "舵角已调整为左2级\r\n"; break;
                case "!4": textBox1.Text += "舵角已调整为左1级\r\n"; break;
                case "!5": textBox1.Text += "舵角已调整为0级\r\n"; break;
                case "!6": textBox1.Text += "舵角已调整为右1级\r\n"; break;
                case "!7": textBox1.Text += "舵角已调整为右2级\r\n"; break;
                case "!8": textBox1.Text += "舵角已调整为右3级\r\n"; break;
                case "!9": textBox1.Text += "舵角已调整为右4级\r\n"; break;
                case "!10": textBox1.Text += "舵角已调整为右5级\r\n"; break;
                case ":OC": textBox1.Text += "油门已调整为：关闭\r\n"; break;
                case ":OO": textBox1.Text += "油门已调整为：打开\r\n"; break;
                case ":WC": textBox1.Text += "抽水机已调整为：关闭\r\n"; break;
                case ":WO": textBox1.Text += "抽水机已调整为：打开\r\n"; break;
                case ":0": textBox1.Text += "速度已调整为0级\r\n"; break;
                case ":1": textBox1.Text += "速度已调整为1级\r\n"; break;
                case ":2": textBox1.Text += "速度已调整为2级\r\n"; break;
                case ":3": textBox1.Text += "速度已调整为3级\r\n"; break;
                case ":4": textBox1.Text += "速度已调整为4级\r\n"; break;
                case ":5": textBox1.Text += "速度已调整为5级\r\n"; break;
                case ":6": textBox1.Text += "速度已调整为6级\r\n"; break;
                case ":7": textBox1.Text += "速度已调整为7级\r\n"; break;
                case ":8": textBox1.Text += "速度已调整为8级\r\n"; break;
                case ":9": textBox1.Text += "速度已调整为9级\r\n"; break;
                case ":10": textBox1.Text += "速度已调整为10级\r\n"; break;
            }
        }       

        #region 串口配置库函数
        private bool CheckPortSetting()      //检查端口是否初始化
        {
            if (comboBox1.Text.Trim() == "") return false;
            if (comboBox2.Text.Trim() == "") return false;
            if (comboBox3.Text.Trim() == "") return false;
            if (comboBox4.Text.Trim() == "") return false;
            if (comboBox5.Text.Trim() == "") return false;
            if (comboBox10.Text.Trim() == "") return false;
            if (comboBox9.Text.Trim() == "") return false;
            if (comboBox8.Text.Trim() == "") return false;
            if (comboBox7.Text.Trim() == "") return false;
            if (comboBox6.Text.Trim() == "") return false;
            return true;
        }

        private bool CheckSendData()        //检测发送数据是否为空
        {
            if (control_instruction == "") return false;
            return true;
        }
        #endregion

        private void SetPortProperty1(SerialPort sp)       //初始化端口状态
        {

            sp.PortName = comboBox1.Text.Trim();
            sp.BaudRate = Convert.ToInt32(comboBox2.Text.Trim());
            float f = Convert.ToSingle(comboBox5.Text.Trim());
            if (f == 0)
            {
                sp.StopBits = StopBits.None;
            }
            else if (f == 1.5)
            {
                sp.StopBits = StopBits.OnePointFive;
            }
            else if (f == 1)
            {
                sp.StopBits = StopBits.One;
            }
            else if (f == 2)
            {
                sp.StopBits = StopBits.Two;
            }
            else
            {
                sp.StopBits = StopBits.One;
            }
            sp.DataBits = Convert.ToInt16(comboBox4.Text.Trim());
            string s = comboBox3.Text.Trim();
            if (s.CompareTo("无") == 0)
            {
                sp.Parity = Parity.None;
            }
            else if (s.CompareTo("奇校验") == 0)
            {
                sp.Parity = Parity.Odd;
            }
            else if (s.CompareTo("偶校验") == 0)
            {
                sp.Parity = Parity.Even;
            }
            else
            {
                sp.Parity = Parity.None;
            }
            sp.ReadTimeout = -1;
            try
            {
                sp.Open();
                isopen1 = true;
            }
            catch (Exception)
            {
                //label8.Text = "打开串口时发生错误";
                MessageBox.Show("打开串口时发生错误");
            }
        }

        private void SetPortProperty2(SerialPort sp)       //初始化端口状态
        {

            sp.PortName = comboBox10.Text.Trim();
            sp.BaudRate = Convert.ToInt32(comboBox9.Text.Trim());
            float f = Convert.ToSingle(comboBox6.Text.Trim());
            if (f == 0)
            {
                sp.StopBits = StopBits.None;
            }
            else if (f == 1.5)
            {
                sp.StopBits = StopBits.OnePointFive;
            }
            else if (f == 1)
            {
                sp.StopBits = StopBits.One;
            }
            else if (f == 2)
            {
                sp.StopBits = StopBits.Two;
            }
            else
            {
                sp.StopBits = StopBits.One;
            }
            sp.DataBits = Convert.ToInt16(comboBox7.Text.Trim());
            string s = comboBox8.Text.Trim();
            if (s.CompareTo("无") == 0)
            {
                sp.Parity = Parity.None;
            }
            else if (s.CompareTo("奇校验") == 0)
            {
                sp.Parity = Parity.Odd;
            }
            else if (s.CompareTo("偶校验") == 0)
            {
                sp.Parity = Parity.Even;
            }
            else
            {
                sp.Parity = Parity.None;
            }
            sp.ReadTimeout = -1;
            try
            {
                sp.Open();
                isopen1 = true;
            }
            catch (Exception)
            {
                //label8.Text = "打开串口时发生错误";
                MessageBox.Show("打开串口时发生错误");
            }
        }

        private void send()//端口发射函数
        {
            //if (!CheckPortSetting())
            //{ MessageBox.Show("串口未设置！", "错误提示"); }
            //if (!CheckSendData())
            //{ MessageBox.Show("请输入要发送的数据！", "错误提示"); }
            //if (!isopen1)
            //{ SetPortProperty1(main_sp); }
            //if (isopen1)
            //{
            //    try
            //    {
            //        main_sp.WriteLine(control_instruction);
            //    }
            //    catch (Exception)
            //    {
            //        //label8.Text = "发送数据时发生错误！";
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("串口未打开！", "错误提示");
            //}
            sws.Write(control_instruction);
            order_back(control_instruction);
            try
            {
                sww_nag.Write(control_instruction);
                sww_nag.Write("\r\n");
            }
            catch {return ;}
        }
        #region 菜单组
        private void gPS校正ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GPS gps = new GPS();
            gps.MessageSent += delegate(object caller, string msg, string msg2)
            {
               latitude_true   = Convert .ToDouble (msg);
               longitude_true  = Convert .ToDouble (msg2);
               //check_ok = false;
            };
            gps.ShowDialog();
        }

        private void gPS开始接收ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (check_ok)
            //{
                if (!isopen1)
                {
                SetPortProperty1(main_sp);
                SetPortProperty2(GPS_sp);
                }
                gPS开始接收ToolStripMenuItem.Enabled = false;
                暂停接收ToolStripMenuItem.Enabled = true;
            //}
            //else { MessageBox .Show ("请先设置GPS校正值")}
        }

        private void 暂停接收ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (isopen1)
                {
                    main_sp.Close();
                    GPS_sp.Close();
                    isopen1 = false;
                }
                gPS开始接收ToolStripMenuItem.Enabled = true;
                暂停接收ToolStripMenuItem.Enabled = false;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
        }
        #endregion

        #region 按钮，等级舵角，速度，电机按钮
        private void button1_Click(object sender, EventArgs e)
        {
            control_instruction = "!0";
            send();
            
            button1.Enabled = false;
            button2.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button7.Enabled = true;
            button8.Enabled = true;
            button9.Enabled = true;
            button10.Enabled = true;
            button11.Enabled = true;
            button12.Enabled = true;
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            control_instruction = "!1";
            send();
            
            button1.Enabled = true;
            button2.Enabled = false;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button7.Enabled = true;
            button8.Enabled = true;
            button9.Enabled = true;
            button10.Enabled = true;
            button11.Enabled = true;
            button12.Enabled = true;
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            control_instruction = "!2";
            send();
            
            button1.Enabled = true;
            button2.Enabled = true;
            button4.Enabled =false ;
            button5.Enabled = true;
            button6.Enabled = true;
            button7.Enabled = true;
            button8.Enabled = true;
            button9.Enabled = true;
            button10.Enabled = true;
            button11.Enabled = true;
            button12.Enabled = true;
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            control_instruction = "!3";
            send();
            
            button1.Enabled = true;
            button2.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = false ;
            button6.Enabled = true;
            button7.Enabled = true;
            button8.Enabled = true;
            button9.Enabled = true;
            button10.Enabled = true;
            button11.Enabled = true;
            button12.Enabled = true;
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            control_instruction = "!4";
            send();
            
            button1.Enabled = true;
            button2.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled =false ;
            button7.Enabled = true;
            button8.Enabled = true;
            button9.Enabled = true;
            button10.Enabled = true;
            button11.Enabled = true;
            button12.Enabled = true;
            
        }

        private void button12_Click(object sender, EventArgs e)
        {
            control_instruction = "!5";
            send();
            
            button1.Enabled = true;
            button2.Enabled = true;
            button4.Enabled = true;
            button5.Enabled =true ;
            button6.Enabled = true;
            button7.Enabled = true;
            button8.Enabled = true;
            button9.Enabled = true;
            button10.Enabled = true;
            button11.Enabled = true;
            button12.Enabled = false ;
            
        }

        private void button7_Click(object sender, EventArgs e)
        {
            control_instruction = "!6";
            send();
            
            button1.Enabled = true;
            button2.Enabled = true ;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button7.Enabled = false ;
            button8.Enabled = true;
            button9.Enabled = true;
            button10.Enabled = true;
            button11.Enabled = true;
            button12.Enabled = true;
            
        }

        private void button8_Click(object sender, EventArgs e)
        {
            control_instruction = "!7";
            send();
            
            button1.Enabled = true;
            button2.Enabled = true ;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button7.Enabled = true;
            button8.Enabled = false ;
            button9.Enabled = true;
            button10.Enabled = true;
            button11.Enabled = true;
            button12.Enabled = true;
            
        }

        private void button9_Click(object sender, EventArgs e)
        {
            control_instruction = "!8";
            send();
            
            button1.Enabled = true;
            button2.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button7.Enabled = true;
            button8.Enabled = true;
            button9.Enabled = false ;
            button10.Enabled = true;
            button11.Enabled = true;
            button12.Enabled = true;
            
        }

        private void button10_Click(object sender, EventArgs e)
        {
            control_instruction = "!9";
            send();
            
            button1.Enabled = true;
            button2.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button7.Enabled = true;
            button8.Enabled = true;
            button9.Enabled = true;
            button10.Enabled = false ;
            button11.Enabled = true;
            button12.Enabled = true;
            
        }

        private void button11_Click(object sender, EventArgs e)
        {
            control_instruction = "!10";
            send();
            
            button1.Enabled = true;
            button2.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button7.Enabled = true;
            button8.Enabled = true;
            button9.Enabled = true;
            button10.Enabled = true;
            button11.Enabled =false ;
            button12.Enabled = true;
            
        }

        private void button13_Click(object sender, EventArgs e)
        {
            control_instruction = ":0";
            send();
            
            button13.Enabled = false ;
            button14.Enabled = true;
            button15.Enabled = true;
            button16.Enabled = true;
            button17.Enabled = true;
            button18.Enabled = true;
            button19.Enabled = true;
            button20.Enabled = true;
            button21.Enabled = true;
            button22.Enabled = true;
            button23.Enabled = true;
            
        }

        private void button23_Click(object sender, EventArgs e)
        {
            control_instruction = ":1";
            send();
            
            button13.Enabled = true;
            button14.Enabled = true;
            button15.Enabled = true;
            button16.Enabled = true;
            button17.Enabled = true;
            button18.Enabled = true;
            button19.Enabled = true;
            button20.Enabled = true;
            button21.Enabled = true;
            button22.Enabled = true;
            button23.Enabled = false ;
            
        }

        private void button22_Click(object sender, EventArgs e)
        {
            control_instruction = ":2";
            send();
            
            button13.Enabled = true;
            button14.Enabled = true;
            button15.Enabled = true;
            button16.Enabled = true;
            button17.Enabled = true;
            button18.Enabled = true;
            button19.Enabled = true;
            button20.Enabled = true;
            button21.Enabled = true;
            button22.Enabled = false ;
            button23.Enabled = true;
            
        }

        private void button21_Click(object sender, EventArgs e)
        {
            control_instruction = ":3";
            send();
            
            button13.Enabled = true;
            button14.Enabled = true;
            button15.Enabled = true;
            button16.Enabled = true;
            button17.Enabled = true;
            button18.Enabled = true;
            button19.Enabled = true;
            button20.Enabled = true;
            button21.Enabled =false ;
            button22.Enabled = true;
            button23.Enabled = true;
            
        }

        private void button20_Click(object sender, EventArgs e)
        {
            control_instruction = ":4";
            send();
            
            button13.Enabled = true;
            button14.Enabled = true;
            button15.Enabled = true;
            button16.Enabled = true;
            button17.Enabled = true;
            button18.Enabled = true;
            button19.Enabled = true;
            button20.Enabled =false ;
            button21.Enabled = true;
            button22.Enabled = true;
            button23.Enabled = true;
            
        }

        private void button19_Click(object sender, EventArgs e)
        {
            control_instruction = ":5";
            send();
            
            button13.Enabled = true;
            button14.Enabled = true;
            button15.Enabled = true;
            button16.Enabled = true;
            button17.Enabled = true;
            button18.Enabled = true;
            button19.Enabled = false ;
            button20.Enabled = true;
            button21.Enabled = true;
            button22.Enabled = true;
            button23.Enabled = true;
            
        }

        private void button18_Click(object sender, EventArgs e)
        {
            control_instruction = ":6";
            send();
            
            button13.Enabled = true;
            button14.Enabled = true;
            button15.Enabled = true;
            button16.Enabled = true;
            button17.Enabled = true;
            button18.Enabled = false ;
            button19.Enabled = true;
            button20.Enabled = true;
            button21.Enabled = true;
            button22.Enabled = true;
            button23.Enabled = true;
            
        }

        private void button17_Click(object sender, EventArgs e)
        {
            control_instruction = ":7";
            send();
            
            button13.Enabled = true;
            button14.Enabled = true;
            button15.Enabled = true;
            button16.Enabled = true;
            button17.Enabled = false ;
            button18.Enabled = true;
            button19.Enabled = true;
            button20.Enabled = true;
            button21.Enabled = true;
            button22.Enabled = true;
            button23.Enabled = true;
            
        }

        private void button16_Click(object sender, EventArgs e)
        {
            control_instruction = ":8";
            send();
            
            button13.Enabled = true;
            button14.Enabled = true;
            button15.Enabled = true;
            button16.Enabled =false ;
            button17.Enabled = true;
            button18.Enabled = true;
            button19.Enabled = true;
            button20.Enabled = true;
            button21.Enabled = true;
            button22.Enabled = true;
            button23.Enabled = true;
            
        }

        private void button15_Click(object sender, EventArgs e)
        {
            control_instruction = ":9";
            send();
            
            button13.Enabled = true;
            button14.Enabled = true;
            button15.Enabled =false ;
            button16.Enabled = true;
            button17.Enabled = true;
            button18.Enabled = true;
            button19.Enabled = true;
            button20.Enabled = true;
            button21.Enabled = true;
            button22.Enabled = true;
            button23.Enabled = true;
            
        }

        private void button14_Click(object sender, EventArgs e)
        {
            control_instruction = ":10";
            send();
            
            button13.Enabled = true;
            button14.Enabled = false;
            button15.Enabled = true;
            button16.Enabled = true;
            button17.Enabled = true;
            button18.Enabled = true;
            button19.Enabled = true;
            button20.Enabled = true;
            button21.Enabled = true;
            button22.Enabled = true;
            button23.Enabled = true;
            
        }

        private void button3_Click(object sender, EventArgs e)//游动机开启
        {
            //control_instruction = ":OO";
            //send();
            //
            //button3.Enabled = false;
            //button24.Enabled = true;
        }

        private void button24_Click(object sender, EventArgs e)//游动机关闭
        {
            control_instruction = ":OC";
            send(); 
            button3.Enabled = true;
            button24.Enabled = false;
            
        }

        private void button25_Click(object sender, EventArgs e)//抽水机开启
        {
            control_instruction = ":WO";
            send();           
            button25.Enabled = false  ;
            button26.Enabled = true  ;
            
        }

        private void button26_Click(object sender, EventArgs e)//抽水机关闭
        {
            control_instruction = ":WC";
            send();          
            button25.Enabled = true ;
            button26.Enabled = false ;
            
        }
        #endregion


        #region 定时器组
        private void timer1_Tick(object sender, EventArgs e)//定时发送@函数
        {
            control_instruction = "@";
            send();
        }
        private void timer2_Tick(object sender, EventArgs e)//是否 连接 判断函数
        {
            connect_count++;
            if (connect_count > 4)
            {
                label7.Text = "失去连接";
                label7.BackColor = Color.Red;
            }
            if (connect_count < 5)
            {
                label7.Text = "已连接";
                label7.BackColor = Color.White;
            }
        }
        #endregion

        #region 地图操作控件

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Value > 20)
                numericUpDown1.Value = 20;
            object[] array = new object[1];
            array[0] = (object)numericUpDown1.Value;
            webBrowser1.Document.InvokeScript("zoom", array);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            change_map();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            change_map();
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            change_map();
        }

        private void change_map()
        {
            if (radioButton1.Checked)
            {
                webBrowser1.Navigate("http://99.blog.lc/map_google.html");
            }
            else if (radioButton2.Checked)
            {
                webBrowser1.Navigate("http://99.blog.lc/map_google_road.html");
            }
            else if (radioButton3.Checked)
            {
                webBrowser1.Navigate("http://99.blog.lc/map_chuanxun.html");
            }
        }

        private void 重载地图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            change_map();
        }

        #endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (main_sp != null)
                    main_sp.Close();
                if (GPS_sp != null)
                    GPS_sp.Close();
                sws.Close();
                swr.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
        }
        // 测试按钮
        private void button27_Click(object sender, EventArgs e)
        {
           main_sp_receive (":10$GPRMC,031015.00,A,3036.78331,N,11421.08548,E,0.101,,020313,,,A*7B,!##!");
        }

        private void button28_Click(object sender, EventArgs e)
        {

            Form_navigation form_navigation = new Form_navigation();
            form_navigation.MessageSent += delegate(object caller, string msg, string msg2)
            {
                form_navigation_name = msg;
                sww_nag = new StreamWriter("E:\\" + form_navigation_name+".txt", false);
            };
            if (button28.Text == "开始学习")
            {
                form_navigation.ShowDialog();
                button28.Text = "结束学习";
                timer3.Start();
                
            }
            else
            { 
                button28.Text = "开始学习"; 
                timer3.Stop();
                sww_nag.Close();
            }
        }

        private void button29_Click(object sender, EventArgs e)
        {
            if (button29.Text == "一键执行")
            {
                openFileDialog1.Title = "选择动作";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    swr_nag = new StreamReader(openFileDialog1.FileName);
                button29.Text = "停止执行";
                timer4.Start();
            }
            else
            {
                button29.Text = "一键执行";
                timer4.Stop();
                swr_nag.Close();
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            sww_nag.Write("navigation\r\n");
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            string str = swr_nag.ReadLine();
            if (str != "navigation")
            {
                control_instruction = str;
                send();
                //label14.Text = str;
            }
            //label15.Text = str;

        }




     

    }
}