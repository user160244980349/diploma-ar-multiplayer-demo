using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Network
{
    public static class Formatter
    {
        public static byte[] Serialize(NetworkMessage data)
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            formatter.Serialize(stream, data);
            return stream.ToArray();
        }

        public static NetworkMessage Deserialize(byte[] array)
        {
            var stream = new MemoryStream(array);
            var formatter = new BinaryFormatter();
            return (NetworkMessage) formatter.Deserialize(stream);
        }
    }
}