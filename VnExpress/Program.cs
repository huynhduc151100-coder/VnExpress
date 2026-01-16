using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace VnExpress
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public class MainForm : Form
        {
            private ComboBox cboCategory;
            private Button btnLoad;
            private ListBox lstArticles;
            private TextBox txtDescription;
            private TextBox txtLink;
            private PictureBox picArticle;
            private Label lblStatus;
            private Panel pnlTop;
            private Panel pnlBottom;
            private Label lblImageStatus;

            private string[] rssFeeds = new string[]
            {
            "https://vnexpress.net/rss/tin-moi-nhat.rss",
            "https://vnexpress.net/rss/thoi-su.rss",
            "https://vnexpress.net/rss/the-gioi.rss",
            "https://vnexpress.net/rss/kinh-doanh.rss",
            "https://vnexpress.net/rss/giai-tri.rss",
            "https://vnexpress.net/rss/the-thao.rss",
            "https://vnexpress.net/rss/phap-luat.rss",
            "https://vnexpress.net/rss/giao-duc.rss",
            "https://vnexpress.net/rss/suc-khoe.rss",
            "https://vnexpress.net/rss/gia-dinh.rss",
            "https://vnexpress.net/rss/du-lich.rss",
            "https://vnexpress.net/rss/khoa-hoc.rss",
            "https://vnexpress.net/rss/so-hoa.rss",
            "https://vnexpress.net/rss/oto-xe-may.rss"
            };

            private string[] categoryNames = new string[]
            {
            "Tin mới nhất", "Thời sự", "Thế giới", "Kinh doanh",
            "Giải trí", "Thể thao", "Pháp luật", "Giáo dục",
            "Sức khỏe", "Gia đình", "Du lịch", "Khoa học",
            "Số hóa", "Ô tô xe máy"
            };

            public MainForm()
            {
                InitializeComponents();
            }

            private void InitializeComponents()
            {
                // Form settings
                this.Text = "VnExpress RSS Reader - Raw Socket";
                this.Size = new Size(1200, 750);
                this.StartPosition = FormStartPosition.CenterScreen;

                // Top Panel
                pnlTop = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 60,
                    Padding = new Padding(10)
                };

                Label lblCategory = new Label
                {
                    Text = "Chủ đề:",
                    Location = new Point(10, 18),
                    AutoSize = true
                };

                cboCategory = new ComboBox
                {
                    Location = new Point(70, 15),
                    Width = 200,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cboCategory.Items.AddRange(categoryNames);
                cboCategory.SelectedIndex = 0;

                btnLoad = new Button
                {
                    Text = "Tải tin tức",
                    Location = new Point(280, 14),
                    Width = 100,
                    Height = 25
                };
                btnLoad.Click += BtnLoad_Click;

                lblStatus = new Label
                {
                    Text = "Sẵn sàng",
                    Location = new Point(390, 18),
                    AutoSize = true,
                    ForeColor = Color.Green
                };

                pnlTop.Controls.AddRange(new Control[] { lblCategory, cboCategory, btnLoad, lblStatus });

                // ListBox for articles
                lstArticles = new ListBox
                {
                    Location = new Point(10, 70),
                    Width = 700,
                    Height = 600,
                    Font = new Font("Segoe UI", 9)
                };
                lstArticles.SelectedIndexChanged += LstArticles_SelectedIndexChanged;

                // Right Panel for details
                Panel pnlRight = new Panel
                {
                    Location = new Point(720, 70),
                    Width = 460,
                    Height = 600,
                    BorderStyle = BorderStyle.FixedSingle
                };

                // PictureBox for image
                picArticle = new PictureBox
                {
                    Location = new Point(10, 10),
                    Width = 440,
                    Height = 250,
                    BorderStyle = BorderStyle.FixedSingle,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.LightGray
                };

                lblImageStatus = new Label
                {
                    Location = new Point(10, 265),
                    Width = 440,
                    Height = 20,
                    Text = "",
                    Font = new Font("Segoe UI", 8),
                    ForeColor = Color.Gray
                };

                Label lblDesc = new Label
                {
                    Text = "Mô tả:",
                    Location = new Point(10, 290),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };

                txtDescription = new TextBox
                {
                    Location = new Point(10, 310),
                    Width = 440,
                    Height = 150,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    ReadOnly = true,
                    Font = new Font("Segoe UI", 9)
                };

                Label lblLnk = new Label
                {
                    Text = "Link:",
                    Location = new Point(10, 470),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };

                txtLink = new TextBox
                {
                    Location = new Point(10, 490),
                    Width = 440,
                    ReadOnly = true,
                    Font = new Font("Segoe UI", 8)
                };

                Button btnOpenLink = new Button
                {
                    Text = "Mở trong trình duyệt",
                    Location = new Point(10, 520),
                    Width = 150,
                    Height = 30
                };
                btnOpenLink.Click += BtnOpenLink_Click;

                pnlRight.Controls.AddRange(new Control[] {
                picArticle, lblImageStatus, lblDesc, txtDescription, lblLnk, txtLink, btnOpenLink
            });

                // Add controls to form
                this.Controls.AddRange(new Control[] { pnlTop, lstArticles, pnlRight });
            }

            private void BtnLoad_Click(object sender, EventArgs e)
            {
                lstArticles.Items.Clear();
                txtDescription.Clear();
                txtLink.Clear();
                picArticle.Image = null;
                lblImageStatus.Text = "";

                lblStatus.Text = "Đang tải...";
                lblStatus.ForeColor = Color.Orange;
                Application.DoEvents();

                string selectedRss = rssFeeds[cboCategory.SelectedIndex];
                string rssContent = GetRSSContent(selectedRss);

                if (!string.IsNullOrEmpty(rssContent))
                {
                    ParseAndDisplayRSS(rssContent);
                    lblStatus.Text = $"Đã tải {lstArticles.Items.Count} bài viết";
                    lblStatus.ForeColor = Color.Green;
                }
                else
                {
                    lblStatus.Text = "Lỗi khi tải tin tức";
                    lblStatus.ForeColor = Color.Red;
                    MessageBox.Show("Không thể tải nội dung RSS. Vui lòng thử lại!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            private void LstArticles_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (lstArticles.SelectedItem != null)
                {
                    ArticleItem item = (ArticleItem)lstArticles.SelectedItem;
                    txtDescription.Text = item.Description;
                    txtLink.Text = item.Link;

                    // Load image
                    LoadImage(item.ImageUrl);
                }
            }

            private void LoadImage(string imageUrl)
            {
                picArticle.Image = null;
                lblImageStatus.Text = "";

                if (string.IsNullOrEmpty(imageUrl))
                {
                    lblImageStatus.Text = "Không có hình ảnh";
                    lblImageStatus.ForeColor = Color.Gray;
                    return;
                }

                try
                {
                    lblImageStatus.Text = "Đang tải hình ảnh...";
                    lblImageStatus.ForeColor = Color.Orange;
                    Application.DoEvents();

                    using (WebClient webClient = new WebClient())
                    {
                        webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                        byte[] imageBytes = webClient.DownloadData(imageUrl);

                        using (var ms = new System.IO.MemoryStream(imageBytes))
                        {
                            picArticle.Image = Image.FromStream(ms);
                            lblImageStatus.Text = $"Hình ảnh: {imageUrl}";
                            lblImageStatus.ForeColor = Color.Green;
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblImageStatus.Text = $"Lỗi tải hình: {ex.Message}";
                    lblImageStatus.ForeColor = Color.Red;
                }
            }

            private void BtnOpenLink_Click(object sender, EventArgs e)
            {
                if (!string.IsNullOrEmpty(txtLink.Text))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(txtLink.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Không thể mở link: {ex.Message}", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            private string GetRSSContent(string url)
            {
                try
                {
                    Uri uri = new Uri(url);
                    string host = uri.Host;
                    string path = uri.PathAndQuery;
                    int port = uri.Port;
                    bool isHttps = uri.Scheme.ToLower() == "https";

                    if (port == 80 && isHttps)
                        port = 443;

                    if (isHttps)
                    {
                        using (var client = new System.Net.Http.HttpClient())
                        {
                            client.Timeout = TimeSpan.FromSeconds(15);
                            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                            var httpResponse = client.GetAsync(url).Result;
                            return httpResponse.Content.ReadAsStringAsync().Result;
                        }
                    }

                    // Raw socket cho HTTP
                    IPHostEntry hostEntry = Dns.GetHostEntry(host);
                    IPAddress ipAddress = hostEntry.AddressList[0];
                    IPEndPoint endPoint = new IPEndPoint(ipAddress, port);

                    using (Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                    {
                        socket.ReceiveTimeout = 15000;
                        socket.Connect(endPoint);

                        StringBuilder requestBuilder = new StringBuilder();
                        requestBuilder.Append($"GET {path} HTTP/1.1\r\n");
                        requestBuilder.Append($"Host: {host}\r\n");
                        requestBuilder.Append("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64)\r\n");
                        requestBuilder.Append("Accept: application/rss+xml, application/xml, text/xml, */*\r\n");
                        requestBuilder.Append("Connection: close\r\n");
                        requestBuilder.Append("\r\n");

                        byte[] requestBytes = Encoding.UTF8.GetBytes(requestBuilder.ToString());
                        socket.Send(requestBytes);

                        StringBuilder responseBuilder = new StringBuilder();
                        byte[] buffer = new byte[8192];
                        int bytesReceived;

                        while ((bytesReceived = socket.Receive(buffer)) > 0)
                        {
                            responseBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesReceived));
                        }

                        string responseData = responseBuilder.ToString();
                        int headerEndIndex = responseData.IndexOf("\r\n\r\n");

                        if (headerEndIndex >= 0)
                        {
                            responseData = responseData.Substring(headerEndIndex + 4);
                        }

                        return responseData;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }

            private void ParseAndDisplayRSS(string xmlContent)
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xmlContent);

                    XmlNodeList items = xmlDoc.SelectNodes("//item");

                    if (items != null && items.Count > 0)
                    {
                        foreach (XmlNode item in items)
                        {
                            string title = item.SelectSingleNode("title")?.InnerText ?? "N/A";
                            string link = item.SelectSingleNode("link")?.InnerText ?? "";
                            string description = item.SelectSingleNode("description")?.InnerText ?? "";
                            string pubDate = item.SelectSingleNode("pubDate")?.InnerText ?? "";

                            // Trích xuất URL hình ảnh từ description
                            string imageUrl = ExtractImageUrl(description);

                            // Loại bỏ HTML tags từ description
                            if (!string.IsNullOrEmpty(description))
                            {
                                description = Regex.Replace(description, "<.*?>", string.Empty);
                                description = description.Trim();
                            }

                            ArticleItem article = new ArticleItem
                            {
                                Title = title,
                                Link = link,
                                Description = description,
                                PubDate = pubDate,
                                ImageUrl = imageUrl
                            };

                            lstArticles.Items.Add(article);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi parse XML: {ex.Message}", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            private string ExtractImageUrl(string html)
            {
                if (string.IsNullOrEmpty(html))
                    return null;

                // Tìm URL hình ảnh trong thẻ <img src="...">
                Match match = Regex.Match(html, @"<img[^>]+src\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase);

                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value;
                }

                return null;
            }

            [STAThread]
            static void Main()
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }

        public class ArticleItem
        {
            public string Title { get; set; }
            public string Link { get; set; }
            public string Description { get; set; }
            public string PubDate { get; set; }
            public string ImageUrl { get; set; }

            public override string ToString()
            {
                return $"📰 {Title}  ({PubDate})";
            }
        }
    }
}
