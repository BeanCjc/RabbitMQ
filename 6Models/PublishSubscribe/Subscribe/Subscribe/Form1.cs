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

namespace Subscribe
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
            var exchangeName = "news";
            channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout, true, false, null);
            var queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queueName, exchangeName, "", null);
            var subscriber = new EventingBasicConsumer(channel);
            subscriber.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                channel.BasicAck(ea.DeliveryTag, false);
                richTextBox1.Invoke(new Action(() => richTextBox1.Text += $"订阅者(队列名：[{queueName}])已收到消息:{message}\r\n"));
            };
            channel.BasicConsume(queueName, false, subscriber);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            channel.Close();
            conn.Close();
        }
    }
}
