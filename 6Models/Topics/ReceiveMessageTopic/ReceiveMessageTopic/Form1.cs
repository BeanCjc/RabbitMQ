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

namespace ReceiveMessageTopic
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

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboBox1.Text.Trim()))
            {
                MessageBox.Show("请输入正确的主题BindingKey.\r\n路由值规则:以“.”作为单词的分隔符，“*”可以代替1个单词,“#”可以代替0个或者n个单词\r\n样例1:Bike.#\r\n样例2:Biek.Bus.Motorcycle","BindingKey Explain");
            }
            else
            {
                channel.Close();
                channel = conn.CreateModel();
                richTextBox1.Clear();
                var bindingKey = comboBox1.Text.Trim();
                richTextBox1.Text += $"正在接收BindingKey[{bindingKey}]的主题消息...\r\n";
                var exchangeName = "topicExchange";
                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queueName, exchangeName, bindingKey, null);
                var topicConsumer = new EventingBasicConsumer(channel);
                topicConsumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    channel.BasicAck(ea.DeliveryTag, false);
                    richTextBox1.Invoke(new Action(() => richTextBox1.Text += $"已接收BindingKey[{bindingKey}] 的主题消息:{message}\r\n"));
                };
                channel.BasicConsume(queueName, false, topicConsumer);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            channel.Close();
            conn.Close();
        }
    }
}
