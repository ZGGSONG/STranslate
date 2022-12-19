using STranslate.Model;
using System;
using System.IO;
using YamlDotNet.Serialization;

namespace STranslate.Utils
{
    public class ConfigUtil
    {
        public static ConfigModel ReadConfig(string path)
        {
            using (TextReader reader = File.OpenText(path))
            {
                try
                {
                    var config = new ConfigModel();
                    var deserializer = new Deserializer();
                    config = deserializer.Deserialize<ConfigModel>(reader);
                    return config;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static void WriteConfig(string path, ConfigModel configs)
        {
            var serializer = new Serializer();
            StringWriter strWriter = new StringWriter();

            serializer.Serialize(strWriter, configs);
            serializer.Serialize(Console.Out, configs);

            using (TextWriter writer = File.CreateText(path))
            {
                writer.Write(strWriter.ToString());
            }
        }
    }
}