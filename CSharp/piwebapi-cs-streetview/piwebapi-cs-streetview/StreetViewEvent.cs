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
using System.Collections.Generic;

namespace piwebapi_cs_streetview
{
    public class StreetViewEvent
    {
        public string Name { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string WebId { get; set; }

        public bool ValuesLoaded { get; set; }

        public IList<DateTime> Timestamps { get; set; }

        public IList<double> Latitudes { get; set; }

        public IList<double> Longitudes { get; set; }

        public IList<double> Headings { get; set; }

        public IList<double> Pitches { get; set; }

        public StreetViewEvent(string name, string sTime, string eTime, string webId)
        {
            Name = name;
            StartTime = DateTime.Parse(sTime);
            EndTime = DateTime.Parse(eTime);
            WebId = webId;
            ValuesLoaded = false;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
