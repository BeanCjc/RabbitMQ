﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Worker1
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
            richTextBox1.TabIndex = 0;
            var queueName = "task_queue";
            channel.QueueDeclare(queueName, true, false, false, null);
            var propertites = channel.CreateBasicProperties();
            propertites.DeliveryMode = 2;
            channel.BasicQos(0, 2, false);
            var worker1 = new EventingBasicConsumer(channel);
            worker1.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                //richTextBox1.Text += $"已接收消息: [{message}]\r\n";
                System.Threading.Thread.Sleep(5500);
                channel.BasicAck(ea.DeliveryTag, false);
                richTextBox1.Invoke(new Action(() => { richTextBox1.Text += $"Worker1已接收消息: [{message}]\r\n"; }));
            };
            channel.BasicConsume(queueName, false, worker1);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            channel.Close();
            conn.Close();
        }
    }
}
