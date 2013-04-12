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
                comboBox1.SelectedIndex = 0;
                webBrowser1.Navigate("file:///E:/map.html");
                
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString());} 
        }


        public void udpsend() //UDP发射线程 
        {
                IPEndPoint receiveip = new IPEndPoint(IPAddress.Parse(label9.Text), Convert.ToInt32(label15.Text));//目标IP，目标端口
               byte[] message = Encoding.ASCII .GetBytes(strings);//unicode码编码
                //byte[] message = Encoding.Unicode .GetBytes(strings);
                udpclient.Send(message, message.Length, receiveip);//发射语句
                sws.Write(strings);
        } 
        public void udpreceive()
        {
                while (true)
                {
                    if (udpclient.Available > 0)
                    {
                        IPEndPoint ipe = null;
                        byte[] receivebyte = udpclient.Receive(ref ipe);
                        string str = Encoding.ASCII.GetString(receivebyte);
                        //string str = Encoding.ASCII  .GetString(receivebyte);
                        label9.Text = ipe.Address.ToString();//获得目标ip
                        label15.Text = ipe.Port.ToString();//获得目标端口                  
                        this.Invoke(Udprr,str);
                    }
                 }                   
         }
             void udprr(string str)
            {
                if (mar)
                {
                    label12.Text = str;

                    if (str.Contains("$GPGGA"))
                    {
                        string[] str1 = str.Split(',');
                        string[] th = str1[14].Split('P','R','H','\r');
                        //string humidity = th[3].Substring(0, 4);                //湿度
                        string temperature = th[2];                             //温度 
                        label18.Text = temperature+"摄氏度";
                        //label19.Text = th[4] + "%";
                        //label19.Text = humidity+"%";
                        //Double latitude = Convert.ToDouble(str1[3]) / 100 + 0.24164985;
                        //Double longitude = Convert.ToDouble(str1[5]) / 100 + 0.14801964;
                        Double latitude = Convert.ToDouble(str1[2]) / 100 ;
                        Double longitude = Convert.ToDouble(str1[4]) / 100;
                        label2.Text = longitude.ToString();
                        label3.Text = latitude.ToString();
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
                    button3.Enabled = true;
                    //button4.Enabled = true;
                    hScrollBar1.Enabled = true;
                    timer1.Start();
                }                
            }
                    

        private void button1_Click(object sender, EventArgs e)//开始接收
           {
            udpr = new Thread(udpreceive);
            udpr.IsBackground = true;  
            udpr.Start();          
            button1.Enabled = false;
            button2.Enabled = true;
            udpcheck = !udpcheck;
            }        

        private void button3_Click(object sender, EventArgs e)//发射速度
        {
            strings = ":" + comboBox1.SelectedItem.ToString();
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            strings ="@";
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
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

        private void button2_Click(object sender, EventArgs e)
        {
            udpcheck = !udpcheck;
            udpr.Suspend();
            button1.Enabled = true;
            button2.Enabled = false;
        }

        private void gPS校正ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GPS gps = new GPS();
            gps.ShowDialog();
        }

        private void hScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            strings = "!" + hScrollBar1.Value.ToString();
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
