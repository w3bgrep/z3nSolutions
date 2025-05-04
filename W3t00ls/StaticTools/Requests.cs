
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Http;

namespace ZBS
{
    internal static class Requests
    {
        public static string GET(string url, string proxy = "", bool log = false)
        {
            string response = ZennoPoster.HttpGet(url, proxy, "UTF-8", ResponceType.BodyOnly, 5000, "", "Mozilla/5.0", true, 5, null, "", false);
            return response;
        }
        public static string POST(string url, string body, string proxy = "")
        {
            string response = ZennoPoster.HttpPost(url, body, "application/json", proxy, "UTF-8", ResponceType.BodyOnly, 5000, "", "Mozilla/5.0", true, 5, null, "", false);
            return response;
        }


    }
}



