using System;
using System.Collections.Concurrent;
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

namespace RPCClient
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
        static readonly IModel channel = conn.CreateModel();
        static readonly BlockingCollection<string> collection = new BlockingCollection<string>();
        static readonly string queueName = "rpc_queue";
        static readonly string replyQueueName = channel.QueueDeclare().QueueName;//使用统一的回调队列，ID不同

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.TabIndex = 0;
            //fib(41);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                MessageBox.Show("输入要发往服务器的消息", "Message Error!");
            }
            else
            {
                var propertites = channel.CreateBasicProperties();
                var correlationId = new Guid().ToString();
                propertites.CorrelationId = correlationId;//回调队列id
                propertites.ReplyTo = replyQueueName;//回调队列名
                var message = textBox1.Text.Trim();
                var body = Encoding.UTF8.GetBytes(message);

                #region 订阅回调消息队列
                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(replyQueueName, false, consumer);
                consumer.Received += (model, ea) =>
                {
                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        var replyBody = ea.Body;
                        var replyMessage = Encoding.UTF8.GetString(replyBody);
                        collection.Add(replyMessage);//先这么写吧
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                };
                #endregion

                //客户端发送消息到远程服务器
                channel.BasicPublish("", queueName, propertites, body);
                richTextBox1.Text += $"客户端消息:[{message}] 已发往远程服务端,客户端等待远程处理完毕!\r\n";
                var replyInfo = collection.Take();//阻塞线程
                richTextBox1.Text += $"服务端已处理完毕，并发回确认的相关消息 [{replyInfo}]\r\n";
                textBox1.Clear();
            }

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(sender, e);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
        private int fib(int n)
        {
            if (n == 0 || n == 1)
            {
                return n;
            }

            return fib(n - 1) + fib(n - 2);
        }
    }
}
