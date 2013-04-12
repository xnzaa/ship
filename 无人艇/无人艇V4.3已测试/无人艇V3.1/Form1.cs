using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 全局变量的定义
        /// </summary>
        public static Thread udpr, udps;
        public static string strings;
        UdpClient udpclient = new UdpClient(9998);        
        public static bool mar = false;
        StreamWriter swr = new StreamWriter("E:\\received.txt", true);
        StreamWriter sws = new StreamWriter("E:\\send.txt", true);
        public static object[] objArray = new object[2]; //纬度 经度
        public delegate void delr1(string str); //代理接收1
        public static bool udpcheck = true;
        static int connect_count = 0;
        static int direction_count = 5;//用于键盘控制计数
        static int speed_count = 5;//用于键盘控制计数
        delr1 Udprr;
        public Form1()
        {
            InitializeComponent();
            Udprr = new delr1(udprr);
            Control.CheckForIllegalCrossThreadCalls = false;        
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                webBrowser1.Navigate("file:///E:/map.html");


            }
            catch (Exception ex) { MessageBox.Show(ex.ToString());} 
        }


        public void udpsend() //UDP发射线程 
        {
            IPEndPoint receiveip = new IPEndPoint(IPAddress.Parse(label9.Text), Convert.ToInt32(label15.Text));//目标IP，目标端口
            byte[] message = Encoding.ASCII.GetBytes(strings);
            udpclient.Send(message, message.Length, receiveip);//发射语句
            sws.Write(strings);
        } 
        public void udpreceive()
        {
                while (true)
                {
                    if (udpclient.Available > 0)
                    {
                        connect_count = 0;
                        IPEndPoint ipe = null;
                         byte[] receivebyte = udpclient.Receive(ref ipe);
                        string str = Encoding.ASCII.GetString(receivebyte);;
                        label9.Text = ipe.Address.ToString();//获得目标ip
                        label15.Text = ipe.Port.ToString();//获得目标端口                  
                        this.Invoke(Udprr,str);//从线程的代理
                    }
                 }                   
         }
             void udprr(string str)//主线程
            {
                if (mar)
                {
                    label12.Text = str;

                    if (str.Contains("$GPGGA"))
                    {
                        string[] str1 = str.Split(',');
                        //string[] th = str1[14].Split('P','R','H','\r');
                        //string humidity = th[3].Substring(0, 4);                //湿度
                        //string temperature = th[2];                             //温度 
                        //label18.Text = temperature+"摄氏度";
                        //label19.Text = th[4] + "%";
                        //label19.Text = humidity+"%";
                        //Double latitude = Convert.ToDouble(str1[3]) / 100 + 0.24164985;
                        //Double longitude = Convert.ToDouble(str1[5]) / 100 + 0.14801964;
                        Double latitude = 30.613200;//Convert.ToDouble(str1[2]) / 100 ;
                        Double longitude = 114.351177;//Convert.ToDouble(str1[4]) / 100;
                        label2.Text = longitude.ToString();
                        label3.Text = latitude.ToString();
                       // label21.Text = str1[15];      //红外检测
                        objArray[0] = (object)latitude;
                        objArray[1] = (object)longitude;
                        webBrowser1.Document.InvokeScript("mark", objArray);
                        if (checkbox1.Checked)
                        {
                            objArray[0] = (object)latitude;
                            objArray[1] = (object)longitude;
                            webBrowser1.Document.InvokeScript("center", objArray);
                        }
                        swr.WriteLine(str);
                    }
                }
                else
                {
                    mar = true;
                    timer1.Start();
                    timer2.Start();
                    label7.Text = "已连接";
                    label7.BackColor = Color.White;
                    button1.Enabled = true;
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
                    button23.Enabled = true;
                    //button3.Enabled = true;
                    button24.Enabled = true;                   
                    button25.Enabled = true;
                    //button26.Enabled = true;
                }                
            }
                    
      

        private void timer1_Tick(object sender, EventArgs e)//定时发送@函数
        {
            strings ="@";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
        }
        private void timer2_Tick(object sender, EventArgs e)//是否连接判断函数
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
            if (radioButton1.Checked)
            {
                object[] array = new object[1];
                array[0] = (object)2;
                webBrowser1.Document.InvokeScript("type", array);
            }
            if (radioButton2.Checked)
            {
                object[] array = new object[1];
                array[0] = (object)1;
                webBrowser1.Document.InvokeScript("type", array);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!udpcheck)
            udpr.Abort();
            sws.Close();
            swr.Close();
        }


        private void gPS校正ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GPS gps = new GPS();
            gps.ShowDialog();
        }

        private void gPS开始接收ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            udpr = new Thread(udpreceive);
            udpr.IsBackground = true;
            udpr.Start();
            gPS开始接收ToolStripMenuItem.Enabled = false;
            暂停接收ToolStripMenuItem.Enabled = true;
            udpcheck = !udpcheck;
        }

        private void 暂停接收ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            udpcheck = !udpcheck;
            udpr.Suspend();
            gPS开始接收ToolStripMenuItem.Enabled = true;
            暂停接收ToolStripMenuItem.Enabled = false;
        }





        #region 按钮，等级舵角，速度按钮
        private void button1_Click(object sender, EventArgs e)
        {
            strings = "!0";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = "!1";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = "!2";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = "!3";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = "!4";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = "!5";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = "!6";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = "!7";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = "!8";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = "!9";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = "!10";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = ":0";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = ":1";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = ":2";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = ":3";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = ":4";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = ":5";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = ":6";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = ":7";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = ":8";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = ":9";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
            strings = ":10";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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
        #endregion

        private void button3_Click(object sender, EventArgs e)//游动机开启
        {
            //strings = ":OO";
            //udps = new Thread(new ThreadStart(udpsend));
            //udps.Start();
            //button3.Enabled = false;
            //button24.Enabled = true;
        }

        private void button24_Click(object sender, EventArgs e)//游动机关闭
        {
            strings = ":OC";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
            button3.Enabled = true;
            button24.Enabled = false;
        }

        private void button25_Click(object sender, EventArgs e)//抽水机开启
        {
            strings = ":WO";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
            button25.Enabled = false  ;
            button26.Enabled = true  ;
        }

        private void button26_Click(object sender, EventArgs e)//抽水机关闭
        {
            strings = ":WC";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
            button25.Enabled = true ;
            button26.Enabled = false ;
        }

        //#region //实现键盘控制速度，方向
        //private void Form1_KeyUp(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Up)
        //    {
        //        if (speed_count == 10)
        //            speed_count = speed_count; //防止数据溢出
        //        else
        //            speed_count++;
        //    }
        //    if (e.KeyCode == Keys.Down)
        //    {
        //        if (speed_count == 0)
        //            speed_count = speed_count; //防止数据溢出
        //        else
        //            speed_count--;
        //    }
        //    if (e.KeyCode == Keys.Left)
        //    {
        //        if (direction_count == 0)
        //            direction_count = direction_count;//防止数据溢出
        //        else
        //            direction_count--;
        //    }
        //    if (e.KeyCode == Keys.Right)
        //    {
        //        if (direction_count == 10)
        //            direction_count = direction_count;//防止数据溢出MessageBox.Show("6");
        //        else
        //            direction_count++;
        //    }
        //    switch (speed_count)
        //    {
        //        case 0: button13_Click(sender, e); break;
        //        case 1: button23_Click(sender, e); break;
        //        case 2: button22_Click(sender, e); break;
        //        case 3: button21_Click(sender, e); break;
        //        case 4: button20_Click(sender, e); break;
        //        case 5: button19_Click(sender, e); break;
        //        case 6: button18_Click(sender, e); break;
        //        case 7: button17_Click(sender, e); break;
        //        case 8: button16_Click(sender, e);  break;
        //        case 9: button15_Click(sender, e);  break;
        //        case 10: button14_Click(sender, e);  break;

        //    }
        //    switch (direction_count)
        //    {
        //        case 0: button1_Click(sender, e); break;
        //        case 1: button2_Click(sender, e); break;
        //        case 2: button4_Click(sender, e); break;
        //        case 3: button5_Click(sender, e); break;
        //        case 4: button6_Click(sender, e); break;
        //        case 5: button12_Click(sender, e); break;
        //        case 6: button7_Click(sender, e); break;
        //        case 7: button8_Click(sender, e); break;
        //        case 8: button9_Click(sender, e); break;
        //        case 9: button10_Click(sender, e); break;
        //        case 10: button11_Click(sender, e); break;
        //    }

        //}
        //#endregion

 




    }
}
