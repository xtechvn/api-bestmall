using System;
using System.Text.Json.Serialization;


namespace Entities.ViewModels.ElasticSearch
{
    
    public class AttachFileESModel
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("dataid")]
        public long DataId { get; set; }

        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("type")]
        public int? Type { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("ext")]
        public string Ext { get; set; }

        [JsonPropertyName("capacity")]
        public double? Capacity { get; set; }

        [JsonPropertyName("createDate")]
        public DateTime CreateDate { get; set; }
    }
}
