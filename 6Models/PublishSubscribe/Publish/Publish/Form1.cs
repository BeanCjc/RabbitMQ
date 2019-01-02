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

namespace Publish
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
            textBox1.TabIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                MessageBox.Show("请输入发布信息","Message Error");
            }
            else
            {
                using (var channel = conn.CreateModel())
                {
                    var exchangeName = "news";
                    channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout, true, false, null);
                    //var propertites = channel.CreateBasicProperties();
                    //propertites.DeliveryMode = 2;
                    var body = Encoding.UTF8.GetBytes(textBox1.Text.Trim());
                    channel.BasicPublish(exchangeName, "", null, body);
                    richTextBox1.Text += $"消息: [{textBox1.Text.Trim()}] 已发布成功!\r\n";
                    textBox1.Clear();
                }
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
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
