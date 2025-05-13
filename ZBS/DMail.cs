using Nethereum.Signer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace ZBSolutions
{
    public class DMail : W3bWrite
    {
        private readonly string _key;
        private string _encstring;
        private string _pid;
        private dynamic _allMail;
        private Dictionary<string, string> _headers;
        private readonly NetHttp _h;
        public DMail(IZennoPosterProjectModel project, string key = null, bool log = false)
        : base(project, key: key, log: log)
        {
            _key = Key(key);
            _h = new NetHttp(project, true);
        }

        private string Key(string key = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                string encryptedkey = _sql.Get("secp256k1", "accounts.blockchain_private");
                key = SAFU.Decode(_project, encryptedkey);
            }

            if (string.IsNullOrEmpty(key))
            {
                Log("!W key is null or empty");
                throw new Exception("emptykey");
            }
            ;
            return key;

        }
        public string Auth()
        {

            var signer = new EthereumMessageSigner();
            string key = _key;
            Log(key);
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


            _headers = new Dictionary<string, string>
        {
            { "dm-encstring", encstring },
            { "dm-pid",pid }
        };

            return $"{encstring}|{pid}";


        }
        public dynamic GetAll()
        {

            var pageInfo = new JObject {
            { "page", 1 },
            { "pageSize", 20 }
        };
            var data = new JObject {
            { "dm_folder", "inbox" },
            { "store_type", "mail" },
            { "pageInfo", pageInfo }
        };


            string getMsgsBody = JsonConvert.SerializeObject(data);

            string allMailJson = _h.POST("https://icp.dmail.ai/api/node/v6/dmail/inbox_all/read_by_page_with_content", getMsgsBody, headers: _headers, parse: false);

            dynamic mail = JsonConvert.DeserializeObject<ExpandoObject>(allMailJson);
            string count_items = mail.data.list.Count.ToString();
            var allMailObj = mail.data.list;

            _allMail = allMailObj;
            return allMailObj;

        }
        public Dictionary<string, string> ReadMsg(int index = 0, dynamic mail = null, bool markAsRead = true)
        {
            if (mail == null) mail = _allMail;

            string sender = mail[index].dm_salias.ToString();
            string date = mail[index].dm_date.ToString();

            string dm_scid = mail[index].dm_scid.ToString();
            string dm_smid = mail[index].dm_smid.ToString();

            dynamic content = mail[index].content;
            string subj = content.subject.ToString();
            string html = content.html.ToString();


            var message = new Dictionary<string, string>
        {
            { "sender", sender },
            { "date", date },
            { "subj", subj },
            { "html", html },
            { "dm_scid", dm_scid },
            { "dm_smid", dm_smid },
        };
            if (markAsRead) MarkAsRead(index, dm_scid, dm_smid);
            return message;
        }
        public void MarkAsRead(int index = 0, string dm_scid = null, string dm_smid = null)
        {
            var status = new JObject {
            {"dm_is_read", 1 }
        };

            if (string.IsNullOrEmpty(dm_scid) || string.IsNullOrEmpty(dm_smid))
            {
                var MessageData = ReadMsg(index);
                MessageData.TryGetValue("dm_scid", out dm_scid);
                MessageData.TryGetValue("dm_smid", out dm_smid);
            }


            var info = new JArray {
            new JObject {
                {"dm_cid", dm_scid},
                {"dm_mid", dm_smid},
                {"dm_foldersource", "inbox"}
            }
        };

            var data = new JObject {
            {"status", status},
            {"mail_info_list", info},
            {"store_type", "mail"}
        };

            string body = JsonConvert.SerializeObject(data);
            string makeRead = _h.POST("https://icp.dmail.ai/api/node/v6/dmail/inbox_all/update_by_bulk", body, headers: _headers, parse: false);
        }



    }


}
