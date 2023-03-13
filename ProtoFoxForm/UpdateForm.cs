using CefSharp;
using Markdig;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProtoFoxForm {
    public partial class UpdateForm : Form {

        WebBrowser browser = new WebBrowser();
        private string updateFile;

        private string updateURL = "https://github.com/NATroutter/ProtoFox/releases";
        private string mainApp = AppDomain.CurrentDomain.BaseDirectory + "ProtoFox.exe";

        public UpdateForm(string updateFile) {
            this.updateFile = updateFile;
            InitializeComponent();

            browser.Navigating += Browser_Navigating;

            browser.IsWebBrowserContextMenuEnabled = false;

            browser.Dock = DockStyle.Fill;
            panel1.Controls.Add(browser);

        }

        private void Browser_Navigating(object? sender, WebBrowserNavigatingEventArgs e) {
            string url = e.Url.ToString();
            if(url != "about:blank") {
                if(url.StartsWith("https://") || url.StartsWith("http://")) {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                e.Cancel = true;
            }
        }

        private void UpdateForm_Load(object sender, EventArgs e) {
            using(StreamReader file = File.OpenText(updateFile))
            using(JsonTextReader reader = new JsonTextReader(file)) {

                JsonSerializer serializer = new JsonSerializer();
                Update update = serializer.Deserialize<Update>(reader);

                updateFile = update.link;
                this.Text = "Update | " + update.title + " | " + update.tag;

                browser.DocumentText = html_head + Markdown.ToHtml(update.body) + html_footer;

                Clipboard.SetText(html_head + Markdown.ToHtml(update.body) + html_footer);

            }
        }

        private void button1_Click(object sender, EventArgs e) {
            Process.Start(new ProcessStartInfo(updateURL) { UseShellExecute = true });
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e) {
            if(File.Exists(mainApp)) {
                Process.Start(new ProcessStartInfo() {
                    FileName = mainApp,
                    Arguments = "force",
                    UseShellExecute = true
                });
            }
            Application.Exit();
        }

        string html_head = @"
            <html>
	            <head>
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
		            <style>
                        @import url('https://fonts.googleapis.com/css2?family=Cousine:ital,wght@0,400;0,700;1,400;1,700&display=swap');
                        html {
				            font-family: 'Cousine', monospace;
				            font-size: 20px;
				            color:white;
				            background-color: rgb(40, 40, 40);
			            }
			            h1,h2,h3,h4,h5 {
				            margin-bottom: 0px;
			            }
			            ul,ol,p {
				            margin-top: 0px;
				            margin-bottom: 0px;
			            }
			            a,a:active {
				            color:#ff8864
			            }
			            a:hover {
				            color:#fd3a00
			            }
		            </style>
                    <script type=""text/javascript"">
                    function disableSelection(target) {
	                    if (typeof target.onselectstart != ""undefined"") {
		                    target.onselectstart = function () {
			                    return false;
		                    }
	                    } else if (typeof target.style.MozUserSelect != ""undefined"") {
		                    target.style.MozUserSelect = ""none"";
	                    } else {
		                    target.onmousedown = function () {
			                    return false;
		                    }
	                    }
	                    target.style.cursor = ""default"";
                    }
                    </script>
                   
	            </head>
	            <body>
            ";

        string html_footer = @"
            	</body>
                <script type=""text/javascript"">
                    disableSelection(document.body);
                </script>
            </html>
            ";

    }
}
