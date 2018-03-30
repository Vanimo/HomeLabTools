using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerFanControlLib.XML
{
    public class XmlHelper
    {
        public const String ConfigFileName = "Config.xml";

        public static XmlConfig GetOrCreateConfig()
        {
            if (File.Exists(ConfigFileName))
            {
                return MyXmlSerializer.DeserializeFromFile<XmlConfig>(ConfigFileName);
            }

            var config = XmlConfig.GetDefaultConfig();
            MyXmlSerializer.Serialize(ConfigFileName, config);
            return config;
        }
    }
}
