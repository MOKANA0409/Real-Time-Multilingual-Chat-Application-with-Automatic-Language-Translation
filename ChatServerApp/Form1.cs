using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace ChatServerApp
{
    public partial class Form1 : Form
    {
        TcpListener listener;
        bool serverRunning = false;

        static ConcurrentDictionary<TcpClient, Tuple<string, string>> clients =
            new ConcurrentDictionary<TcpClient, Tuple<string, string>>();

        static string apiKey = "fc6d4175d1msh36df35af4dd1694p12f488jsn8e8ce2245b58";
        static string apiUrl = "https://deep-translate1.p.rapidapi.com/language/translate/v2";

        // ✅ Update this connection string based on your SQL instance
        string connStr = @"Data Source=VIJAYAKUMARI\SQLEXPRESS;Initial Catalog=ChatDB;Integrated Security=True";

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!serverRunning)
            {
                Task.Run(() => StartServer());
                AppendText("Server started on port 5000...\r\n");
                serverRunning = true;
            }
        }

        void AppendText(string text)
        {
            if (InvokeRequired)
                Invoke(new Action(() => txtLogs.AppendText(text)));
            else
                txtLogs.AppendText(text);
        }

        async Task<string> TranslateText(string input, string toLang)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-rapidapi-key", apiKey);
                    client.DefaultRequestHeaders.Add("x-rapidapi-host", "deep-translate1.p.rapidapi.com");

                    var body = new
                    {
                        q = input,
                        source = "auto",
                        target = toLang
                    };

                    var content = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(body),
                        Encoding.UTF8,
                        "application/json"
                    );

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    string result = await response.Content.ReadAsStringAsync();

                    JObject json = JObject.Parse(result);
                    var translated = json["data"]?["translations"]?["translatedText"]?.ToString();

                    return translated ?? input;
                }
            }
            catch (Exception ex)
            {
                AppendText("Translation error: " + ex.Message + "\r\n");
                return input;
            }
        }

        async Task HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[2048];

            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string name = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

            bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string language = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

            clients[client] = new Tuple<string, string>(name, language);
            AppendText($"{name} connected ({language})\r\n");

            string joinMsg = $"{name} joined the chat.";
            foreach (var kv in clients)
            {
                if (kv.Key != client)
                {
                    byte[] data = Encoding.UTF8.GetBytes(joinMsg + Environment.NewLine);
                    await kv.Key.GetStream().WriteAsync(data, 0, data.Length);
                }
            }

            while (true)
            {
                try
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    AppendText($"[{name}]: {message}\r\n");

                    foreach (var kv in clients)
                    {
                        string targetLang = kv.Value.Item2;
                        string translated = await TranslateText(message, targetLang);
                        string finalMsg = $"[{name}] {translated}";

                        // ✅ Save to database
                        SaveChatToDatabase(name, message, translated, targetLang);

                        byte[] data = Encoding.UTF8.GetBytes(finalMsg + Environment.NewLine);
                        await kv.Key.GetStream().WriteAsync(data, 0, data.Length);
                    }
                }
                catch
                {
                    break;
                }
            }

            clients.TryRemove(client, out _);
            AppendText($"{name} disconnected.\r\n");
        }

        void StartServer()
        {
            listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Task.Run(() => HandleClient(client));
            }
        }

        // ✅ Store chat messages into SQL Server
        private void SaveChatToDatabase(string sender, string original, string translated, string lang)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "INSERT INTO ChatHistory (SenderName, OriginalMessage, TranslatedMessage, TargetLanguage, SentTime) VALUES (@s, @o, @t, @l, @time)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@s", sender);
                        cmd.Parameters.AddWithValue("@o", original);
                        cmd.Parameters.AddWithValue("@t", translated);
                        cmd.Parameters.AddWithValue("@l", lang);
                        cmd.Parameters.AddWithValue("@time", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                AppendText("Database Error: " + ex.Message + "\r\n");
            }
        }

        // ✅ Load chat history and show in txtLogs
        private void btnLoadHistory_Click(object sender, EventArgs e)
        {
            LoadChatHistory();
        }

        private void LoadChatHistory()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT SenderName, OriginalMessage, TranslatedMessage, TargetLanguage, SentTime FROM ChatHistory ORDER BY SentTime";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        txtLogs.Clear();
                        while (reader.Read())
                        {
                            string sender = reader["SenderName"].ToString();
                            string msg = reader["OriginalMessage"].ToString();
                            string trans = reader["TranslatedMessage"].ToString();
                            string lang = reader["TargetLanguage"].ToString();
                            DateTime time = (DateTime)reader["SentTime"];

                            txtLogs.AppendText($"[{time}] {sender}: {msg} → ({lang}) {trans}\r\n");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppendText("Error loading chat history: " + ex.Message + "\r\n");
            }
        }
    }
}
