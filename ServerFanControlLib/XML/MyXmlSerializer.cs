using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerFanControlLib.Loggers;

namespace ServerFanControlLib.XML
{
    public static class MyXmlSerializer
    {
        public static void Serialize(String filePath, Object obj)
        {
            try
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
                using (var writer = File.Open(filePath, FileMode.Create))
                {
                    serializer.Serialize(writer, obj);
                }
            }
            catch (Exception e)
            {
                ErrorLogger.Instance.Log(e);
            }
        }

        public static T DeserializeFromFile<T>(String filePath) where T : new()
        {
            T result;
            if (File.Exists(filePath))
            {
                using (var stream = File.OpenRead(filePath))
                {
                    result = Deserialize<T>(stream);
                }
            }
            else
            {
                result = new T();
            }
            return result;
        }

        public static T Deserialize<T>(String xmlText) where T : new()
        {
            T result;
            using (Stream stream = new MemoryStream(Encoding.Default.GetBytes(xmlText ?? String.Empty)))
            {
                result = Deserialize<T>(stream);
            }
            return result;
        }

        public static T Deserialize<T>(Stream xmlStream) where T : new()
        {
            try
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(xmlStream);
            }
            catch (Exception e)
            {
                ErrorLogger.Instance.Log(e);
                return new T();
            }
        }
    }
}
