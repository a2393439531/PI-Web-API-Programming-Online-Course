using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace piwebapi_cs_streetview
{
    public class PIWebAPIClient
    {
        private HttpClient client;

        public PIWebAPIClient(string userName, string password)
        {
            client = new HttpClient();
            string authInfo = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", userName, password)));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
            client.Timeout = new TimeSpan(0, 0, 10);
        }

        public async Task<JObject> GetAsync(string uri)
        {
            HttpResponseMessage response = await client.GetAsync(uri);
            string content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var responseMessage = "Response status code does not indicate success: " + (int)response.StatusCode;
                throw new HttpRequestException(response + Environment.NewLine + content);
            }
            return JObject.Parse(content);
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
