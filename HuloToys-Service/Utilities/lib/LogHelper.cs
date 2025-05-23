﻿using Newtonsoft.Json;
using System.Net;
using Telegram.Bot;

namespace HuloToys_Service.Utilities.Lib
{
    public static class LogHelper
    {
        public static string botToken = "5321912147:AAFhcJ9DolwPWL74WbMjOOyP6-0G7w88PWY";
        public static string group_Id = "-739120187";
        public static string enviromment = "DEV";
        public static string CompanyType = " ";
        public static int CompanyTypeInt = 0;
        public static int InsertLogTelegram(string message)
        {
            var rs = 1;
            try
            {
                LoadConfig();
                TelegramBotClient alertMsgBot = new TelegramBotClient(botToken);
                var rs_push = alertMsgBot.SendTextMessageAsync(group_Id, "[" + enviromment + "-" + CompanyType + "] - " + message).Result;
            }
            catch (Exception ex)
            {
                rs = -1;
            }
            return rs;
        }
        public static int InsertLogTelegram(string bot_token, string id_group, string message)
        {
            var rs = 1;
            try
            {
                TelegramBotClient alertMsgBot = new TelegramBotClient(bot_token);
                var rs_push = alertMsgBot.SendTextMessageAsync(id_group, message).Result;
            }
            catch (Exception ex)
            {
                rs = -1;
            }
            return rs;
        }
        public static void InsertLogTelegramByUrl(string bot_token, string id_group, string msg)
        {
            string JsonContent = string.Empty;
            string url_api = "https://api.telegram.org/bot" + bot_token + "/sendMessage?chat_id=" + id_group + "&text=" + msg;
            try
            {
                using (var webclient = new WebClient())
                {
                    JsonContent = webclient.DownloadString(url_api);
                }
            }
            catch (Exception ex)
            {
                WriteLogActivity("D://", ex.ToString());
            }
        }
        public static void WriteLogActivity(string AppPath, string log_content)
        {
            StreamWriter sLogFile = null;
            try
            {
                //Ghi lại hành động của người sử dụng vào log file
                string sDay = string.Format("{0:dd}", DateTime.Now);
                string sMonth = string.Format("{0:MM}", DateTime.Now);
                string strLogFileName = sDay + "-" + sMonth + "-" + DateTime.Now.Year + ".log";
                string strFolderName = AppPath + @"\Logs\" + DateTime.Now.Year + "-" + sMonth;
                //Application.StartupPath
                //Tạo thư mục nếu chưa có
                if (!Directory.Exists(strFolderName + @"\"))
                {
                    Directory.CreateDirectory(strFolderName + @"\");
                }
                strLogFileName = strFolderName + @"\" + strLogFileName;

                if (File.Exists(strLogFileName))
                {
                    //Nếu đã tồn tại file thì tiếp tục ghi thêm
                    sLogFile = File.AppendText(strLogFileName);
                    sLogFile.WriteLine(string.Format("Thời điểm ghi nhận: {0:hh:mm:ss tt}", DateTime.Now));
                    sLogFile.WriteLine(string.Format("Chi tiết log: {0}", log_content));
                    sLogFile.WriteLine("-------------------------------------------");
                    sLogFile.Flush();
                }
                else
                {
                    //Nếu file chưa tồn tại thì có thể tạo mới và ghi log
                    sLogFile = new StreamWriter(strLogFileName);
                    sLogFile.WriteLine(string.Format("Thời điểm ghi nhận: {0:hh:mm:ss tt}", DateTime.Now));
                    sLogFile.WriteLine(string.Format("Chi tiết log: {0}", log_content));
                    sLogFile.WriteLine("-------------------------------------------");
                }
                sLogFile.Close();
            }
            catch (Exception)
            {
                if (sLogFile != null)
                {
                    sLogFile.Close();
                }
            }
        }
        private static void LoadConfig()
        {

            using (StreamReader r = new StreamReader("appsettings.json"))
            {
                AppSettings _appconfig = new AppSettings();
                string json = r.ReadToEnd();
                _appconfig = JsonConvert.DeserializeObject<AppSettings>(json);
                enviromment = _appconfig.BotSetting.environment;
                botToken = _appconfig.BotSetting.bot_token;
                group_Id = _appconfig.BotSetting.bot_group_id;
                CompanyType = "Hulotoys ";

            }
        }
        public class AppSettings
        {
            public BotSetting BotSetting { get; set; }
            public string CompanyType { get; set; }

        }

        public class BotSetting
        {
            public string bot_token { get; set; }
            public string bot_group_id { get; set; }
            public string environment { get; set; }
        }
        public class SystemLog
        {

            public int SourceID { get; set; } // log từ nguồn nào, quy định trong SystemLogSourceID
            public string Type { get; set; } // nội dung: booking, order,....
            public string KeyID { get; set; } // Key: mã đơn, mã khách hàng, mã booking,....
            public string ObjectType { get; set; } // ObjectType: Dùng để phân biệt các đối tượng cần log với nhau. Ví dụ: log cho đơn hàng, khách hàng, hợp đồng, Phiếu thu...
            public int CompanyType { get; set; }//dùng để phân biệt company nào
            public string Log { get; set; } // nội dung log
            public DateTime CreatedTime { get; set; } // thời gian tạo
        }
    }
}
