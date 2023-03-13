using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtoFoxForm {

    public class MenuHandler : IContextMenuHandler {

        private string start;

        public MenuHandler(string start) {
            this.start = start;
        }

        public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model) {

            model.Clear();

            model.AddItem(CefMenuCommand.Back, "Back");
            model.AddItem(CefMenuCommand.Forward, "Forward");
            model.AddItem(CefMenuCommand.CustomFirst, "Start page");

        }

        public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags) {
            if(commandId == CefMenuCommand.Back) {
                browser.Back();
                return true;
            }

            if(commandId == CefMenuCommand.Forward) {
                browser.Forward();
                return true;
            }

            if(commandId == CefMenuCommand.CustomFirst) {
                browserControl.LoadUrl(start);
                return true;
            }
            return false;
        }

        public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame) {}

        public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback) {
            return false;
        }
    }
}
