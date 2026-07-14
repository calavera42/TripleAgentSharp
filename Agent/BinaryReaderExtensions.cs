using System;
using System.Collections.Generic;
using System.Text;

namespace Agent
{
    internal static class BinaryReaderExtensions
    {
        public static string ReadCString(this BinaryReader br)
        {
            int length = (int)br.ReadUInt32();

            if (length == 0)
                return string.Empty;

            string output = Encoding.Unicode.GetString(br.ReadBytes(length * 2));

            br.ReadBytes(2); // a string tem um null terminator que não consta no length.

            return output;
        }

        public static long GetPos(this BinaryReader br) => br.BaseStream.Position;

        public static long Seek(this BinaryReader br, long offset, SeekOrigin origin) => br.BaseStream.Seek(offset, origin);

        public static Guid ReadGuid(this BinaryReader br) => new(br.ReadBytes(16));
    }
}
