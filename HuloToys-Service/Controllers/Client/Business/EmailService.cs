using Caching.Elasticsearch;
using HuloToys_Service.Models.Account;
using HuloToys_Service.Models.Client;
using System.Net;
using System.Net.Mail;
using Telegram.Bot.Types;

namespace HuloToys_Service.Controllers.Client.Business
{
    public class EmailService
    {

        private readonly IConfiguration _configuration;
        
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
           


        }
        private void InitilizationEmail(MailMessage message, SmtpClient smtp)
        {
            //--Re-initialization Email:
            message = new MailMessage();
            message.From = new MailAddress(_configuration["Email:UserName"]);
            smtp = new SmtpClient(_configuration["Email:HOST"],
                Convert.ToInt32(_configuration["Email:PORT"]));
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Credentials = new NetworkCredential(_configuration["Email:UserName"], _configuration["Email:Password"]);
            smtp.Timeout = 20000;
        }
        public bool SendEmailChangePassword(string forgot_password_token, AccountESModel account, ClientESModel client)
        {
            bool ressult = true;
            try
            {
                //--Re-initialization Email:
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                InitilizationEmail(message, smtp);
                message = new MailMessage();
                message.From = new MailAddress(_configuration["Email:UserName"]);
                message.To.Add(client.Email);
                smtp = new SmtpClient(_configuration["Email:HOST"],
                    Convert.ToInt32(_configuration["Email:PORT"]));
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Credentials = new NetworkCredential(_configuration["Email:UserName"], _configuration["Email:Password"]);
                smtp.Timeout = 20000;
                //-- Body
                var cc = _configuration["Email:CC"];
                if(cc!=null && cc.Trim() != "")
                {
                    message.CC.Add(cc);
                }
                var bcc = _configuration["Email:BCC"];
                if (bcc != null && bcc.Trim() != "")
                {
                    message.Bcc.Add(bcc);
                }
                var body = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Template", "email", "forgot-password.html"));
                if(body!=null && body.Trim() != "")
                {
                    var body_fixed=body
                        .Replace("{domain}", _configuration["Email:Domain"])
                        .Replace("{client_name}",client.ClientName)
                        .Replace("{username}", account.UserName)
                        .Replace("{change_password_url}", forgot_password_token.Replace("+", "-").Replace("/", "_"))
                        ;
                    message.Body = body_fixed;
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                ressult = false;
            }
            return ressult;
        }
    }
}
