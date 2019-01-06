using Network.Messages;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Network
{
    public static class Formatter
    {
        public static byte[] Serialize(ANetworkMessage data)
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            formatter.Serialize(stream, data);
            return stream.ToArray();
        }
        public static ANetworkMessage Deserialize(byte[] array)
        {
            var stream = new MemoryStream(array);
            var formatter = new BinaryFormatter();
            return (ANetworkMessage) formatter.Deserialize(stream);
        }
    }
}