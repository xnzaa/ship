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
    public partial class Form_navigation : Form
    {
        public Form_navigation()
        {
            InitializeComponent();
        }

        public delegate void SendMessage(object sender, string message, string message2);
        public event SendMessage MessageSent;

        private void Form_navigation_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageSent != null)
                MessageSent(this, textBox1.Text, "");
            this.Close();
        }
    }
}
