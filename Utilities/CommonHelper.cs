using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OtpNet;
using System.Collections;
using System.ComponentModel;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;


namespace Utilities
{
    public static class CommonHelper
    {
        public static string dollarCurrencyFormat = @"\$(\d{1,3}(,\d{3})*).(\d{2})";
        public static bool GetParamWithKey(string Token, out JArray objParr, string EncryptApi)
        {
            objParr = null;
            try
            {

                Token = Token.Replace(" ", "+");
                // var serializer = new JavaScriptSerializer();                
                var jsonContent = GetContentObject(Token, EncryptApi);
                objParr = JArray.Parse("[" + jsonContent + "]");
                if (objParr != null && objParr.Count > 0)
                {
                    return true;
                }
                else { return false; }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static byte[] ConvertSecretToBytes(string secret, bool secretIsBase32) =>
          secretIsBase32 ? Base32Encoding.ToBytes(secret) : Encoding.UTF8.GetBytes(secret);
        public static string GetContentObject(string sContentEncode, string sKey)
        {
            try
            {
                // api.insidekp: Key quy uoc giua  2 ben | parramKey: tham so dong
                sContentEncode = sContentEncode.Replace(" ", "+");

                string data = Decode(sContentEncode, sKey); // Lay ra content 
                return data;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

        }
        public static string Decode(string strString, string strKeyPhrase)
        {
            try
            {
                Byte[] byt = Convert.FromBase64String(strString);
                strString = System.Text.Encoding.UTF8.GetString(byt);
                strString = KeyED(strString, strKeyPhrase);
                return strString;
            }
            catch (Exception ex)
            {

                return strString;
            }
        }
        public static string Encode(string strString, string strKeyPhrase)
        {
            try
            {
                strString = KeyED(strString, strKeyPhrase);
                Byte[] byt = System.Text.Encoding.UTF8.GetBytes(strString);
                strString = Convert.ToBase64String(byt);
                return strString;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

        }
        private static string KeyED(string strString, string strKeyphrase)
        {
            int strStringLength = strString.Length;
            int strKeyPhraseLength = strKeyphrase.Length;

            System.Text.StringBuilder builder = new System.Text.StringBuilder(strString);

            for (int i = 0; i < strStringLength; i++)
            {
                int pos = i % strKeyPhraseLength;
                int xorCurrPos = (int)(strString[i]) ^ (int)(strKeyphrase[pos]);
                builder[i] = Convert.ToChar(xorCurrPos);
            }

            return builder.ToString();
        }

        public static double convertToPound(double value, string unit)
        {
            try
            {
                double rs = 0;
                switch (unit)
                {
                    case "ounces":
                    case "oz":
                        rs = value * 0.0625;
                        break;
                    case "grams":
                    case "g":
                        rs = value * 0.0022046;
                        break;
                    case "kilograms":
                        rs = value * 2.2046;
                        break;
                    case "tonne":
                        rs = value * 2204.62262;
                        break;
                    case "kiloton":
                        rs = value * 2204622.6218;
                        break;
                    case "pounds":
                        rs = value;
                        break;
                    default:
                        rs = value;
                        break;
                }
                return rs;
            }
            catch (Exception ex)
            {
                return 1; // Nếu k có đơn vị nào thỏa mãn sẽ coi như là k có cân nặng và báo mail về cho cskh
            }

        }
        public static string RemoveUnicode(string text)
        {
            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
    "đ",
    "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
    "í","ì","ỉ","ĩ","ị",
    "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
    "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
    "ý","ỳ","ỷ","ỹ","ỵ",};
            string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
    "d",
    "e","e","e","e","e","e","e","e","e","e","e",
    "i","i","i","i","i",
    "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
    "u","u","u","u","u","u","u","u","u","u","u",
    "y","y","y","y","y",};
            for (int i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(arr1[i], arr2[i]);
                text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
            }
            return text;
        }
        public static bool isCheckLink(string strURL)
        {
            strURL = strURL.Replace("https", "http").Replace("%", "");
            Uri uriResult;
            return Uri.TryCreate(strURL, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
        }

        public static string genLinkDetailProduct(string label_name, string product_code, string product_name)
        {
            product_name = CommonHelper.RemoveSpecialCharacters(product_name);
            product_name = RemoveUnicode(CheckMaxLength(product_name.Trim(), 50));
            product_name = product_name.Replace(" ", "-");
            return ("/product/" + label_name + "/" + product_name + "-").ToLower() + product_code + ".html";
        }
        public static string genLinkDetailProductOtherLabel(string label_name, string path, bool is_extension = false)
        {
            path = path.Replace(".html?", "-variant-");
            path = path.Replace(".html", "");
            path = path.Replace("=", "__");
            string url = ("/product/" + label_name + "/" + path).ToLower() + ".html";
            if (is_extension)
            {
                url += "?product_source=3";
            }
            return url;
        }
        public static string ConvertUsExpressPathToSourcePath(string path)
        {
            string source_path = path.Split(".html")[0];
            if (source_path.Contains("-variant-"))
            {
                source_path = source_path.Replace("-variant-", ".html?");
            }
            else
            {
                source_path += ".html";
            }
            source_path = source_path.Replace("__", "=");
            return source_path;
        }



        // xử lý chuỗi quá dài
        //str: Chuoi truyen vao
        // So ky tu toi da cho phep
        // OUPUT: Tra ra chuoi sau khi xu ly
        public static string CheckMaxLength(string str, int MaxLength)
        {
            try
            {
                //str = RemoveSpecialCharacters(str);
                if (str.Length > MaxLength)
                {

                    str = str.Substring(0, MaxLength + 1); // cat chuoi
                    if (str != " ") //  ky tu sau truoc khi cat co chua ky tu ko
                    {
                        while (str.Last().ToString() != " ") // cat not cac cu tu chu cho den dau cach gan nhat
                        {
                            str = str.Substring(0, str.Length - 1); // dich trai
                        }
                    }
                    //str = str + "...";
                }
                return str;
            }
            catch (Exception ex)
            {
                // Utilities.Common.WriteLog(Models.Contants.FOLDER_LOG, "ERROR CheckMaxLength : " + ex.ToString());
                return string.Empty;
            }
        }

        public static string RemoveSpecialCharacters(string input)
        {
            try
            {
                Regex r = new Regex("(?:[^a-z0-9 ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
                return r.Replace(input, String.Empty);
            }
            catch (Exception e)
            {
                return input ?? string.Empty;
            }
        }       



        public static string ConvertAllNonCharacterToSpace(string text)
        {
            string rs = Regex.Replace(text, @"\s+", " ", RegexOptions.Singleline);
            return rs.Trim();
        }
        /// <summary>
        /// Attempts to match the supplied pattern to the input
        /// string. Only obtains a single match and returns the
        /// matching string if successful and an empty string if not.
        /// </summary>
        /// <param name="inputString">String to be searched</param>
        /// <param name="regExPattern">Pattern to be matched</param>
        /// <returns>String match or empty string if match not found</returns>
        public static string GetSingleRegExMatch(string inputString, string regExPattern)
        {
            string msg;
            string result = "";
            try
            {
                Match match = Regex.Match(inputString,
                    regExPattern,
                    RegexOptions.Singleline);
                if (match.Success)
                {
                    result = match.Value;
                }
            }
            catch (ArgumentException ex)
            {
                msg = regExPattern;
            }

            return result;
        }
        public static string RemoveAllNonCharacter(string text)
        {
            string rs = Regex.Replace(text, @"\s+", "", RegexOptions.Singleline);
            return rs.Trim();
        }
        public static string RemoveUnusedTags(this string source)
        {
            return Regex.Replace(source, @"<(\w+)\b(?:\s+[\w\-.:]+(?:\s*=\s*(?:""[^""]*""|'[^']*'|[\w\-.:]+))?)*\s*/?>\s*</\1\s*>", string.Empty, RegexOptions.Multiline);
        }
        public static T ConvertFromJsonString<T>(string jsonString)
        {
            try
            {
                T rs = JsonConvert.DeserializeObject<T>(jsonString);
                return rs;
            }
            catch
            {
                return default(T);
            }

        }
        public static string DecodeHTML(string html)
        {
            string result = "";
            try
            {
                result = HttpUtility.HtmlDecode(html);
            }
            catch
            {
                string msg = "Unable to decode HTML: " + html;
                throw new ArgumentException(msg);
            }

            return result;
        }

        /// <summary>
        /// Finds and returns a list of signed/unsigned integers/doubles 
        /// parsed from the supplied string. Comma-formatted numbers are
        /// recognized.
        /// </summary>
        /// Only recognizes "correctly formatted" comma pattern:
        /// e.g. 1,234.123 or 12,345,678.123 but not 1,23,4.123
        /// Optional parameter parseCount allows the user to limit the number
        ///  of numbers returned.
        /// Note: limiting the amount of results does NOT improve performance;
        ///  it simply returns the firs N results found.
        /// <param name="text">The string to parse</param>
        /// <param name="parseCount">The number of double values 
        /// it will attempt to parse</param>
        /// <returns>List of Double values</returns>
        public static List<Double> ParseDoubleValues(string text,
            int parseCount = -1)
        {
            // Full pattern:
            // (((-?)(\d{1,3}(,\d{3})+)|(-?)(\d)+)(\.(\d)*)?)|((-)?\.(\d)+)

            List<Double> results = new List<Double>();
            if (text == null) { return results; }

            // Optional negative sign and one or more digits
            // Valid: "1234", "-1234", "0", "-0"
            string signedIntegerNoCommas = @"(-?)(\d)+";

            // Optional negative sign and digits grouped by commas
            // Valid: "1,234", "-1,234", "1,234,567"
            // INVALID: "12,34" <-- does not fit "normal" comma pattern
            string signedIntegerCommas = @"(-?)(\d{1,3}(,\d{3})+)";

            string or = @"|";

            // Optional decimal point and digits            
            // Valid: ".123", ".0", "", ".12345", "."
            string optionalUnsignedDecimalAndTrailingNumbers = @"(\.(\d)*)?";

            // Optional negative sign, decimal point and at least one digit
            // Valid: "-.12", ".123"
            // INVALID: "", ".", "-."
            string requiredSignedDecimalAndTrailingNumbers = @"((-)?\.(\d)+)";

            string pattern = @"";

            // Allow a signed integer with or without commas
            // and an optional decimal portion
            pattern += @"(" + signedIntegerCommas + or + signedIntegerNoCommas
                + @")" + optionalUnsignedDecimalAndTrailingNumbers;

            // OR allow just a decimal portion (with or without sign)
            pattern = @"(" + pattern + @")" + or
                + requiredSignedDecimalAndTrailingNumbers;

            List<string> matches = GetMultipleRegExMatches(text, pattern);

            int matchIndex = 0;
            foreach (string match in matches)
            {
                // If the user supplied a max number of
                // doubles to parse, check to make sure we don't exceed it
                if (parseCount > 0)
                {
                    if (matchIndex + 1 > parseCount) break;
                }

                try
                {
                    // Get rid of any commas before converting
                    results.Add(Convert.ToDouble(match.Replace(",", "")));
                }
                catch
                {
                    string msg = "Unable to convert {0} to a double";
                    //Debug.WriteLine(string.Format(msg, match));
                }
                matchIndex += 1;
            }

            return results;
        }
        /// <summary>
        /// Attempts to match the supplied pattern to the input
        /// string. Obtains multiple matches and returns a
        /// list of string matches if successful and an empty
        /// list of strings if no matches found.
        /// </summary>
        /// <param name="inputString">String to search</param>
        /// <param name="regExPattern">RegEx pattern to search for</param>
        /// <returns>List of matches or empty list if no matches</returns>
        public static List<string> GetMultipleRegExMatches(
            string inputString,
            string regExPattern)
        {
            string msg;
            List<string> results = new List<string>();
            try
            {
                MatchCollection matches = Regex.Matches(inputString,
                    regExPattern,
                    RegexOptions.Singleline);
                if (matches.Count == 0) return results;

                IEnumerator e = matches.GetEnumerator();
                while (e.MoveNext())
                {
                    results.Add(((Match)e.Current).Value);
                }
            }
            catch (ArgumentException ex)
            {
                msg = regExPattern;
              
            }
            catch (RegexMatchTimeoutException ex)
            {
                msg = regExPattern;
            }
            return results;

        }


        public static double RegexPriceInHtmlPage(string dom_has_price)
        {
            try
            {
                // Dollarsign and Digits grouped by commas plus decimal
                // and change (change is required)
                //  string dollarCurrencyFormat = @"\$(\d{1,3}(,\d{3})*).(\d{2})";

                // Optional spaces and hyphen
                string spacesAndHyphen = @"\s+-\s+";

                // Grab the end of the preceeding tag, the dollar amount, and
                // optionally a hyphen and a high range amount before the
                // beginning bracket of the next tag
                string pricePattern = ">" + dollarCurrencyFormat + "(" + spacesAndHyphen + dollarCurrencyFormat + ")?" + "<";

                string match = CommonHelper.GetSingleRegExMatch(dom_has_price, pricePattern);

                // Need to remove the tag beginning and end:
                match = match.Trim(new char[] { '<', '>' });

                if (match.Length == 0)
                {
                    return 0;
                }

                List<Double> prices = CommonHelper.ParseDoubleValues(match, 2);
                return prices[0];

            }
            catch (Exception ex)
            {
                return 0;
            }
        }



        

        //public static string getUrlCurrent(HttpRequest request)
        //{
        //    //string displayUrl = UriHelper.GetDisplayUrl(Request);
        //    var urlBuilder = new UriBuilder(displayUrl)
        //    {
        //        Query = null,
        //        Fragment = null
        //    };
        //    string url = urlBuilder.ToString();

        //    return url;
        //}

        public static string ReverDateTimeTiny(string strDate)
        {
            if (!string.IsNullOrEmpty(strDate))
            {
                strDate = strDate.Replace('/', '-');
                string[] ArrDate = strDate.Split('-');

                string DD = ArrDate[0].ToString();
                string MM = ArrDate[1].ToString();
                string YYYY = ArrDate[2].ToString().Split(' ')[0];
                string JoinDate = MM + "-" + DD + "-" + YYYY;
                return JoinDate;
            }
            else
            {
                return string.Empty;
            }
        }

        public static string StripTagsRegex(string source)
        {
            if (source != null)
            {
                return Regex.Replace(source, "<.*?>", string.Empty);
            }
            else
            {
                return string.Empty;
            }
        }

        public static byte[] GetImage(string url)
        {
            Stream stream = null;
            byte[] buf;

            try
            {
                WebProxy myProxy = new WebProxy();
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                stream = response.GetResponseStream();

                using (BinaryReader br = new BinaryReader(stream))
                {
                    int len = (int)(response.ContentLength);
                    buf = br.ReadBytes(len);
                    br.Close();
                }

                stream.Close();
                response.Close();
            }
            catch (Exception exp)
            {
                buf = null;
            }

            return (buf);
        }


        public static string genLinkDetailProductv2(string label_name, string url)
        {
            var product_path = url.Split("/");
            var plant_text = product_path[product_path.Length - 1].Replace(".html", "");
            if (product_path[product_path.Length - 1].Contains(".html?product_id="))
            {
                plant_text = product_path[product_path.Length - 1].Replace(".html?product_id=", "-");
            }
            return "/product/" + label_name + "/" + plant_text + ".html";
        }
        public static string RemoveSpecialCharactersExceptDot(string input)
        {
            try
            {
                Regex r = new Regex("(?:[^a-z0-9. ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
                return r.Replace(input, String.Empty);
            }
            catch (Exception e)
            {
                return input ?? string.Empty;
            }
        }
        public static string RemoveSpecialCharactersProductName(string input)
        {
            try
            {
                var s = Regex.Replace(input, "[^a-zA-Z0-9-_ ]", "");
                return s.Replace(":", "-");
            }
            catch (Exception e)
            {
                return input ?? string.Empty;
            }
        }
        public static string MD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }
        public static string GetDescriptionFromEnumValue(Enum value)
        {
            DescriptionAttribute attribute = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .SingleOrDefault() as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }
        public static string genLinkNews(string Title, string article_id)
        {
            Title = RemoveUnicode(CheckMaxLength(Title.Trim(), 100));
            Title = RemoveSpecialCharacters(CheckMaxLength(Title.Trim(), 100));
            Title = Title.Replace(" ", "-").ToLower();
            return ("/" + Title + "-" + article_id + ".html");
        }

        public static string buildTimeSpanCurrent()
        {
            try
            {
                DateTime myDate1 = new DateTime(1970, 1, 9, 0, 0, 00);
                DateTime myDate2 = DateTime.Now;

                TimeSpan myDateResult;

                myDateResult = myDate2 - myDate1;

                double seconds = myDateResult.TotalSeconds;
                return seconds.ToString();
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static string GetEnumLabel(Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : enumValue.ToString();
        }

    }
}
