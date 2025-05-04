using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZXing;

namespace ZBS
{
    internal static class HtmlExtensions
    {
        public static string DecodeQr(HtmlElement element)
        {
            try
            {
                var bitmap = element.DrawPartAsBitmap(0, 0, 200, 200, true);
                var reader = new BarcodeReader();
                var result = reader.Decode(bitmap);
                if (result == null || string.IsNullOrEmpty(result.Text)) return "qrIsNull";
                return result.Text;
            }
            catch (Exception) { return "qrError"; }
        }

    }
}
