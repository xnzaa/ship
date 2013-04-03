using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace whut_ship_control
{
    public partial class GPS : Form
    {
        public delegate void SendMessage(object sender, string message, string message2);
        public event SendMessage MessageSent;
        public GPS()
        {
            InitializeComponent();
        }

        private void GPS_Load(object sender, EventArgs e)
        {
            //30.61322767,114.3513717
            textBox1.Text = "30.61314415";
            textBox2.Text = "114.35139584";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageSent != null)
                MessageSent(this, textBox1.Text, textBox2.Text);
            this.Close();
        }
    }
}
