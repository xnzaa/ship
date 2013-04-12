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
        public Thread udpr,udps;
        public  static string strings;
        UdpClient udpclient = new UdpClient(9998);
        public static string[] str1;
        public static bool mar=false;
        StreamWriter sw = new StreamWriter("C:\\1.txt", true);
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 6;
            Control.CheckForIllegalCrossThreadCalls = false;
            try
            {
               webBrowser1.Navigate("http://sherlock99.blog.lc/map2.php?latitude=39.92&longitude=116.46");
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString());} 
        }


        public void udpsend() //UDP发射线程 ，指令分别为：0--12;
        {
            //try
            //{
            IPEndPoint receiveip = new IPEndPoint(IPAddress.Parse(label9.Text), Convert.ToInt32(label15.Text));//目标IP，目标端口
                string str = strings;
                byte[] message = Encoding.ASCII.GetBytes(str);//ascii码编码
                udpclient.Send(message, message.Length, receiveip);//发射语句
            //}
            //catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
        } 



        public void udpreceive()
        {
                IPEndPoint ipe = null;
                byte[] receivebyte;
                string str;
                Double latitude, longittude;
                while (true)
                {
                    try
                    {
                        if (udpclient.Available > 0)
                        {
                            if (mar)
                            {
                                receivebyte = udpclient.Receive(ref ipe);
                                str = Encoding.ASCII.GetString(receivebyte);
                                label9.Text = ipe.Address.ToString();//获得目标ip
                                label15.Text = ipe.Port.ToString();//获得目标端口
                                label12.Text = str;
                                str1 = str.Split(',');
                                longittude = Convert.ToDouble(str1[3]) / 100 + 0.24164985;
                                latitude = Convert.ToDouble(str1[5]) / 100 + 0.14801964;
                                label2.Text = longittude.ToString();
                                label3.Text = latitude.ToString();
                                //webBrowser1.Navigate("http://sherlock99.blog.lc/map2.php?latitude=" + longittude.ToString() + "&longitude=" + latitude.ToString());
                                //webBrowser1.Document.InvokeScript("mark", new string[] { "30.607301", "114.361389" });//这一句是调用网页标记函数。不能正常运行
                                sw.WriteLine(str);
                            }
                            else
                          {
                            mar = true;
                            button3.Enabled = true;
                            button4.Enabled = true;
                          }
                        }
                        
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
                  }
                   
         }



        private void button1_Click(object sender, EventArgs e)//开始接收
        {
            udpr = new Thread(new ThreadStart(udpreceive));
            udpr.Start();
            button1.Enabled = false;
            button2.Enabled = true;            
        }

        private void button2_Click(object sender, EventArgs e)//结束接收
        {
            timer1.Stop();
            sw.Close();
            udpr.Abort();           
            udpclient.Close(); 
            button2.Enabled = false;
        }
        private void button3_Click(object sender, EventArgs e)//发射速度
        {
            strings = ":" + comboBox1.SelectedItem.ToString();
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
            timer1.Start();
        }

        private void button4_Click(object sender, EventArgs e)//发射舵角
        {
            strings = "!" + comboBox2.SelectedIndex.ToString();
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            strings = "@";// + comboBox1.SelectedItem.ToString();
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

    }
}
