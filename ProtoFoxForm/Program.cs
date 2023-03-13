using System.Security.Policy;

namespace ProtoFoxForm {
    internal static class Program {

        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ApplicationConfiguration.Initialize();

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length >= 2) {
                string action = args[1];
                switch (action.ToLower()) {
                    case "display":
                        if(args.Length == 6) {
                            string url = args[2];
                            string title = args[3];
                            int width = 900;
                            int height = 500;

                            int.TryParse(args[4], out width);
                            int.TryParse(args[5], out height);

                            Application.Run(new DisplayForm(url, title, width, height));
                        }
                        break;
                    case "update":
                        string updateFile = AppDomain.CurrentDomain.BaseDirectory + @"update.json";
                        if(!File.Exists(updateFile)) return;

                        Application.Run(new UpdateForm(updateFile));
                        break;
                }
            }         
        }
    }
}