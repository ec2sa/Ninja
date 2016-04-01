using EC2.ChatEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatWinTest
{
    public partial class Form1 : Form
    {

        protected Chatroom room;

        public Form1()
        {
            InitializeComponent();
        }

        private void show(TextBox tb, ChatResult result)
        {
            foreach (var m in result.Messages)
                tb.AppendText(string.Format("[{0}] {1}: {2}{3}", m.Date.ToString("HH:mm:ss"), m.Author, m.Body, Environment.NewLine));

        }

        private void button4_Click(object sender, EventArgs e)
        {
            room = new Chatroom();
            ChatResult msg = room.Init("testowy", "adminek");
            show(textBox1, msg);
            button1.Tag = msg.Pin;
            timer1.Enabled = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            show(textBox1, room.AddSlot(button1.Tag.ToString()));

        }

        private void button6_Click(object sender, EventArgs e)
        {
            button2.Tag = textBox5.Text;
            show(textBox2, room.Join("USER 1", button2.Tag.ToString()));
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (button1.Tag != null)
                show(textBox1, room.Pull(button1.Tag.ToString()));
            if (button2.Tag != null)
            show(textBox2, room.Pull(button2.Tag.ToString()));
            //if (button3.Tag != null)
            //show(textBox3, room.Pull(button3.Tag.ToString()));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            show(textBox1, room.Push(button1.Tag.ToString(), textBox4.Text));
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            show(textBox2,room.Push(button2.Tag.ToString(), textBox5.Text));
        }

        private void button3_Click(object sender, EventArgs e)
        {
           show(textBox3, room.Push(button3.Tag.ToString(), textBox6.Text));
        }

        private void button7_Click(object sender, EventArgs e)
        {
            button3.Tag = textBox6.Text;
            show(textBox3, room.Join("USER 2", button3.Tag.ToString()));
        }

        private void button8_Click(object sender, EventArgs e)
        {
          
            show(textBox3, room.Pull(button3.Tag.ToString()));
        }
    }

   
   
}
