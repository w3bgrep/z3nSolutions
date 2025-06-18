using System;
using System.Collections.Concurrent;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3n
{
    public static class FunctionStorage
    {
        public static ConcurrentDictionary<string, object> Functions = new ConcurrentDictionary<string, object>();
    }


    public interface ISAFU
    {
        string Encode(IZennoPosterProjectModel project, string toEncrypt, bool log);
        string Decode(IZennoPosterProjectModel project, string toDecrypt, bool log);
        string HWPass(IZennoPosterProjectModel project, bool log);
    }

    internal class SimpleSAFU : ISAFU
    {
        public string Encode(IZennoPosterProjectModel project, string toEncrypt, bool log)
        {
            if (project.Variables["debug"].Value == "True") log = true;
            if (log) project.SendInfoToLog($"[SimpleSAFU.Encode] input: ['{toEncrypt}'] key: ['{project.Variables["cfgPin"].Value}']");
            if (string.IsNullOrEmpty(toEncrypt)) return string.Empty;
            return AES.EncryptAES(toEncrypt, project.Variables["cfgPin"].Value, true);
        }

        public string Decode(IZennoPosterProjectModel project, string toDecrypt, bool log)
        {
            if (project.Variables["debug"].Value == "True") log = true;
            if (log) project.SendInfoToLog($"[SimpleSAFU.Decode] input: ['{toDecrypt}'] key: ['{project.Variables["cfgPin"].Value}']");
            if (string.IsNullOrEmpty(toDecrypt)) return string.Empty;
            try
            {
                return AES.DecryptAES(toDecrypt, project.Variables["cfgPin"].Value, true);
            }
            catch (Exception ex)
            {
                project.SendWarningToLog($"[SimpleSAFU.Decode] ERR: [{ex.Message}] key: ['{project.Variables["cfgPin"].Value}']");
                throw;
            }

        }

        public string HWPass(IZennoPosterProjectModel project, bool log)
        {
            if (project.Variables["debug"].Value == "True") log = true;
            if (log) project.SendInfoToLog($"[SimpleSAFU.HWPass] pass: ['{project.Variables["cfgPin"].Value}']");
            return project.Variables["cfgPin"].Value;
        }
    }

    public static class SAFU
    {
        private static readonly SimpleSAFU _defaultSAFU = new SimpleSAFU();

        public static void Initialize(IZennoPosterProjectModel project)
        {
            if (!FunctionStorage.Functions.ContainsKey("SAFU_Encode") ||
                !FunctionStorage.Functions.ContainsKey("SAFU_Decode") ||
                !FunctionStorage.Functions.ContainsKey("SAFU_HWPass"))
            {
                project.SendInfoToLog("using SAFU simple...");
                FunctionStorage.Functions.TryAdd("SAFU_Encode", (Func<IZennoPosterProjectModel, string, bool, string>)_defaultSAFU.Encode);
                FunctionStorage.Functions.TryAdd("SAFU_Decode", (Func<IZennoPosterProjectModel, string, bool, string>)_defaultSAFU.Decode);
                FunctionStorage.Functions.TryAdd("SAFU_HWPass", (Func<IZennoPosterProjectModel, bool, string>)_defaultSAFU.HWPass);
            }
        }

        public static string Encode(IZennoPosterProjectModel project, string toEncrypt, bool log = false)
        {
            var encodeFunc = (Func<IZennoPosterProjectModel, string, bool, string>)FunctionStorage.Functions["SAFU_Encode"];
            if (log) project.SendInfoToLog($"SAFU.Encode: toEncrypt = '{toEncrypt}'");
            string result = encodeFunc(project, toEncrypt, log);
            if (log) project.SendInfoToLog($"SAFU.Encode: result = '{result}'");
            return result;
        }

        public static string Decode(IZennoPosterProjectModel project, string toDecrypt, bool log = false)
        {
            var decodeFunc = (Func<IZennoPosterProjectModel, string, bool, string>)FunctionStorage.Functions["SAFU_Decode"];
            if (log)
                project.SendInfoToLog($"SAFU.Decode: Вызов с toDecrypt = '{toDecrypt}'");
            string result = decodeFunc(project, toDecrypt, log);
            if (log)
                project.SendInfoToLog($"SAFU.Decode: Результат = '{result}'");
            return result;
        }

        public static string HWPass(IZennoPosterProjectModel project, bool log = false)
        {
            var hwPassFunc = (Func<IZennoPosterProjectModel, bool, string>)FunctionStorage.Functions["SAFU_HWPass"];
            if (log) project.SendInfoToLog("SAFU.HWPass: call");
            string result = hwPassFunc(project, log);
            if (log) project.SendInfoToLog($"SAFU.HWPass: result = '{result}'");
            return result;
        }
    }

}
