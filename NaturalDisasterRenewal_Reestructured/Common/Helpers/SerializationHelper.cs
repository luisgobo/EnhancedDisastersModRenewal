using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NaturalDisasterRenewal_Reestructured.Common.Helpers
{
    public static class SerializationHelper
    {
        static readonly string path = CommonProperties.GetOptionsFilePath()+"_test.xml";
        public static void Serialize<T>(T data) {
            var serializer = new XmlSerializer(typeof(T));
            using (var writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, data);
                writer.Close();
            }
        }

        public static T Deserialize<T>() {

            object obj = null;

            if (!File.Exists(path))
                return default;

            var serializer = new XmlSerializer(typeof(T));
            using (var writer = new StreamWriter(path))
            {
                TextReader textReader = new StreamReader(path);
                obj = serializer.Deserialize(textReader);   
                textReader.Close();
            }

            return (T) obj;

        }
    }
}
