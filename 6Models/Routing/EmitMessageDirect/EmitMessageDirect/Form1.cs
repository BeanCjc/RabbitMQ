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

namespace EmitMessageDirect
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        static ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost", UserName = "Bean", Password = "123456" };
        static IConnection conn = factory.CreateConnection();

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            textBox1.TabIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                MessageBox.Show("请输入需要发送的信息","Message Error!");
            }
            else if (string.IsNullOrEmpty(comboBox1.Text.Trim()))
            {
                MessageBox.Show("请选择或输入路由值","RoutingKey Error");
            }
            else
            {
                using (var channel=conn.CreateModel())
                {
                    var exchangeName = "directExchange";
                    channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
                    var routingKey = comboBox1.Text.Trim();
                    var message = textBox1.Text.Trim();
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchangeName, routingKey, null, body);
                    richTextBox1.Text += $"已发布RoutingKey:[{routingKey}]的消息:[{message}]\r\n";
                    textBox1.Clear();
                }
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode==Keys.Enter)
            {
                button1_Click(sender, e);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            conn.Close();
        }
    }
}
