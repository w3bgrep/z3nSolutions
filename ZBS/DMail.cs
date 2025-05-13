using Nethereum.Signer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ZBSolutions
{
    public class DMail : W3bWrite
    {
        private readonly string _key;
        private string _encstring;
        private string _pid;

        private readonly NetHttp _h;
        public DMail(IZennoPosterProjectModel project, string key = null, bool log = false)
        : base(project, key:key, log:log)
        {          
            _key = Key(key);
            _h = new NetHttp(project, true);
        }


        public string[] Auth()
        {

            var signer = new EthereumMessageSigner();
            string key = _key;
            string wallet = _key.ToPubEvm();
            string time = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);

            // 1. GET nonce
            string nonceJson = _h.GET("https://icp.dmail.ai/api/node/v6/dmail/auth/generate_nonce", "");
            dynamic data_nonce = JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(nonceJson);
            string nonce = data_nonce.data.nonce;

            string msgToSign = "SIGN THIS MESSAGE TO LOGIN TO THE INTERNET COMPUTER\n\n" +
                "APP NAME: \ndmail\n\n" +
                "ADDRESS: \n" + wallet + "\n\n" +
                "NONCE: \n" + nonce + "\n\n" +
                "CURRENT TIME: \n" + time;

            string sign = signer.EncodeUTF8AndSign(msgToSign, new EthECKey(key));

            // Create JObject for the message
            var message = new JObject
            {
                { "Message", "SIGN THIS MESSAGE TO LOGIN TO THE INTERNET COMPUTER" },
                { "APP NAME", "dmail" },
                { "ADDRESS", wallet },
                { "NONCE", nonce },
                { "CURRENT TIME", time }
            };

            var data = new JObject
            {
                { "message", message },
                { "signature", sign },
                { "wallet_name", "metamask" },
                { "chain_id", 1 }
            };

            string body = JsonConvert.SerializeObject(data);

            // 2. POST to verify signature
            string get_verify_json = _h.POST("https://icp.dmail.ai/api/node/v6/dmail/auth/evm_verify_signature", body);

            dynamic data_dic = JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(get_verify_json);
            string encstring = data_dic.data.token;
            string pid = data_dic.data.pid;

            try
            {
                _project.Variables["encstring"].Value = encstring;
                _project.Variables["pid"].Value = pid;
            }
            catch { }

            Log($"{encstring} {pid}");

            _encstring = encstring; _pid = pid; 
            return new string[] { encstring, pid };
        }













    }
}
