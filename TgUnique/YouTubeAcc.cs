using Newtonsoft.Json;
namespace TgUnique
{
    public class YouTubeAcc
    {
        [JsonProperty("ChannelName")]
        public string ChannelName { get; set; }
        [JsonProperty("RefreshToken")]
        public string RefreshToken { get; set; }
        [JsonProperty("ClientId")]
        public string ClientId { get; set; }
        [JsonProperty("ClientSecret")]
        public string ClientSecret { get; set; }
        [JsonProperty("isActive")]
        public bool isActive { get; set; }
        public YouTubeAcc(string name, string token, string id, string secret)
        {
            ChannelName = name;
            RefreshToken = token;
            ClientId = id;
            ClientSecret = secret;
            isActive = true;
        }
        public YouTubeAcc() { }
    }
}
