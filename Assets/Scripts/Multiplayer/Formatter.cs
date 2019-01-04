using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Multiplayer
{
    public static class Formatter
    {
        public static byte[] Serialize(MultiplayerMessage data)
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            formatter.Serialize(stream, data);
            return stream.ToArray();
        }

        public static MultiplayerMessage Deserialize(byte[] array)
        {
            var stream = new MemoryStream(array);
            var formatter = new BinaryFormatter();
            return (MultiplayerMessage) formatter.Deserialize(stream);
        }
    }
}