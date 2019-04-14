using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Automation.Models
{
    public class Coordinate
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        [JsonProperty("millisectime")]
        public int MilliSecDelayTime { get; set; }


        public Coordinate(int id, int x, int y, String name = null, int millisecdelaytime = 0)
        {
            Id = id;
            X = x;
            Y = y;
            Name = name;
            MilliSecDelayTime = millisecdelaytime;
        }

        public Coordinate()
        {
        }

        static public void Serialize(Object o, String filePath)
        {

            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            try
            {
                using (StreamWriter sw = new StreamWriter(filePath))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, o);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
