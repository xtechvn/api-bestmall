using APP_CHECKOUT.Model.Orders;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace APP_CHECKOUT.Repositories
{
    public class EmailService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly string _cc;
        private readonly string _bcc;
        private readonly string _domain;
        private const string EmailTemplatePath = "\\EmailTemplates\\OrderConfirmationEmail.html"; // Đường dẫn tới file template

        public EmailService()
        {
            _host = ConfigurationManager.AppSettings["Email_HOST"];
            _port = int.Parse(ConfigurationManager.AppSettings["Email_PORT"]);
            _username = ConfigurationManager.AppSettings["Email_UserName"];
            _password = ConfigurationManager.AppSettings["Email_Password"];
            _cc = ConfigurationManager.AppSettings["Email_CC"];
            _bcc = ConfigurationManager.AppSettings["Email_BCC"];
            _domain = ConfigurationManager.AppSettings["Email_Domain"];
        }

        public bool SendOrderConfirmationEmail(string recipientEmail, string orderNo, List<CartItemMongoDbModel> carts)
        {
            try
            {
                using (SmtpClient client = new SmtpClient(_host, _port))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_username, _password);
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress(_username, "BestMall CSKH"); // Tên hiển thị là BestMall
                        mail.To.Add(recipientEmail);
                        mail.Subject = $"Xác nhận đơn hàng của bạn tại BestMall - #{orderNo}";
                        mail.IsBodyHtml = true;

                        if (!string.IsNullOrEmpty(_cc))
                        {
                            mail.CC.Add(_cc);
                        }
                        if (!string.IsNullOrEmpty(_bcc))
                        {
                            mail.Bcc.Add(_bcc);
                        }

                        mail.Body = ReadEmailTemplateAndPopulate( orderNo, carts);

                        client.Send(mail);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                // Log lỗi chi tiết hơn ở đây
                return false;
            }
        }
        private string ReadEmailTemplateAndPopulate(string orderNo,List<CartItemMongoDbModel> carts)
        {
            try
            {
                string templatePath = Environment.CurrentDirectory + EmailTemplatePath;
                string htmlContent = File.ReadAllText(templatePath);

                // Thay thế các placeholder tĩnh
                htmlContent = htmlContent.Replace("{OrderNo}", orderNo);
                htmlContent = htmlContent.Replace("{Domain}", _domain);
                htmlContent = htmlContent.Replace("{CurrentYear}", DateTime.Now.Year.ToString());

                // Xây dựng các dòng sản phẩm
                StringBuilder productRowsBuilder = new StringBuilder();
                foreach (var c in carts)
                {
                    productRowsBuilder.Append($"<tr><td>{c.product.name}</td><td>{c.product.amount.ToString("N0")}</td><td>{c.quanity}</td><td>{c.total_amount.ToString("N0")}</td></tr>");
                }
                htmlContent = htmlContent.Replace("{ProductRows}", productRowsBuilder.ToString());

                return htmlContent;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Lỗi: Không tìm thấy file template email tại: {EmailTemplatePath}. Hãy đảm bảo file đã được đặt trong thư mục đầu ra và thuộc tính 'Copy to Output Directory' đã được thiết lập.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi đọc hoặc xử lý template email: {ex.Message}");
                return null;
            }
        }
        //private string BuildEmailBody(string orderNo, List<CartItemMongoDbModel> carts)
        //{
        //    StringBuilder bodyBuilder = new StringBuilder();
        //    bodyBuilder.Append("<!DOCTYPE html>");
        //    bodyBuilder.Append("<html lang=\"vi\">");
        //    bodyBuilder.Append("<head>");
        //    bodyBuilder.Append("<meta charset=\"UTF-8\">");
        //    bodyBuilder.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        //    bodyBuilder.Append("<title>Xác nhận đơn hàng BestMall</title>");
        //    bodyBuilder.Append("<style>");
        //    bodyBuilder.Append("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
        //    bodyBuilder.Append(".container { max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; background-color: #f9f9f9; }");
        //    bodyBuilder.Append(".header { background-color: #4CAF50; color: white; padding: 10px 20px; text-align: center; border-radius: 8px 8px 0 0; }");
        //    bodyBuilder.Append(".content { padding: 20px; }");
        //    bodyBuilder.Append(".footer { text-align: center; padding: 10px; font-size: 0.9em; color: #777; border-top: 1px solid #eee; margin-top: 20px; }");
        //    bodyBuilder.Append("table { width: 100%; border-collapse: collapse; margin-top: 15px; }");
        //    bodyBuilder.Append("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        //    bodyBuilder.Append("th { background-color: #f2f2f2; }");
        //    bodyBuilder.Append(".order-details strong { color: #0056b3; }");
        //    bodyBuilder.Append("</style>");
        //    bodyBuilder.Append("</head>");
        //    bodyBuilder.Append("<body>");
        //    bodyBuilder.Append("<div class=\"container\">");
        //    bodyBuilder.Append("<div class=\"header\">");
        //    bodyBuilder.Append("<h2>Xác nhận đơn hàng BestMall</h2>");
        //    bodyBuilder.Append("</div>");
        //    bodyBuilder.Append("<div class=\"content\">");
        //    bodyBuilder.Append($"<p>Kính gửi khách hàng,</p>");
        //    bodyBuilder.Append($"<p>Cảm ơn bạn đã đặt hàng tại <strong>BestMall</strong>! Đơn hàng của bạn đã được tiếp nhận và đang được xử lý.</p>");
        //    bodyBuilder.Append("<div class=\"order-details\">");
        //    bodyBuilder.Append($"<p>Mã đơn hàng: <strong>{orderNo}</strong></p>");
        //    bodyBuilder.Append("</div>");
        //    bodyBuilder.Append("<p>Chi tiết đơn hàng của bạn:</p>");
        //    bodyBuilder.Append("<table>");
        //    bodyBuilder.Append("<thead>");
        //    bodyBuilder.Append("<tr><th>Tên sản phẩm</th><th>Đơn giá (VND)</th><th>Số lượng</th><th>Thành tiền (VND)</th></tr>");
        //    bodyBuilder.Append("</thead>");
        //    bodyBuilder.Append("<tbody>");

        //    foreach (var c in carts)
        //    {
        //        bodyBuilder.Append($"<tr><td>{c.product.name}</td><td>{c.product.amount.ToString("N0")}</td><td>{c.quanity}</td><td>{c.total_amount.ToString("N0")}</td></tr>");
        //    }

        //    bodyBuilder.Append("</tbody>");
        //    bodyBuilder.Append("</table>");
        //    bodyBuilder.Append($"<p>Chúng tôi sẽ thông báo cho bạn ngay khi đơn hàng được giao. Mọi thắc mắc, xin vui lòng liên hệ với chúng tôi.</p>");
        //    bodyBuilder.Append($"<p>Trân trọng,</p>");
        //    bodyBuilder.Append($"<p>Đội ngũ BestMall</p>");
        //    bodyBuilder.Append($"<p><a href=\"" + ConfigurationManager.AppSettings["Email_Domain"] + "\">Truy cập BestMall</a></p>");
        //    bodyBuilder.Append("</div>");
        //    bodyBuilder.Append("<div class=\"footer\">");
        //    bodyBuilder.Append("<p>&copy; " + DateTime.Now.Year + " BestMall. All rights reserved.</p>");
        //    bodyBuilder.Append("</div>");
        //    bodyBuilder.Append("</div>");
        //    bodyBuilder.Append("</body>");
        //    bodyBuilder.Append("</html>");

        //    return bodyBuilder.ToString();
        //}
    }
}
