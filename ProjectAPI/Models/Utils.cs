using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace ProjectAPI.Models
{
    public class Utils
    {
        public static string SaveFile(string base64string, string folder, string extension)
        {
            var path = HttpContext.Current.Server.MapPath($"/Content/{folder}/");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string pic = Guid.NewGuid() + extension;
            byte[] imageBytes = Convert.FromBase64String(base64string);

            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);

            image.Save(path + pic);
            return $"/Content/{folder}/{pic}";
        }

        public class BarCodeGenerator
        {
            public static byte[] GetBarCode(String value)
            {
                Zen.Barcode.Code128BarcodeDraw barCode = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                Image img = barCode.Draw(value, 50);
                byte[] data = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    img.Save(ms, ImageFormat.Png);
                    data = ms.ToArray();
                }
                return data;
            }
        }
    }
}