using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public class AppData
    {
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        public static decimal Round(decimal e)
        {
            return Convert.ToDecimal((e - Math.Floor(e)) >= .5m ? Math.Ceiling(e) : Math.Floor(e));
        }
        public static decimal Round(decimal e, int r)
        {
            decimal i = 1;
            for (int o = 0; o < r; o++)
                i *= 10;
            e *= i;
            return e - Math.Floor(e) >= .5m ? Math.Ceiling(e) / i : Math.Floor(e) / i;
        }
        public static string RestoreSpecialCharacter(String value)
        {
            value = value.Replace("74512541", "+");
            value = value.Replace("01245124", "/");
            value = value.Replace("74512025", "&");
            return value;
        }

        public static string ReplaceSpecialCharacter(String value)
        {
            value = value.Replace("+", "74512541");
            value = value.Replace("/", "01245124");
            value = value.Replace("&", "74512025");
            return value;
        }

        public static void CheckAppKey(AkashGangaTravelEntities dbContext, string key, byte KeyFor)
        {
            bool isValid = false;
            if (!string.IsNullOrEmpty(key))
            {
                Guid AppKey;
                Guid.TryParse(key, out AppKey);
                var apps = (from a1 in dbContext.Apps
                            where a1.AppKey == AppKey
                            && a1.KeyFor == KeyFor
                            && a1.Status == (byte)Status.Active
                            select a1);
                if (apps.Any())
                    isValid = true;
            }
            if (!isValid)
                throw new Exception("Invalid request!!");
        }

        public static byte[] FileUrlToBytes(string url)
        {
            System.Net.HttpWebRequest request = null;
            System.Net.HttpWebResponse response = null;
            byte[] b = null;

            request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            response = (System.Net.HttpWebResponse)request.GetResponse();

            if (request.HaveResponse)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        receiveStream.CopyTo(ms);
                        b = ms.ToArray();
                    }
                }
            }
            return b;
        }
        public static byte[] StreamToBytes(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        public static byte[] BitmapToBytes(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
        public static DateTime CheckDate(string date, string errMsg)
        {
            DateTime parsed;
            if (!DateTime.TryParseExact(date, "dd'/'MM'/'yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out parsed))
                throw new ArgumentException(errMsg);
            return parsed;
        }
        public static DateTime? CheckDate(string date)
        {
            if (!DateTime.TryParseExact(date, "dd'/'MM'/'yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsed))
                return null;
            return parsed;
        }

        public static DateTime CheckDateYMD(string date, string errMsg)
        {
            if (!DateTime.TryParseExact(date, "yyyy'-'MM'-'dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsed))
                throw new ArgumentException(errMsg);
            return parsed;
        }
        public static DateTime CheckDateTimeYMD(string date, string errMsg)
        {
            //DateTime dt = DateTime.ParseExact(date, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
            if (!DateTime.TryParseExact(date, "yyyy-MM-dd HH:mm", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsed))
                throw new ArgumentException(errMsg);
            return parsed;
        }


        public static int CheckInt(string data)
        {
            if (!int.TryParse(data, out int parsed))
                return 0;
            return parsed;
        }
        public static int CheckInt(string data, string errMsg)
        {
            if (!int.TryParse(data, out int parsed))
                throw new ArgumentException(errMsg);
            return parsed;
        }

        public static byte CheckByte(string data, string errMsg)
        {
            if (!byte.TryParse(data, out byte parsed))
                throw new ArgumentException(errMsg);
            return parsed;
        }
        public static decimal CheckDecimal(string data, string errMsg)
        {
            if (!decimal.TryParse(data, out decimal parsed))
                throw new ArgumentException(errMsg);
            return parsed;
        }
        public static string GenerateStaffCode(AkashGangaTravelEntities dbContext)
        {
            int SlNo = 0;
            var data = dbContext.Staffs.OrderByDescending(x => x.StaffId);
            if (data.Any())
                SlNo = Convert.ToInt32(data.First().StaffCode.Substring(4));
            SlNo += 1;
            return "STF" + SlNo.ToString("D10");
        }

        public static string GeneratePackageCode(AkashGangaTravelEntities dbContext)
        {
            int SlNo = 0;
            var data = dbContext.Packages.OrderByDescending(x => x.PackageId);
            if (data.Any())
                SlNo = Convert.ToInt32(data.First().PackageCode.Substring(4));
            SlNo += 1;
            return "PCK" + SlNo.ToString("D3");
        }

        public static string GenerateEnquiryCode(AkashGangaTravelEntities dataContext)
        {
            string invoice = string.Empty;
            int serialNo = 1;

            string year = DateTime.Now.Date.Year.ToString().Substring(2, 2);
            string month = DateTime.Now.Date.Month.ToString("D2");
            string currentPrefix = "ENQ" + year + month;
            var lastEnquiryCode = dataContext.Enquiries
                .Where(s => s.EnquiryCode.StartsWith(currentPrefix))
                .OrderByDescending(s => s.EnquiryId)
                .Select(s => s.EnquiryCode)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(lastEnquiryCode))
            {
                string serialPart = lastEnquiryCode.Substring(currentPrefix.Length);
                if (int.TryParse(serialPart, out int lastSerial))
                {
                    serialNo = lastSerial + 1;
                }
            }
            invoice = currentPrefix + serialNo.ToString();
            return invoice;
        }
        public static string GenerateAgentCode(AkashGangaTravelEntities dataContext)
        {
            string invoice = string.Empty;
            int serialNo = 1;

            string year = DateTime.Now.Date.Year.ToString().Substring(2, 2); 
            string month = DateTime.Now.Date.Month.ToString("D2");           
            string currentPrefix = "AGT" + year + month;                     
            var lastAgentCode = dataContext.Agents
                .Where(s => s.AgentCode.StartsWith(currentPrefix))
                .OrderByDescending(s => s.AgentId)
                .Select(s => s.AgentCode)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(lastAgentCode))
            {
                string serialPart = lastAgentCode.Substring(currentPrefix.Length);
                if (int.TryParse(serialPart, out int lastSerial))
                {
                    serialNo = lastSerial + 1;
                }
            }
            invoice = currentPrefix + serialNo.ToString();
            return invoice;
        }







        public static string GenerateBookingCode(AkashGangaTravelEntities dataContext)
        {
            string invoice = string.Empty;
            int serialNo = 1;

            string year = DateTime.Now.Date.Year.ToString().Substring(2, 2);
            string month = DateTime.Now.Date.Month.ToString("D2");
            string currentPrefix = "BKC" + year + month;
            var lastBookingCode = dataContext.Bookings
                .Where(s => s.BookingCode.StartsWith(currentPrefix))
                .OrderByDescending(s => s.BookingId)
                .Select(s => s.BookingCode)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(lastBookingCode))
            {
                string serialPart = lastBookingCode.Substring(currentPrefix.Length);
                if (int.TryParse(serialPart, out int lastSerial))
                {
                    serialNo = lastSerial + 1;
                }
            }
            invoice = currentPrefix + serialNo.ToString();
            return invoice;
        }


    }
}
