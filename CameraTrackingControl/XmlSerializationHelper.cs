using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CameraTrackingControl
{
    public static class XmlSerializationHelper
    {
        public static string SerializeObject<T>(T obj, string ns)
        {
            var serializer = new XmlSerializer(typeof(T), ns);
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                return writer.ToString();
            }
        }

        public static T DeserializeObject<T>(string xml, string ns)
        {
            var serializer = new XmlSerializer(typeof(T), ns);
            using (var reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
    }
}
