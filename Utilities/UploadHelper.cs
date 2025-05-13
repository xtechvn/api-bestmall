using Newtonsoft.Json;
using System.Drawing;
using System.Text;
using Utilities.ModelHelpers;

namespace Utilities
{
    public class UpLoadHelper
    {
        static string apiPrefix = "https://static-image.adavigo.com/images/upload";
        static string apiUploadVideo = "https://localhost:44377/Video/upload-video";
        static string key_token_api = "wVALy5t0tXEgId5yMDNg06OwqpElC9I0sxTtri4JAlXluGipo6kKhv2LoeGQnfnyQlC07veTxb7zVqDVKwLXzS7Ngjh1V3SxWz69";
        /// <summary>
        /// UploadImageBase64
        /// </summary>
        /// <param name="ImageBase64">src of image</param>
        /// <returns></returns>
        public static async Task<string> UploadImageBase64(ImageBase64 modelImage)
        {
            string ImagePath = string.Empty;
            string tokenData = string.Empty;
            try
            {

                var j_param = new Dictionary<string, string> {
                    { "data_file", modelImage.ImageData },
                    { "extend", modelImage.ImageExtension }};

                using (HttpClient httpClient = new HttpClient())
                {
                    tokenData = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), key_token_api);
                    var contentObj = new { token = tokenData };
                    var content = new StringContent(JsonConvert.SerializeObject(contentObj), Encoding.UTF8, "application/json");
                    var result = await httpClient.PostAsync(apiPrefix, content);
                    dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(result.Content.ReadAsStringAsync().Result);
                    if (resultContent.status == 0)
                    {
                        return resultContent.url_path;
                    }
                    else
                    {
                        //LogHelper.InsertLogTelegram("UploadImageBase64. Result: " + resultContent.status + ". Message: " + resultContent.msg);
                    }
                }
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("UploadImageBase64 - " + ex.Message.ToString() + " Token:" + tokenData);
            }
            return ImagePath;
        }

        public static async Task<string> UploadBase64Src(string ImageSrc, string StaticDomain)
        {
            try
            {
                if (ImageSrc.StartsWith("http"))
                {
                    // Download image and convert to base64
                    var base64 = await DownloadImageAsBase64(ImageSrc);
                    if (!string.IsNullOrEmpty(base64))
                    {
                        var objimage = StringHelpers.GetImageSrcBase64Object(base64);
                        objimage.ImageData = ResizeBase64Image(objimage.ImageData, out string FileType);
                        if (!string.IsNullOrEmpty(FileType)) objimage.ImageExtension = FileType;
                        return await UploadImageBase64(objimage);
                    }
                    else
                    {
                        return ImageSrc; // fallback
                    }
                }
                else
                {
                    var objimage = StringHelpers.GetImageSrcBase64Object(ImageSrc);
                    if (objimage != null)
                    {
                        objimage.ImageData = ResizeBase64Image(objimage.ImageData, out string FileType);
                        if (!string.IsNullOrEmpty(FileType)) objimage.ImageExtension = FileType;
                        return await UploadImageBase64(objimage);
                    }
                    else
                    {
                        if (ImageSrc.StartsWith(StaticDomain))
                            return ImageSrc.Replace(StaticDomain, string.Empty);
                        else
                            return ImageSrc;
                    }
                }
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("UploadImageBase64 - " + ex.Message.ToString());
            }
            return string.Empty;
        }

        public static async Task<string> DownloadImageAsBase64(string imageUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // ⚠️ Bypass 403 bằng cách giả lập browser headers
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                    client.DefaultRequestHeaders.Referrer = new Uri("https://google.com");

                    var bytes = await client.GetByteArrayAsync(imageUrl);
                    var base64 = Convert.ToBase64String(bytes);

                    string extension = Path.GetExtension(imageUrl).ToLower();
                    string mime = extension switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        _ => "image/jpeg"
                    };

                    return $"data:{mime};base64,{base64}";
                }
                catch (Exception ex)
                {
                    //LogHelper.InsertLogTelegram("DownloadImageAsBase64 - " + ex.Message);
                }
            }
            return null;
        }

        /// <summary>
        /// Resize image with maximum 1000px width
        /// </summary>
        /// <param name="ImageBase64"></param>
        /// <returns></returns>
        public static string ResizeBase64Image(string ImageBase64, out string FileType)
        {
            FileType = null;
            try
            {
                var IsValid = StringHelpers.TryGetFromBase64String(ImageBase64, out byte[] ImageByte);
                if (IsValid)
                {
                    using (var memoryStream = new MemoryStream(ImageByte))
                    {
                        var RootImage = Image.FromStream(memoryStream);
                        if (RootImage.Width > 1000)
                        {
                            int width = 1000;
                            int height = (int)(width / ((double)RootImage.Width / RootImage.Height));
                            var ResizeImage = (Image)(new Bitmap(RootImage, new Size(width, height)));
                            using (var stream = new MemoryStream())
                            {
                                ResizeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                                ImageByte = stream.ToArray();
                            }
                            FileType = "jpg";
                            return Convert.ToBase64String(ImageByte);
                        }
                        else
                        {
                            return ImageBase64;
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

    }
}
