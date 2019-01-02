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
using RabbitMQ.Client.Events;

namespace ReceiveMessageDirect
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        static ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost", UserName = "Bean", Password = "123456" };
        static IConnection conn = factory.CreateConnection();
        static IModel channel = conn.CreateModel();
        string exchangeName = "directExchange";

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            channel.Close();
            conn.Close();
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboBox1.Text.Trim()))
            {
                MessageBox.Show("请输入正确的BindingKey");
            }
            else
            {
                channel.Close();
                channel = conn.CreateModel();
                richTextBox1.Clear();
                var bindingKey = comboBox1.Text.Trim();
                richTextBox1.Text += $"正在接收BindingKey[{bindingKey}]的消息...\r\n";
                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queueName, exchangeName, bindingKey, null);
                var receiver = new EventingBasicConsumer(channel);
                receiver.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    richTextBox1.Invoke(new Action(() => richTextBox1.Text += $"已接收BindingKey[{bindingKey}] 的消息:{message}\r\n"));
                    channel.BasicAck(ea.DeliveryTag, false);
                };
                channel.BasicConsume(queueName, false, receiver);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
