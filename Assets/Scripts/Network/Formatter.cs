using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Network
{
    public static class Formatter
    {
        public static byte[] Serialize(Message data)
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            formatter.Serialize(stream, data);
            return stream.ToArray();
        }

        public static Message Deserialize(byte[] array)
        {
            var stream = new MemoryStream(array);
            var formatter = new BinaryFormatter();
            return (Message) formatter.Deserialize(stream);
        }
    }
}