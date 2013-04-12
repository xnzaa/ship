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

namespace WindowsFormsApplication2
{
    
    public partial class Form1 : Form
    {
        public Thread udpr,udps;
        public  static string strings;
        UdpClient udpclient = new UdpClient(int.Parse("9998"));
        public static string[] str1;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            Control.CheckForIllegalCrossThreadCalls = false;
            try
            {
                webBrowser1.Navigate("http://sherlock99.blog.lc/map2.php?latitude=39.92&longitude=116.46");
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString());} 
        }



        public void udpsend() //UDP发射线程 ，指令分别为：0--12;
        {
            IPEndPoint receiveip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);//目标IP，目标端口
            UdpClient udpclients = new UdpClient(12346);
            string str = strings;
            byte[] message = Encoding.ASCII.GetBytes(str);//ascii码编码
            udpclients.Send(message, message.Length, receiveip);//发射语句
            udpclients.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            strings = comboBox1.SelectedItem.ToString();
            udps = new Thread(new ThreadStart(udpsend));
            udps.Start();
        }


        public void udpreceive()
        {
                while (true)
                {
                    if (true )
                    {
                        IPEndPoint ipe = null;
                        byte[] receivebyte = udpclient.Receive(ref ipe);
                        label9.Text = ipe.Address.ToString();//获得目标ip
                        string str = Encoding.ASCII.GetString(receivebyte);
                        label12.Text = str;
                        str1 = str.Split(',');
                        Double longittude = Convert.ToDouble(str1[3]) / 100 + 0.24164985; 
                        Double latitude = Convert.ToDouble(str1[5]) / 100 + 0.14801964;
                        label2.Text = longittude.ToString();
                        label3.Text = latitude.ToString();
                        webBrowser1.Navigate("http://sherlock99.blog.lc/map2.php?latitude=" + longittude.ToString() + "&longitude=" + latitude.ToString());
                        //webBrowser1.Document.InvokeScript("mark", new string[] { "30.607301", "114.361389" });//这一句是调用网页标记函数。不能正常运行
                        //udpclient .
                        button3.Enabled  = true;
                    }
                }
        }



        private void button1_Click(object sender, EventArgs e)
        {
            udpr = new Thread(new ThreadStart(udpreceive));
            udpr.Start();
            button1.Enabled = false;
            button2.Enabled = true;            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            udpr.Abort();
            button2.Enabled = false;
            //button1.Enabled = true;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            udpclient.Close();
            //if ("Running" == udpr.ThreadState.ToString()) 
            //udpr.Abort();
            //Thread.Sleep(30);
        }




    }
}
