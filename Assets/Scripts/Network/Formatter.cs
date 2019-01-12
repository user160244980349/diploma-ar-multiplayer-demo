using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Network.Messages;

namespace Network
{
    public class Formatter
    {
        private BinaryFormatter _formatter;

        public Formatter()
        {
            _formatter = new BinaryFormatter();
        }
        public byte[] Serialize(ANetworkMessage data)
        {
            var _stream = new MemoryStream();
            _formatter.Serialize(_stream, data);
            return _stream.ToArray();
        }
        public ANetworkMessage Deserialize(byte[] array)
        {
            var _stream = new MemoryStream(array);
            return (ANetworkMessage) _formatter.Deserialize(_stream);
        }
    }
}