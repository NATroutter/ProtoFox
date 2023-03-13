using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using Pastel;
using Microsoft.Win32;
using System.Diagnostics;
using System.Web;
using System.Collections.Specialized;
using System.Reflection;
using System.IO;
using System.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;
using Octokit;
using Application = System.Windows.Forms.Application;
using Newtonsoft.Json;
using System.Security.Policy;

namespace ProtoFox {
    internal class ProtoFox {

        //DLL imports
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll")]
        internal static extern Boolean AllocConsole();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        //Settings
        private static Color logoC = Color.FromArgb(255, 117, 25);
        private static Color mainC = Color.FromArgb(255, 20, 63);
        private static Color highC = Color.FromArgb(255, 99, 128);
        private static Color textC = Color.FromArgb(255, 168, 184);

        //Global variables
        private static Assembly assembly = typeof(ProtoFox).Assembly;
        private static FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        private static String scheme = "ProtoFox";
        private static String schemeName = "ProtoFox";
        private static String scriptFolder = AppDomain.CurrentDomain.BaseDirectory + @"scripts\";

        private static FileTarget logfile = new FileTarget("logfile") {
            FileName = AppDomain.CurrentDomain.BaseDirectory + @"logs\latest.log",
            Layout = "[${date:format=dd-MM-yyyy_HH-mm}][${level:uppercase=true}] | ${message:withexception=true}",
            Header = "--------------------------[LOG START]--------------------------",
            Footer = "--------------------------[LOG END]--------------------------",
            CreateDirs = true,
            CleanupFileName = true,
            EnableArchiveFileCompression = true,
            ArchiveAboveSize = 104857600,
            ArchiveOldFileOnStartupAboveSize = 104857600,
            ArchiveOldFileOnStartup = true,
            MaxArchiveFiles = 9,
            ArchiveFileName = AppDomain.CurrentDomain.BaseDirectory + @"logs\archive-{#}.zip",
            ArchiveDateFormat = DateTime.Now.ToString("dd-MM-yyyy"),
            ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
            WriteFooterOnArchivingOnly = true,

        };
        private static LoggingConfiguration logConfig = new LoggingConfiguration() {
            LoggingRules = {
                    new LoggingRule("*", LogLevel.Info, logfile)
                }
        };

        private static Logger logger = LogManager.GetCurrentClassLogger();

        static async Task Main(string[] args) {
            Console.Title = "ProtoFox v" + fvi.FileVersion;
            LogManager.Configuration = logConfig;

            //Handle input from protocol
            if(args.Length > 0) {
                if(!Properties.Settings.Default.showRequest) {
                    ShowWindow(GetConsoleWindow(), SW_HIDE);
                }

                if(!Directory.Exists(scriptFolder)) {
                    Directory.CreateDirectory(scriptFolder);
                }

                if(args[0] == "force") {
                    ShowWindow(GetConsoleWindow(), SW_HIDE);
                    logger.Warn("Forcing to open without update!");
                    start();

                } else if(Uri.TryCreate(args[0], UriKind.Absolute, out var uri) &&
                    string.Equals(uri.Scheme, "ProtoFox", StringComparison.OrdinalIgnoreCase)) {
                    NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);

                    printBanner();
                    Console.WriteLine(("Executing: " + args[0]).Pastel(textC));
                    logger.Info("Requesting: " + args[0]);

                    switch(uri.DnsSafeHost) {
                        case "folder":
                            if(uri.Segments.Length == 2) {
                                String folder = uri.Segments[1];
                                if(folder == "scripts" && Directory.Exists(scriptFolder)) {
                                    Process.Start("explorer.exe", scriptFolder);
                                }
                            }
                            break;
                        case "exec":
                            if(!String.IsNullOrEmpty(uri.LocalPath)) {
                                String file = uri.LocalPath;
                                if(file.Contains(" ")) {
                                    file = file.Split(' ')[0];
                                }
                                file = file.Replace("/", "");
                                file = scriptFolder + file;
                                if(!file.EndsWith(".bat")) {
                                    file = file + ".bat";
                                }
                                if(File.Exists(file)) {
                                    Process.Start(file);
                                }
                            }
                            break;
                        case "open":
                            if(!String.IsNullOrEmpty(query.Get("url"))) {
                                String url = query.Get("url");
                                if(!url.StartsWith("https://") || !url.StartsWith("http://")) {
                                    url = "http://" + url;
                                }
                                Process.Start(url);
                            }
                            break;
                        case "display":
                            if(!String.IsNullOrEmpty(query.Get("url"))) {
                                String url = query.Get("url");
                                String title = "ProtoFox | Display";
                                int width = 900;
                                int height = 500;
                                if(!url.StartsWith("https://") || !url.StartsWith("http://")) {
                                    url = "http://" + url;
                                }
                                if(!String.IsNullOrEmpty(query.Get("title"))) {
                                    title = query.Get("title");
                                }
                                if(!String.IsNullOrEmpty(query.Get("width"))) {
                                    int.TryParse(query.Get("width"), out width);
                                }
                                if(!String.IsNullOrEmpty(query.Get("height"))) {
                                    int.TryParse(query.Get("height"), out height);
                                }
                                var process = new Process {
                                    StartInfo = {
                                            WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                                            FileName = "ProtoFoxForm.exe",
                                            Arguments = "display " + url + " " + title + " " + width + " " + height
                                      }
                                };
                                process.Start();
                            }
                            break;
                        case "about":
                            Process.Start("https://github.com/NATroutter/ProtoFox");
                            break;
                    }
                }
                return;
            }
            if (!update()) {
                start();
            }
        }

        private static void start() {
            logger.Info("Console session started!");
            input();
        }

        private static void input() {
            Console.Clear();
            printBanner();
            Console.WriteLine("Select option:".Pastel(mainC));
            Console.WriteLine("   1. ".Pastel(highC) + "Install protocol".Pastel(textC));
            Console.WriteLine("   2. ".Pastel(highC) + "Uninstall protocol".Pastel(textC));
            Console.WriteLine("   3. ".Pastel(highC) + "Open script folder".Pastel(textC));
            Console.WriteLine("   4. ".Pastel(highC) + "Protocol usage help".Pastel(textC));
            Console.WriteLine("   5. ".Pastel(highC) + "Settings".Pastel(textC));

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("> ");
            string selection = Console.ReadLine();

            switch(selection) {
                case "1":
                    Console.Clear();
                    printBanner();
                    Console.WriteLine("Installing...".Pastel(textC));
                    installProtocol(scheme, schemeName);
                    Console.WriteLine("Done!".Pastel(textC));
                    logger.Info("Protocol installed!");
                    Thread.Sleep(1000);
                    break;
                case "2":
                    Console.Clear();
                    printBanner();
                    Console.WriteLine("Uninstalling...".Pastel(textC));
                    uninstallProtocol(scheme);
                    Console.WriteLine("Done!".Pastel(textC));
                    logger.Info("Protocol uninstalled!");
                    Thread.Sleep(1000);
                    break;
                case "3":
                    Console.Clear();
                    printBanner();
                    if(!Directory.Exists(scriptFolder)) {
                        Directory.CreateDirectory(scriptFolder);
                    }
                    Console.WriteLine("Opened!".Pastel(textC));
                    Process.Start("explorer.exe", scriptFolder);
                    logger.Info("Script folder opened!");
                    Thread.Sleep(1000);
                    break;
                case "4":
                    Console.Clear();
                    printBanner();
                    Console.WriteLine("Protocol usage:".Pastel(mainC));
                    Console.WriteLine("ProtoFox://exec/<ScriptFile>".Pastel(highC));
                    Console.WriteLine(" - This will execute any .bat script inside scripts folder".Pastel(textC));
                    Console.WriteLine("ProtoFox://display?url=google.com&title=Google&width=500&height=400".Pastel(highC));
                    Console.WriteLine(" - This will open 500x400 window dispalying spesific website".Pastel(textC));
                    Console.WriteLine("ProtoFox://open?url=google.com".Pastel(highC));
                    Console.WriteLine(" - This will open spesific website in your default browser".Pastel(textC));
                    Console.WriteLine("ProtoFox://folder/scripts".Pastel(highC));
                    Console.WriteLine(" - This will open scripts folder".Pastel(textC));
                    Console.WriteLine("ProtoFox://about".Pastel(highC));
                    Console.WriteLine(" - This will open ProtoFox project page".Pastel(textC));
                    Console.WriteLine(" ");
                    Console.WriteLine("Press any key to go back to selection.".Pastel(textC));
                    logger.Info("Protocol help shown!");
                    Console.ReadKey();
                    break;
                case "5":
                    settings();
                    break;
                default:
                    break;
            }
            input();
        }

        private static void settings() {
            Console.Clear();
            printBanner();
            Console.WriteLine("Settings:".Pastel(mainC));
            Console.WriteLine("   1. ".Pastel(highC) + ("Show window on request " + status(Properties.Settings.Default.showRequest)).Pastel(textC));
            Console.WriteLine("   2. ".Pastel(highC) + ("Check update on request " + status(Properties.Settings.Default.checkUpdateAtRequst)).Pastel(textC));
            Console.WriteLine(" ");
            Console.WriteLine("   3. ".Pastel(highC) + "Back to selection".Pastel(textC));

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("> ");
            string selection = Console.ReadLine();
            switch(selection) {
                case "1":
                    Properties.Settings.Default.showRequest = !Properties.Settings.Default.showRequest;
                    break;
                case "2":
                    Properties.Settings.Default.checkUpdateAtRequst = !Properties.Settings.Default.checkUpdateAtRequst;
                    break;
                case "3":
                    input();
                    break;
                default:
                    break;
            }
            Properties.Settings.Default.Save();
            settings();
        }

        private static bool update() {
            var client = new GitHubClient(new ProductHeaderValue("ProtoFox-Update"));
            using(var releases = client.Repository.Release.GetAll("NATroutter", "ProtoFox")) {
                if(releases.Result.Count < 1) return false;
                Release release = releases.Result[0];

                if(release.Prerelease) return false;
                if(fvi.ProductVersion == release.TagName) return false;

                Update update = new Update();
                update.title = release.Name;
                update.tag = release.TagName;
                update.body = release.Body;
                update.link = release.HtmlUrl;

                using(StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "update.json"))
                using(JsonWriter writer = new JsonTextWriter(sw)) {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, update);

                    var process = new Process {
                        StartInfo = {
                            WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                            FileName = "ProtoFoxForm.exe",
                            Arguments = "update"
                        }
                    };
                    process.Start();
                }

                return true;
            }
        }

        private static string status(bool setting) {
            return setting ? "[Enabled]" : "[Disabled]";
        }

        private static void printBanner() {
            Console.WriteLine(@"╔═══════════════════════════════════════════════════════╗".Pastel(mainC));
            Console.WriteLine(@"║       ".Pastel(mainC) + @" ____            _        _____          ".Pastel(logoC) + "       ║".Pastel(mainC));
            Console.WriteLine(@"║       ".Pastel(mainC) + @"|  _ \ _ __ ___ | |_ ___ |  ___|____  __ ".Pastel(logoC) + "       ║".Pastel(mainC));
            Console.WriteLine(@"║       ".Pastel(mainC) + @"| |_) | '__/ _ \| __/ _ \| |_ / _ \ \/ / ".Pastel(logoC) + "       ║".Pastel(mainC));
            Console.WriteLine(@"║       ".Pastel(mainC) + @"|  __/| | | (_) | || (_) |  _| (_) >  <  ".Pastel(logoC) + "       ║".Pastel(mainC));
            Console.WriteLine(@"║       ".Pastel(mainC) + @"|_|   |_|  \___/ \__\___/|_|  \___/_/\_\ ".Pastel(logoC) + "       ║".Pastel(mainC));
            Console.WriteLine(@"║                                                       ║".Pastel(mainC));
            Console.WriteLine(@"║                          ".Pastel(mainC) + "_,-=._              /|_/|".Pastel(textC) + "    ║".Pastel(mainC));
            Console.WriteLine(@"║".Pastel(mainC) + " Version: ".Pastel(textC) + fvi.FileVersion.Pastel(highC) + "         `-.}   `=._,.-=-._.,  @ @._".Pastel(textC) + "  ║".Pastel(mainC));
            Console.WriteLine(@"║".Pastel(mainC) + " Author: ".Pastel(textC) + "NATroutter       ".Pastel(highC) + "    `._ _,-.   )      _,.-'".Pastel(textC) + "  ║".Pastel(mainC));
            Console.WriteLine(@"║".Pastel(mainC) + " Website: ".Pastel(textC) + "NATroutter.fi".Pastel(highC) + "               G.m-\" ^ m`m'".Pastel(textC) + "     ║".Pastel(mainC));
            Console.WriteLine(@"║                                                       ║".Pastel(mainC));
            Console.WriteLine(@"╚═══════════════════════════════════════════════════════╝".Pastel(mainC));
        }
        public static void uninstallProtocol(string UriScheme) {
            Registry.CurrentUser.DeleteSubKeyTree("SOFTWARE\\Classes\\" + UriScheme);
        }

        public static void installProtocol(string UriScheme, string FriendlyName) {
            using(var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + UriScheme)) {
                string applicationLocation = typeof(ProtoFox).Assembly.Location;

                key.SetValue("", "URL:" + FriendlyName);
                key.SetValue("URL Protocol", "");

                using(var defaultIcon = key.CreateSubKey("DefaultIcon")) {
                    defaultIcon.SetValue("", applicationLocation + ",1");
                }

                using(var commandKey = key.CreateSubKey(@"shell\open\command")) {
                    commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
                }
            }
        }
    }
}
