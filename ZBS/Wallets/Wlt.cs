using System;
using System.Linq;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using NBitcoin;
using System.Collections.Generic;
using Org.BouncyCastle.Asn1.Crmf;

namespace ZBSolutions
{
    public enum W
    {
        MetaMask,
        Rabby,
        Backpack,
        Razor,
        Zerion,
        Keplr
    }
    public enum KeyT
    {
        secp256k1,
        base58,
        bip39,
    }
    public class Wlt
    {
        protected readonly IZennoPosterProjectModel _project;
        protected readonly Instance _instance;
        protected readonly bool _logShow;
        protected readonly string _pass;
        protected readonly Sql _sql;
        protected string _key;
        protected string _seed;

        public Wlt(IZennoPosterProjectModel project, Instance instance, bool log = false)
        {
            _project = project;
            _instance = instance;
            _logShow = log;
            _sql = new Sql(_project);
            _pass = SAFU.HWPass(_project);
        }

        protected void Log(string tolog = "", [CallerMemberName] string callerName = "", bool log = false)
        {
            if (!_logShow && !log) return;
            var stackFrame = new System.Diagnostics.StackFrame(1);
            var callingMethod = stackFrame.GetMethod();
            if (callingMethod == null || callingMethod.DeclaringType == null || callingMethod.DeclaringType.FullName.Contains("Zenno")) callerName = "null";
            _project.L0g($"[ 👛  {callerName}] [{tolog}] ");
        }
        public bool Install(string extId, string fileName, bool log = false)
        {
            string path = $"{_project.Path}.crx\\{fileName}";

            if (!File.Exists(path))
            {
                Log($"File not found: {path}", log: log);
                throw new FileNotFoundException($"CRX file not found: {path}");
            }

            var extListString = string.Join("\n", _instance.GetAllExtensions().Select(x => $"{x.Name}:{x.Id}"));
            if (!extListString.Contains(extId))
            {
                Log($"installing {path}", log:log);
                _instance.InstallCrxExtension(path);
                return true;
            }
            return false;
        }


        protected string Decrypt(KeyT KeyType)
        {
            switch (KeyType)
            {
                case KeyT.secp256k1: 
                    return _sql.Key("evm");
                case KeyT.base58:
                    return _sql.Key("sol");
                case KeyT.bip39:
                    return _sql.Key("seed");
                default:
                    throw new Exception("unsupportedType");
            }

        }
        protected string Decrypt(string KeyType)
        {

            switch (KeyType)
            {
                case "evm":
                    return _sql.Key("evm");
                case "sol":
                    return _sql.Key("sol");
                case "seed":
                    return _sql.Key("seed");
                default:
                    throw new Exception("unsupportedType");
            }
     

        }


    }


}


