using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;

namespace CustomizedPushNotifications.API.Twitch
{
    class Twitch
    {
        private static RestClient restClient = new RestClient("https://api.twitch.tv/kraken");

        public static bool StreamerOnline(string streamerName, Configuration configuration)
        {
            var request = new RestRequest("streams/{streamerName}", Method.GET);
            request.AddUrlSegment("streamerName", streamerName);

            request.AddHeader("Accept", "application/vnd.twitchtv.v3+json");
            request.AddHeader("Client-ID", configuration.TwitchClientId);

            IRestResponse response = restClient.Execute(request);
            dynamic responseJson = JsonConvert.DeserializeObject<dynamic>(response.Content);

            return responseJson.stream != null;
        }

        public static Dictionary<string, bool> StreamersOnline(string[] streamerNames, Configuration configuration)
        {
            var result = new Dictionary<string, bool>();

            foreach (var streamerName in streamerNames)
            {
                result.Add(streamerName, false);
            }

            var request = new RestRequest("streams?channel={streamerNames}", Method.GET);
            request.AddUrlSegment("streamerNames", string.Join(",", streamerNames));

            request.AddHeader("Accept", "application/vnd.twitchtv.v3+json");
            request.AddHeader("Client-ID", configuration.TwitchClientId);

            IRestResponse response = restClient.Execute(request);
            dynamic responseJson = JsonConvert.DeserializeObject<dynamic>(response.Content);

            foreach (dynamic stream in responseJson.streams)
            {
                result[stream.channel.name.Value] = true;
            }

            return result;
        }
    }
}