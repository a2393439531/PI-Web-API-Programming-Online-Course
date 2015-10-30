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
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using Newtonsoft.Json.Linq;

namespace piwebapi_cs_streetview
{
    public partial class Form1 : Form
    {
        static string streetViewBaseUrl = "https://maps.googleapis.com/maps/api/streetview";
        static string baseUrl = "https://dng-lemon-web.lemon.local/piwebapi";

        private PIWebAPIClient client;
        private StreetViewEvent svEvent;
        private int currentEventCount;

        public Form1()
        {
            InitializeComponent();
            client = new PIWebAPIClient("admina", ConfigurationManager.AppSettings["password"]);
            currentEventCount = 0;
            prevBtn.Enabled = false;
            nextBtn.Enabled = false;
            GetEventFrames();
        }

        private void searchBtn_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("{0}?size={1}x{2}", streetViewBaseUrl, webBrowser1.Width - 40, webBrowser1.Height - 40));

            if (searchTb.Text != string.Empty)
            {
                sb.Append(string.Format("&location={0}", searchTb.Text));
                webBrowser1.Navigate(sb.ToString());
            }
            else
            {
                MessageBox.Show("Please provide a location to search.");
            }
        }

        private async void GetEventFrames()
        {
            try
            {
                // Get AF database WebId
                string afServerName = "DNG-LEMON-AF";
                string afDatabaseName = "GoogleStreetView";
                string url = string.Format(@"{0}/assetdatabases?path=\\{1}\{2}", baseUrl, afServerName, afDatabaseName);
                JObject jobj = await client.GetAsync(url);
                string webId = jobj["WebId"].ToString();

                // Get Event Frames
                string templateName = "StreetView_EFTemplate";
                url = string.Format(@"{0}/eventframes/{1}/eventframes?templatename={2}&sortfield=starttime&sortorder=descending&starttime=*-1d",
                                            baseUrl, webId, templateName);
                jobj = await client.GetAsync(url);
                
                // Populate combo box
                for (int i = 0; i < jobj["Items"].Count(); i++)
                {
                    StreetViewEvent svEvent = new StreetViewEvent(
                        jobj["Items"][i]["Name"].ToString(),
                        jobj["Items"][i]["StartTime"].ToString(),
                        jobj["Items"][i]["EndTime"].ToString(),
                        jobj["Items"][i]["WebId"].ToString()
                        );
                    eventFrameCb.Items.Add(svEvent);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            
        }

        private async void eventFrameCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                svEvent = (StreetViewEvent)eventFrameCb.SelectedItem;
                
                if (!svEvent.ValuesLoaded)
                {
                    // Load timestamp/value information
                    string url = string.Format("{0}/streamsets/{1}/recorded", baseUrl, svEvent.WebId);
                    JObject jobj = await client.GetAsync(url);
                    LoadEventDetail(jobj);
                }

                // Display the first image
                currentEventCount = 0;
                StreetViewQuery();
                ButtonCheck();

                // Populate start and end time
                startTimeLbl.Text = svEvent.StartTime.ToLocalTime().ToString();
                endTimeLbl.Text = svEvent.EndTime.ToLocalTime().ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadEventDetail(JObject obj)
        {
            JToken latitudesJobj = obj["Items"].Where(i => i["Name"].Value<string>() == "Latitude").FirstOrDefault();
            JToken longitudesJobj = obj["Items"].Where(i => i["Name"].Value<string>() == "Longitude").FirstOrDefault();
            JToken headingsJobj = obj["Items"].Where(i => i["Name"].Value<string>() == "Heading").FirstOrDefault();
            JToken pitchesJobj = obj["Items"].Where(i => i["Name"].Value<string>() == "Pitch").FirstOrDefault();

            svEvent.Timestamps = latitudesJobj["Items"].Select(i => DateTime.Parse(i["Timestamp"].ToString())).ToList();

            svEvent.Latitudes = latitudesJobj["Items"].Select(i => Convert.ToDouble(i["Value"])).ToList();
            svEvent.Longitudes = longitudesJobj["Items"].Select(i => Convert.ToDouble(i["Value"])).ToList();
            svEvent.Headings = headingsJobj["Items"].Select(i => Convert.ToDouble(i["Value"])).ToList();
            svEvent.Pitches = pitchesJobj["Items"].Select(i => Convert.ToDouble(i["Value"])).ToList();

            svEvent.ValuesLoaded = true;
        }

        private void StreetViewQuery()
        {
            StringBuilder query = new StringBuilder();
            query.Append(string.Format("{0}?size={1}x{2}", streetViewBaseUrl, webBrowser1.Width - 40, webBrowser1.Height - 40));
            query.Append(string.Format("&location={0},{1}&heading={2}&pitch={3}",
                                        svEvent.Latitudes[currentEventCount],
                                        svEvent.Longitudes[currentEventCount],
                                        svEvent.Headings[currentEventCount],
                                        svEvent.Pitches[currentEventCount]
                                        ));
            webBrowser1.Navigate(query.ToString());
            imageLbl.Text = string.Format("{0} of {1}", currentEventCount + 1, svEvent.Timestamps.Count);
            timeLbl.Text = svEvent.Timestamps[currentEventCount].ToLocalTime().ToString();
        }

        private void ButtonCheck()
        {
            int totalCount = svEvent.Timestamps.Count;
            prevBtn.Enabled = (currentEventCount == 0) ? false : true;
            nextBtn.Enabled = ((totalCount - currentEventCount) == 1) ? false : true;
        }

        private void nextBtn_Click(object sender, EventArgs e)
        {
            if ((svEvent != null) && ((svEvent.Timestamps.Count - currentEventCount) > 1))
            {
                currentEventCount++;
                StreetViewQuery();
            }
            ButtonCheck();
        }

        private void prevBtn_Click(object sender, EventArgs e)
        {
            if ((svEvent != null) && (currentEventCount > 0))
            {
                currentEventCount--;
                StreetViewQuery();
            }
            ButtonCheck();
        }
    }
}
