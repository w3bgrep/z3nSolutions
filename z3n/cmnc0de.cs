using NBitcoin;

using Nethereum.ABI;
using Nethereum.ABI.ABIDeserialisation;
using Nethereum.ABI.Decoders;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary;
using ZennoLab.InterfacesLibrary.Enums.Browser;
using ZennoLab.InterfacesLibrary.Enums.Http;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;

using z3n;


namespace w3tools //by @w3bgrep
{
    

    public static class TestStatic
    {
        public static string Body(this IZennoPosterProjectModel project, Instance instance, string url, string parametr = "ResponseBody", bool reload = false)
        {
            return new Traffic(project, instance).Get(url, parametr);

        }

        public static string SPLcontract(this string tiker)
        {
            switch (tiker)
            {
                case "Sol":
                    tiker = "So11111111111111111111111111111111111111112";
                    break;
                case "pSol":
                    tiker = "pSo1f9nQXWgXibFtKf7NWYxb5enAM4qfP6UJSiXRQfL";
                    break;
                case "jitoSol":
                    tiker = "J1toso1uCk3RLmjorhTtrVwY9HJ7X8V9yYac6Y7kGCPn";
                    break;
                case "mSol":
                    tiker = "mSoLzYCxHdYgdzU16g5QSh3i5K3z3KZK7ytfqcJm7So";
                    break;
                case "bbSol":
                    tiker = "Bybit2vBJGhPF52GBdNaQfUJ6ZpThSgHBobjWZpLPb4B";
                    break;
                case "soLayer":
                    tiker = "sSo14endRuUbvQaJS3dq36Q829a3A6BEfoeeRGJywEh";
                    break;
                case "ezSol":
                    tiker = "ezSoL6fY1PVdJcJsUpe5CM3xkfmy3zoVCABybm5WtiC";
                    break;
                case "lotusSol":
                    tiker = "gangqfNY8fA7eQY3tHyjrevxHCLnhKRrLGRwUMBR4y6";
                    break;
                case "stepSol":
                    tiker = "StPsoHokZryePePFV8N7iXvfEmgUoJ87rivABX7gaW6";
                    break;
                case "binanceSol":
                    tiker = "BNso1VUJnh4zcfpZa6986Ea66P6TCp59hvtNJ8b1X85";
                    break;
                case "heliumSol":
                    tiker = "he1iusmfkpAdwvxLNGV8Y1iSbj4rUy6yMhEA3fotn9A";
                    break;
                case "driftSol":
                    tiker = "Dso1bDeDjCQxTrWHqUUi63oBvV7Mdm6WaobLbQ7gnPQ";
                    break;
                case "bonkSol":
                    tiker = "BonK1YhkXEGLZzwtcvRTip3gAL9nCeQD7ppZBLXhtTs";
                    break;
                    

                default:
                    throw new Exception($"unexpected {tiker}");


                    
            }
            return tiker;
        }
        public static string GetExtVer(string securePrefsPath, string extId)
        {
            //string securePrefsPath = _project.Variables["pathProfileFolder"].Value + @"\Default\Secure Preferences";
            string json = File.ReadAllText(securePrefsPath);
            JObject jObj = JObject.Parse(json);
            JObject settings = (JObject)jObj["extensions"]?["settings"];

            if (settings == null)
            {
                throw new Exception("Секция extensions.settings не найдена");
            }

            JObject extData = (JObject)settings[extId];
            if (extData == null)
            {
                throw new Exception($"Расширение с ID {extId} не найдено");
            }

            string version = (string)extData["manifest"]?["version"];
            if (string.IsNullOrEmpty(version))
            {
                throw new Exception($"Версия для расширения {extId} не найдена");
            }
            return version;
        }

    }
    public class Zerion
    {
        private readonly IZennoPosterProjectModel _project;
        private readonly Instance _instance;
        private readonly Logger _logger;

        private readonly string _pass;
        private readonly string _extId = "klghhnkeealcohjjanjjdaeeggmfmlpl";

        public Zerion(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _instance = instance;
            _logger = new Logger(project, log: log, classEmoji: "🇿");
        }
        private string KeyLoad(string key)
        {
            if (string.IsNullOrEmpty(key)) key = "key";
            switch (key)
            {
                case "key":
                    _logger.Send("using key from db");
                    key = new Sql(_project).Key("evm");
                    break;
                case "seed":
                    _logger.Send("using seed from db");
                    key = new Sql(_project).Key("seed");
                    break;
                default:
                    _logger.Send("using provided key");
                    return key;
            }
            if (string.IsNullOrEmpty(key))
                throw new Exception("keyIsEmpy");
            return key;
        }
        public string Install(string filename = "Zerion1.26.1.crx")
        {
            _project.Deadline();
            string path = $"{_project.Path}.crx\\{filename}";
            string id = _extId;

        install:
            _project.Deadline(30);
            _instance.InstallCrxExtension(path);
            Thread.Sleep(2000);
            string securePrefsPath = _project.Variables["pathProfileFolder"].Value + @"\Default\Secure Preferences";
            try
            {
                string v = w3tools.TestStatic.GetExtVer(securePrefsPath, id);
                if (v != "1.26.1") _instance.UninstallExtension(id);
                return v;
            }

            catch (Exception ex)
            {
                _project.SendWarningToLog(ex.Message, true);
            }
            Unlock();
            goto install;
        }
        public string Import(string key = null, string refCode = null)
        {
            key = KeyLoad(key);
            var keyType = key.KeyType();
            _logger.Send($"importing {keyType}");
            try
            {
                if (_instance.ActiveTab.IsBusy) _instance.ActiveTab.WaitDownloading();

                _instance.HeClick(("div", "innertext", "Import\\ Existing\\ Wallet", "regexp", 0), "last");

                if (keyType == "seed")
                {
                    _instance.HeClick(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html\\?windowType=tab&appMode=onboarding#/onboarding/import/mnemonic", "regexp", 0));
                    int index = 0;
                    foreach (string word in key.Split(' '))
                    {
                        _instance.ActiveTab.FindElementById($"word-{index}").SetValue(word, "Full", false);
                        index++;
                    }
                }
                else
                {
                    _instance.HeClick(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html\\?windowType=tab&appMode=onboarding#/onboarding/import/private-key", "regexp", 0));
                    _instance.HeSet(("key", "name"), key);
                }

                _instance.HeClick(("button", "innertext", "Import\\ Wallet", "regexp", 0));

                var pass = SAFU.HWPass(_project);
                _instance.HeSet(("password", "name"), pass);
                _instance.HeClick(("button", "innertext", "Confirm\\ Password", "regexp", 0));

                _instance.HeSet(("confirmPassword", "name"), pass);
                _instance.HeClick(("button", "innertext", "Set\\ Password", "regexp", 0));
                _instance.HeGet(("button", "innertext", "Enter\\ Referral\\ Code", "regexp", 0));

                if (!string.IsNullOrEmpty(refCode))
                {
                    _instance.HeClick(("button", "innertext", "Enter\\ Referral\\ Code", "regexp", 0));
                    _instance.HeSet((("referralCode", "name")), refCode);
                    _instance.HeClick(("button", "class", "_regular", "regexp", 0));
                }
                _instance.CloseExtraTabs();
                _instance.ActiveTab.Navigate("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html", "");

                var address = _instance.HeGet(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html#/receive\\?address=", "regexp", 0), atr: "href")
                    .Replace("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html#/receive?address=", "");

                return address;
            }
            catch (Exception ex)
            {
                _project.SendWarningToLog(ex.Message, true);

                _instance.CloseExtraTabs();
                _instance.ActiveTab.Navigate("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html", "");
                _instance.ActiveTab.Navigate("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html", "");

                var address = _instance.HeGet(("a", "href", "chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html#/receive\\?address=", "regexp", 0), atr: "href")
                    .Replace("chrome-extension://klghhnkeealcohjjanjjdaeeggmfmlpl/popup.8e8f209b.html#/receive?address=", "");

                return address;
            }
        }
        private void Unlock()
        {
            try
            {
                _instance.HeSet(("input:password", "fulltagname", "input:password", "text", 0), _pass, deadline: 3);
                _instance.HeClick(("button", "class", "_primary", "regexp", 0));
            }
            catch (Exception ex)
            {
                _logger.Send(ex.Message);
            }
        }
        public string Load(string key = null, string refCode = null, string filename = "Zerion1.26.1.crx")
        {
            Install(filename);
        import:
            try
            {
                var address = Import(key, refCode);
                return address;
            }
            catch (Exception ex)
            {
                _project.SendWarningToLog(ex.Message);
                Unlock();
                goto import;
            }
        }

        public void BlackBtn ()
        {
            _project.Deadline();

            waitTab:
                _project.Deadline(10);
            if(!_instance.ActiveTab.URL.Contains(_extId))
                goto waitTab;

            clickBtn:
            _project.Deadline(30);
            try
            {
                _instance.HeClick(("button", "class", "_primary", "regexp", 0));
            }
            catch (Exception ex)
            {
                _logger.Send(ex.Message);         
            }

            if (!_instance.ActiveTab.URL.Contains(_extId))
                goto clickBtn;
        
        }




    }



}
