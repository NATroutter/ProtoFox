using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProtoFoxForm {
    public partial class DisplayForm : Form {

        private ChromiumWebBrowser browser = new ChromiumWebBrowser();

        public DisplayForm(string url, string title, int width, int height) {
            InitializeComponent();
            this.Text = title;
            this.Width = width;
            this.Height = height;

            this.Controls.Add(browser);

            browser.MenuHandler = new MenuHandler(url);

            browser.Dock = DockStyle.Fill;
            browser.LoadUrl(url);
        }

        private void DisplayForm_Load(object sender, EventArgs e) {

        }
    }
}
