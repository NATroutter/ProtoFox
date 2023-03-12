using CefSharp.WinForms;
using System.Runtime.ConstrainedExecution;

namespace Display {
    internal static class Program {

        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string[] args = Environment.GetCommandLineArgs();

            if(args.Length == 5) {
                string url = args[1];
                string title = args[2];
                int width = 900;
                int height = 500;

                int.TryParse(args[3], out width);
                int.TryParse(args[4], out height);

                ApplicationConfiguration.Initialize();
                Application.Run(new Main(url, title, width, height));
            }

        }
    }
}