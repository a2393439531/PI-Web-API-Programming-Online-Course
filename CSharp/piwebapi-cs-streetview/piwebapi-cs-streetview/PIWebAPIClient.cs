/***************************************************************************
   Copyright 2015 OSIsoft, LLC.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0
   
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 ***************************************************************************/

using System;
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
