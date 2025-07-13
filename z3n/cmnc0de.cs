using Leaf.xNet;
using NBitcoin;
using Nethereum.ABI;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Model;
using Nethereum.Signer;
using Nethereum.Util;
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
using static Leaf.xNet.Services.Cloudflare.CloudflareBypass;


namespace w3tools //by @w3bgrep
{

    public  static class TestStatic
    {
        public static string Body(this IZennoPosterProjectModel project, Instance instance, string url, string parametr = "ResponseBody", bool reload = false)
        {
            return new Traffic(project, instance).Get(url, parametr);

        }
        public static void _SAFU(this IZennoPosterProjectModel project)
        {
            string tempFilePath = project.Path + "_SAFU.zp";
            var mapVars = new List<Tuple<string, string>>();
            mapVars.Add(new Tuple<string, string>("acc0", "acc0"));
            mapVars.Add(new Tuple<string, string>("cfgPin", "cfgPin"));
            mapVars.Add(new Tuple<string, string>("DBpstgrPass", "DBpstgrPass"));
            try { project.ExecuteProject(tempFilePath, mapVars, true, true, true); }
            catch (Exception ex) { project.SendWarningToLog(ex.Message); }
            return;
        }
    }
}
