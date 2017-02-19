using Newtonsoft.Json;
using RestSharp;

namespace CustomizedPushNotifications.API.Twitch
{
    class Twitch
    {
        public static bool StreamerOnline(string streamerName)
        {
            var client = new RestClient("https://api.twitch.tv/kraken");

            var request = new RestRequest("streams/{streamerName}", Method.GET);
            request.AddUrlSegment("streamerName", streamerName);

            request.AddHeader("Accept", "application/vnd.twitchtv.v3+json");
            request.AddHeader("Client-ID", new Configuration().TwitchClientId);

            IRestResponse response = client.Execute(request);
            dynamic deserializedResponse = JsonConvert.DeserializeObject<dynamic>(response.Content);

            return deserializedResponse.stream != null;
        }
    }
}