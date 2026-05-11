using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatClientApp
{
    public partial class Form1 : Form
    {
        TcpClient client;
        NetworkStream stream;

        public Form1()
        {
            InitializeComponent();
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                string name = txtName.Text.Trim();
                string lang = txtLang.Text.Trim();

                client = new TcpClient();
                await client.ConnectAsync("127.0.0.1", 5000);
                stream = client.GetStream();

                byte[] nameData = Encoding.UTF8.GetBytes(name);
                await stream.WriteAsync(nameData, 0, nameData.Length);

                byte[] langData = Encoding.UTF8.GetBytes(lang);
                await stream.WriteAsync(langData, 0, langData.Length);

                txtChat.AppendText("Connected to server.\r\n");
                Task.Run(() => ReceiveMessages());
            }
            catch (Exception ex)
            {
                txtChat.AppendText("Error: " + ex.Message + "\r\n");
            }
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                string msg = txtMessage.Text.Trim();
                if (string.IsNullOrEmpty(msg)) return;

                byte[] data = Encoding.UTF8.GetBytes(msg);
                await stream.WriteAsync(data, 0, data.Length);
                txtMessage.Clear();
            }
            catch
            {
                txtChat.AppendText("Disconnected from server.\r\n");
            }
        }

        private void ReceiveMessages()
        {
            byte[] buffer = new byte[2048];
            int bytes;

            while (true)
            {
                try
                {
                    bytes = stream.Read(buffer, 0, buffer.Length);
                    if (bytes == 0) break;

                    string msg = Encoding.UTF8.GetString(buffer, 0, bytes);
                    AppendText(msg);
                }
                catch
                {
                    AppendText("Connection lost.\r\n");
                    break;
                }
            }
        }

        void AppendText(string text)
        {
            if (InvokeRequired)
                Invoke(new Action(() => txtChat.AppendText(text + "\r\n")));
            else
                txtChat.AppendText(text + "\r\n");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
