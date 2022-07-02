using System.Net.Http;
using Newtonsoft.Json;

namespace Wobble.Core;

public static class ApiHelpers
{
    public static string GetChannelId(string token, string channelName)
    {
        using var httpClient = new HttpClient();

        string url =
            $"https://api.streamelements.com/kappa/v2/channels/{channelName}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", $"Bearer {token}");

        HttpResponseMessage response = httpClient.SendAsync(request).Result;

        if (response.IsSuccessStatusCode)
        {
            string jsonData = response.Content.ReadAsStringAsync().Result;
            dynamic responseData = JsonConvert.DeserializeObject<object>(jsonData);

            if (responseData != null && responseData["providerId"] != null)
            {
                return responseData["providerId"];
            }
        }

        return "";
    }
}