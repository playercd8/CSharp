#define _EnableObjectSerializer
#define _EnableXmlSerializer
#define _EnableJsonSerializer

#if (_EnableObjectSerializer)
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
#endif

#if (_EnableXmlSerializer)
    using System.Xml.Serialization;
#endif

#if (_EnableJsonSerializer)

#endif

namespace com.data
{
    class Serializer
    {
        public Serializer()
        {
        }

#if (_EnableObjectSerializer)
        IFormatter _binaryFormatter = new BinaryFormatter();

        public byte[] SerializeFromObject(object src)
        {
            MemoryStream stream = new MemoryStream();
            _binaryFormatter.Serialize(stream, src);
            return stream.ToArray();
        }

        public object DeserializeToObject(byte[] src)
        {
            MemoryStream stream = new MemoryStream(src);
            stream.Seek(0, SeekOrigin.Begin);
            return _binaryFormatter.Deserialize(stream);
        }
#endif

#if (_EnableXmlSerializer)

        public string SerializeFrom<T>(T src)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            MemoryStream stream = new MemoryStream();
             TextWriter writer = new StreamWriter(stream, new UTF8Encoding());
            serializer.Serialize(writer, src);

        }

        public T DeserializeTo<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(new StringReader(xml));
        }

#endif

#if (_EnableJsonSerializer)
#endif
    }
}