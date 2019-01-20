using System.Text;
using System;

namespace Network
{
    public class NetworkIdGenerator
    {
        private int _size;
        private Random _random;
        private StringBuilder _builder;

        public NetworkIdGenerator(int size)
        {
            _size = size;
            _builder = new StringBuilder(_size);
            _random = new Random((int)DateTime.Now.Ticks);
        }
        public string Generate()
        {
            char c;
            for (int i = 0; i < _size; i++)
            {
                c = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * _random.NextDouble() + 65)));
                _builder.Append(c);
            }
            return _builder.ToString();
        }
    }
}
