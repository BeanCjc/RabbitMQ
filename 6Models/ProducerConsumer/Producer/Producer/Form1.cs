using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RabbitMQ.Client;
using System.Text;

namespace Producer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        static ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost", UserName = "Bean", Password = "123456" };
        static IConnection connection = factory.CreateConnection();

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                MessageBox.Show("输入发送内容");
            }
            else
            {
                using (var channel = connection.CreateModel())
                {
                    var queueName = "hello";
                    channel.QueueDeclare(queueName, true, false, false, null);
                    var propertires = channel.CreateBasicProperties();
                    propertires.DeliveryMode = 2;
                    var body = Encoding.UTF8.GetBytes(textBox1.Text.Trim());
                    channel.BasicPublish("", queueName, propertires, body);
                    richTextBox1.Text += $"消息: [{textBox1.Text.Trim()}] 已发送！\r\n";
                    textBox1.Clear();
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            connection.Close();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(sender, e);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.TabIndex = 0;
        }
    }
}
