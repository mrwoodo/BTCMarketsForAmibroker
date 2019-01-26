using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BTC.RealTimeDataSource
{
    /// <summary>
    /// Adapted with sample code from .Net for Amibroker http://www.dotnetforab.com/
    /// </summary>
    [XmlRoot(Namespace = "BTC.RealTimeDataSource", IsNullable = false)]
    public class Configuration
    {
        public int RefreshPeriod;
        public bool VerboseLog;

        public static string GetConfigString(Configuration configuration)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Configuration));

            Stream stream = new MemoryStream();
            serializer.Serialize(XmlWriter.Create(stream), configuration);

            stream.Seek(0, SeekOrigin.Begin);
            StreamReader streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }

        public static Configuration GetConfigObject(string config)
        {
            if (string.IsNullOrEmpty(config) || config.Trim().Length == 0)
                return GetDefaultConfigObject();

            XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
            Stream stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(config));

            try
            {
                return (Configuration)serializer.Deserialize(stream);
            }
            catch (Exception)
            {
                return GetDefaultConfigObject();
            }
        }

        public static Configuration GetDefaultConfigObject()
        {
            Configuration defConfig = new Configuration();

            defConfig.RefreshPeriod = 10;
            defConfig.VerboseLog = false;

            return defConfig;
        }
    }
}
