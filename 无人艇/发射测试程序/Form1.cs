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

namespace udpreceive
{
    public partial class Form1 : Form
    {
        public  delegate void dele();
        public Thread t; 
        public Form1()
        {
            InitializeComponent();
        }
       
        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;  
            textBox1.Text = string.Empty;
            //webBrowser1.Url = new Uri("http://sherlock99.blog.lc/map.html");
        }
        public void receive()
        {
            //try
            //{
             
                UdpClient udpclient = new UdpClient(9998);
                while (true)
                {
                    if (udpclient.Available > 0)
                    {

                        IPEndPoint ipe = null;
                        byte[] receivebyte = udpclient.Receive(ref ipe);
                        label2 .Text =ipe .Address.ToString ()+ipe .Port .ToString ();
                        string str = Encoding.ASCII.GetString(receivebyte);
                        //char[] sp = { '$' };
                        //    string [] strin=str .Split (sp );
                        //string str2 = "经度："+strin[0]+"   纬度：" +strin[1] ;
                        textBox1.Text += str+ "\r\n";
                    }
                }
            //}
            //catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                t = new Thread(new ThreadStart(receive));
                t.Start();
                button1.Enabled = false;
            }
         catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) { t.Abort(); }
     
    }
}
