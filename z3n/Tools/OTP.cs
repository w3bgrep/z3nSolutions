using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.UI.WebControls;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace z3n
{
    public static class OTP
    {
        public static string Offline(string keyString, int waitIfTimeLess = 5)
        {
            if (string.IsNullOrEmpty(keyString))
                throw new Exception($"invalid input:[{keyString}]");
            
            var key = OtpNet.Base32Encoding.ToBytes(keyString.Trim());
            var otp = new OtpNet.Totp(key);
            string code = otp.ComputeTotp();
            int remainingSeconds = otp.RemainingSeconds();

            if (remainingSeconds <= waitIfTimeLess)
            {
                Thread.Sleep(remainingSeconds * 1000 + 1);
                code = otp.ComputeTotp();
            }

            return code;
        }
        public static string FirstMail(IZennoPosterProjectModel project, string email )
        {
            if (string.IsNullOrEmpty(email))
                throw new Exception($"invalid input:[{email}]");
            return new FirstMail(project).GetOTP(email);
        }


    }
}
