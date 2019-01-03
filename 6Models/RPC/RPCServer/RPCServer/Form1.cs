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

namespace RPCServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /* RPC的核心其实就是生产者定义回调队列，更重要的是定义请求ID(我将其理解为消息ID)，
         * 消费者收到消息并处理消息后将往回调队列里发送处理完毕等消息，并带上生产者定义的请求ID，最后生产者受到消费者发来的消息，即完成一次远程过程调用
         *
         * 这里的生产者也就是RPC的客户端，消费者也就是RPC的服务端
         */

        static readonly ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost", UserName = "xuanyakeji", Password = "123456" };
        static readonly IConnection conn = factory.CreateConnection();
        static IModel channel = conn.CreateModel();
        static string queueName = "rpc_queue";

        private void Form1_Load(object sender, EventArgs e)
        {
            channel.QueueDeclare(queueName, false, false, false, null);
            channel.BasicQos(0, 3, false);
            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queueName, false, consumer);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var random = new Random();
                richTextBox1.Invoke(new Action(() => richTextBox1.Text += $"已收到客户端发来的消息[{message}]并执行处理中...\r\n"));
                var randomTimeSpan = random.Next(3000, 8000);//延时处理中...
                System.Threading.Thread.Sleep(randomTimeSpan);//模拟耗时处理
                var replyProperties = channel.CreateBasicProperties();
                replyProperties.CorrelationId = ea.BasicProperties.CorrelationId;//将队列id返回去
                var replyMessage = $"客户端消息[{message}]已在服务端处理完毕并返回!";
                var replyBody = Encoding.UTF8.GetBytes(replyMessage);
                channel.BasicPublish("", ea.BasicProperties.ReplyTo, replyProperties, replyBody);
                channel.BasicAck(ea.DeliveryTag, false);
                richTextBox1.Invoke(new Action(() => richTextBox1.Text += $"消息[{message}]已在服务端处理完毕.\r\n"));
            };
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            channel.Close();
            conn.Close();
        }
    }
}
