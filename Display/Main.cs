using CefSharp;
using CefSharp.WinForms;
using System.Runtime.ConstrainedExecution;

namespace Display {
    public partial class Main : Form {
        public Main(string url, string title, int width, int height) {
            InitializeComponent();
            this.Text = title;
            this.Width = width;
            this.Height = height;

            ChromiumWebBrowser browser = new ChromiumWebBrowser();
            browser.Dock = DockStyle.Fill;
            this.Controls.Add(browser);

            browser.LoadUrl(url);

        }

        private void Main_Load(object sender, EventArgs e) {

        }
    }
}