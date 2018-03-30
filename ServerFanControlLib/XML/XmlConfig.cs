using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ServerFanControlLib.XML
{
    [XmlRoot("FanControl", Namespace = "http://home.lab", IsNullable = false)]
    public class XmlConfig
    {
        [XmlArray(ElementName = "Servers")]
        [XmlArrayItem("Server")]
        public XmlServer[] Servers { get; set; }

        /// <summary>
        /// Interval between polling cycles in seconds.
        /// </summary>
        [XmlElement(ElementName = "Interval")]
        public Int32 Interval { get; set; }

        //[XmlArray(ElementName = "Speeds")]
        //[XmlArrayItem("Speed")]
        //public XmlSpeed[] Speeds { get; set; }

        public static XmlConfig GetDefaultConfig()
        {
            return new XmlConfig
            {
                Interval = 20,
                Servers = new XmlServer[]
                {
                    new XmlServer {Address = "192.168.0.10", Username = "root", Password = "calvin"}
                },

                // Not supported yet (needs parsing in the controller and stuff)
                //Speeds = new XmlSpeed[]
                //{
                //    new XmlSpeed {Speed = 1560, Type = XmlSpeed.SpeedType.RPM, Tempeature = 20},
                //    new XmlSpeed {Speed = 1800, Type = XmlSpeed.SpeedType.RPM, Tempeature = 22},
                //    new XmlSpeed {Speed = 2040, Type = XmlSpeed.SpeedType.RPM, Tempeature = 24},
                //    new XmlSpeed {Speed = 2520, Type = XmlSpeed.SpeedType.RPM, Tempeature = 25},
                //    new XmlSpeed {Speed = 3120, Type = XmlSpeed.SpeedType.RPM, Tempeature = 26},
                //}
            };
        }
    }

    [XmlRoot(ElementName = "Server")]
    public class XmlServer
    {
        [XmlAttribute(AttributeName = "user")]
        public String Username { get; set; }

        [XmlAttribute(AttributeName = "pass")]
        public String Password { get; set; }

        [XmlText]
        public String Address { get; set; }
    }

    /// <summary>
    /// Not supported yet
    /// </summary>
    [XmlRoot(ElementName = "Speed")]
    public class XmlSpeed
    {
        [XmlText]
        public UInt32 Speed;

        [XmlAttribute("degC")]
        public Int32 Tempeature;

        public enum SpeedType { RPM, HEX }

        [XmlAttribute("type")]
        public SpeedType Type;
    }
}
