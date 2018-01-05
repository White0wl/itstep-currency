using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace StepCoin.Extensions
{
    public static class DateTimeExtension
    {
        public static async Task<DateTime?> GetDateTimeFromInternetAsync()
        {
            DateTime? result = null;
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = null;
                response = await client.GetAsync("https://time.is");
                if (response.IsSuccessStatusCode)
                {
                    DateTimeOffset? currenttime = response.Content.Headers.Expires;
                    result = currenttime.Value.LocalDateTime;
                }
            }
            return result;
        }
    }
}
