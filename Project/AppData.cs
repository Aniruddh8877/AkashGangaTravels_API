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

        public static string GenerateEnquiryCode(AkashGangaTravelEntities dbContext)
        {
            var now = DateTime.Now;
            string prefix = "ENQ" + now.ToString("yyMM"); // e.g., ENQ202507
            var lastCode = dbContext.Enquiries
                .Where(x => x.EnquiryCode.StartsWith(prefix))
                .OrderByDescending(x => x.EnquiryId)
                .Select(x => x.EnquiryCode)
                .FirstOrDefault();
            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastCode) && lastCode.Length >= prefix.Length + 2)
            {
                string numberPart = lastCode.Substring(prefix.Length); // extract the serial (e.g., 01)
                if (int.TryParse(numberPart, out int parsedNumber))
                {
                    nextNumber = parsedNumber + 1;
                }
            }
            string finalCode = prefix + nextNumber.ToString("D2"); // pad with 2 digits (01, 02, ...)
            return finalCode;
        }
        public static string GenterateAgentCode(AkashGangaTravelEntities db)
        {
            int slNo = 1;
            string currentYear = DateTime.Now.ToString("yy");
            string currentMonth = DateTime.Now.ToString("MM");
            string prefix = $"AGT{currentYear}{currentMonth}";
            var lastAgent = db.Agents
                .Where(x => x.AgentCode.StartsWith(prefix))
                .OrderByDescending(x => x.AgentId)
                .FirstOrDefault();
            if (lastAgent != null)
            {
                string lastCode = lastAgent.AgentCode.Substring(6); // get serial
                int.TryParse(lastCode, out slNo);
                slNo += 1;
            }
            return $"{prefix}{slNo.ToString("D3")}";
        }

        public static string GenerateBookingCode(AkashGangaTravelEntities db)
        {
            int slNo = 1;
            string currentYear = DateTime.Now.ToString("yy");
            string currentMonth = DateTime.Now.ToString("MM");
            string prefix = $"BK{currentYear}{currentMonth}"; 
            var lastBooking = db.Bookings
                .Where(x => x.BookingCode.StartsWith(prefix))
                .OrderByDescending(x => x.BookingId) 
                .FirstOrDefault();
            if (lastBooking != null)
            {
                string lastCode = lastBooking.BookingCode.Substring(prefix.Length);
                if (int.TryParse(lastCode, out int lastSlNo))
                {
                    slNo = lastSlNo + 1;
                }
            }
            return $"{prefix}{slNo.ToString("D3")}";
        }


    }
}
