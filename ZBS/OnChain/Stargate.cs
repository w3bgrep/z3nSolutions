using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace ZBSolutions
{
    public class Stargate
    {

        protected readonly IZennoPosterProjectModel _project;
        protected readonly Instance _instance;
        protected readonly bool _logShow;


        public Stargate(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _instance = instance;
            _logShow = log;
        }

        public void Go(string srcChain, string dstChain, string srcToken = null, string dstToken = null)
        {
            var srcDefault = "0xEeeeeEeeeEeEeeEeEeEeeEEEeeeeEeeeeeeeEEeE";
            if (string.IsNullOrEmpty(srcToken)) srcToken = srcDefault;
            if (string.IsNullOrEmpty(dstToken)) dstToken = srcDefault;
            string url = "https://stargate.finance/bridge?" + $"srcChain={srcChain}" + $"&srcToken={srcToken}" + $"&dstChain={dstChain}" + $"&dstToken={dstToken}";
            if (_instance.ActiveTab.URL != url) _instance.ActiveTab.Navigate(url, "");

        }


        public void Connect()
        {
            _project.Deadline();
        check:

            _project.Deadline(60); Thread.Sleep(1000);

            var connectedButton = _instance.ActiveTab.FindElementByAttribute("button", "class", "css-x1wnqh", "regexp", 0);
            var unconnectedButton = _instance.ActiveTab.FindElementByAttribute("button", "sx", "\\[object\\ Object]", "regexp", 0).ParentElement;

            string state = null;

            if (!connectedButton.FindChildByAttribute("img", "alt", "Zerion", "regexp", 0).IsVoid) state = "Zerion";
            if (!connectedButton.FindChildByAttribute("img", "alt", "Backpack", "regexp", 0).IsVoid) state = "Backpack";
            else if (unconnectedButton.InnerText == "Connect Wallet") state = "Connect";

            switch (state)
            {
                case "Connect":
                    _instance.HeClick(unconnectedButton, emu: 1);
                    _instance.HeClick(("button", "innertext", "Zerion\\nConnect", "regexp", 0));
                    new ZerionWallet(_project, _instance).ZerionConnect();
                    goto check;

                case "Zerion":
                    _project.L0g($"{connectedButton.InnerText} connected with {state}");
                    break;

                default:
                    _project.L0g($"unknown state {connectedButton.InnerText}  {unconnectedButton.InnerText}");
                    goto check;

            }
        }

        public void Connect(string wallet)
        {
            _project.Deadline();
        check:

            _project.Deadline(60); Thread.Sleep(1000);

            var connectedButton = _instance.ActiveTab.FindElementByAttribute("button", "class", "css-x1wnqh", "regexp", 0);
            var unconnectedButton = _instance.ActiveTab.FindElementByAttribute("button", "sx", "\\[object\\ Object]", "regexp", 0).ParentElement;

            string state = null;

            if (!connectedButton.FindChildByAttribute("img", "alt", "Zerion", "regexp", 0).IsVoid) state += "Zerion";
            if (!connectedButton.FindChildByAttribute("img", "alt", "Backpack", "regexp", 0).IsVoid) state += "Backpack";
            else if (unconnectedButton.InnerText == "Connect Wallet") state = "Connect";


            switch (state)
            {
                case "Connect":

                    _instance.HeClick(unconnectedButton);
                    _instance.HeClick(("button", "innertext", "Backpac\\nConnect", "regexp", 0));
                    new BackpackWallet(_project, _instance).Connect();

                    goto check;

                case "Zerion":
                    _project.L0g($"{connectedButton.InnerText} connected with {state}");
                    _instance.HeClick(connectedButton);
                    _instance.HeClick(("path", "d", "M14 8H2M8 2v12", "text", 0));
                    _instance.HeClick(("div", "innertext", "Connect\\ Another\\ Wallet", "regexp", 0), "last");
                    _instance.HeClick(("img", "alt", "Backpack", "regexp", 0));
                    _instance.HeClick(("img", "alt", "Backpack", "regexp", 0));

                    goto check;

                case "ZerionBackpack":
                    _project.L0g($"{connectedButton.InnerText} connected with {state}");
                    break;


                default:
                    _project.L0g($"unknown state {connectedButton.InnerText}  {unconnectedButton.InnerText}");
                    goto check;

            }
        }



        public decimal LoadBalance()
        {
            _project.Deadline();
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        waitForBal:
            _project.Deadline(60);
            string est = _instance.HeGet(("div", "class", "css-n2rwim", "regexp", 0));

            try
            {
                decimal bal = decimal.Parse(est.Split('\n')[1].Replace("Balance: ", ""));
                return bal;
            }
            catch
            {
                goto waitForBal;
            }

        }


        public decimal WaitExpected()
        {
            _project.Deadline();
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        waitForBal:
            _project.Deadline(60);
            string est = _instance.HeGet(("input:text", "class", "css-109vo2x", "regexp", 1), atr: "value");

            try
            {
                decimal expected = decimal.Parse(est);
                return expected;
            }
            catch
            {
                goto waitForBal;
            }

        }

        public void SetManualAddress(string address)
        {
            _instance.HeClick(("button", "innertext", "Advanced\\ Transfer", "regexp", 0));
            _instance.HeClick(("button", "role", "switch", "regexp", 1));
            _instance.HeSet(("input:text", "fulltagname", "input:text", "regexp", 1), address);
        }

        public void GasOnDestination(string qnt, string sliperage = "0.5")
        {
            _instance.HeSet(("input:text", "class", "css-1qhcc16", "regexp", 0), qnt);
            _instance.HeSet(("input:text", "class", "css-1qhcc16", "regexp", 1), sliperage);
        }

    }
}
