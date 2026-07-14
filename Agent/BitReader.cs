using System;
using System.Collections.Generic;
using System.Text;

namespace Agent
{
    internal class BitReader(Stream stream)
    {
        public int _curBit = 8;
        public byte _curByte;
        public Stream _stream = stream;

        public byte PeekBit()
        {
            byte bit = ReadBit();
            _curBit--;
            return bit;
        }

        public byte ReadBit()
        {
            if (_curBit == 8)
            {
                int readByte = _stream.ReadByte();
                if (readByte == -1) throw new EndOfStreamException();

                _curBit = 0;
                _curByte = (byte)readByte;
            }
            byte value = (byte)((_curByte >> _curBit) & 1);

            _curBit++;
            return value;
        }

        public uint ReadBits(int bitCount)
        {
            uint output = 0;

            for (int i = 0; i < bitCount; i++)
                output |= (uint)(ReadBit() << i);

            return output;
        }
    }
}
