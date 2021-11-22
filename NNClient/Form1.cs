using NetMQ;
using NetMQ.Sockets;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NNClient
{
  
    public partial class Form1 : Form
    {
        private string a;

      
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;|All files(*.*)|*.*";
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                pictureBox1.Image = Image.FromFile(openFileDialog1.FileName);
                label2.Visible = false;
            }
            a = openFileDialog1.FileName;
            // получаем выбранный файл
            textBox1.Text = a;
        }

       
        private void button1_Click(object sender, EventArgs e)
        {

            TcpClient client = new TcpClient(textBox2.Text, 20000);
            using (FileStream inputStream = File.OpenRead(textBox1.Text))
            {
                using (NetworkStream outputStream = client.GetStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(outputStream))
                    {
                        long lenght = inputStream.Length;
                        long totalBytes = 0;
                        int readBytes = 0;
                        byte[] buffer = new byte[2048];
                        writer.Write(Path.GetFileName(textBox1.Text));
                        writer.Write(lenght);
                        do
                        {
                            readBytes = inputStream.Read(buffer, 0, buffer.Length);
                            outputStream.Write(buffer, 0, readBytes);
                            totalBytes += readBytes;
                        } while (client.Connected && totalBytes < lenght);
                    }
                }
            }
            client.Close();
            BeginInvoke(new MethodInvoker(delegate
            {
                int i = 0;
                while (true)
                    try
                    {
                        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(textBox2.Text), 5558);

                        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        // подключаемся к удаленному хосту
                        socket.Connect(ipPoint);
                        string message = "ky";
                        byte[] data = Encoding.Unicode.GetBytes(message);
                        socket.Send(data);

                        // получаем ответ
                        data = new byte[256]; // буфер для ответа
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0; // количество полученных байт

                        do
                        {
                            bytes = socket.Receive(data, data.Length, 0);
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        }
                        while (socket.Available > 0);
                        var Req = builder.ToString();
                        if (Req == "errore")
                        {
                            if (i == 20)
                            {
                                MessageBox.Show("server dont responce, restart server or client");
                            }
                            i++;
                        }
                        else
                        {
                            label3.Text = Req;
                            break;
                        }
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                        Thread.Sleep(3000);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex + "or server dont responce, restart server or client");
                    }

            }));


        }

         
    }
       
    
}
