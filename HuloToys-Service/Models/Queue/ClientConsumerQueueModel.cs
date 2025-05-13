using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Models.Queue
{
    public class ClientConsumerQueueModel
    {
        public int type { get; set; } // phân biệt các data nhận về lấy từ queue
        public string data_push { get; set; }   // data queue từ nơi khác push về
    }
    public class AccountClientViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("clientid")]
        public long? ClientId { get; set; }

        [JsonProperty("clienttype")]
        public int ClientType { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("clientname")]
        public string ClientName { get; set; }

        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("forgotpasswordtoken")]
        public string ForgotPasswordToken { get; set; }

        [JsonProperty("googletoken")]
        public string GoogleToken { get; set; }

        [JsonProperty("status")]
        public byte? Status { get; set; }

        [JsonProperty("isreceiverinfoemail")]
        public byte? isReceiverInfoEmail { get; set; }

        [JsonProperty("clientcode")]
        public string ClientCode { get; set; }

        [JsonProperty("grouppermission")]
        public string GroupPermission { get; set; }

        [JsonProperty("updatelast")]
        public DateTime? UpdateLast { get; set; }

        [JsonProperty("@version")]
        public string Version { get; set; }

        [JsonProperty("createdate")]
        public DateTime? CreateDate { get; set; }
    }
}
