using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexFileParser
{
    internal class SRecordFormat : Parser.IFormat
    {
        public const byte CODE_HEADER = 0; // Header
        public const byte CODE_DATA16 = 1; // Data 16-bit
        public const byte CODE_DATA24 = 2; // Data 24-bit
        public const byte CODE_DATA32 = 3; // Data 32-bit
        public const byte CODE_COUNT16 = 5; // Count 16-bit (Optional)
        public const byte CODE_COUNT24 = 6; // Count 24-bit (Optional)
        public const byte CODE_TERMINATE32 = 7; // Terminate S3 records
        public const byte CODE_TERMINATE24 = 8; // Terminate S2 records
        public const byte CODE_TERMINATE16 = 9; // Terminate S1 records

        private byte[] genByteArray(char[] a)
        {
            var vals = new List<byte>();
            byte v = 0x00;
            for (int i = 1; i < a.Length; i++)
            {
                if ('0' <= a[i] && a[i] <= '9') v += (byte)(a[i] - '0');
                else if ('a' <= a[i] && a[i] <= 'f') v += (byte)(a[i] - 'a' + 0xa);
                else if ('A' <= a[i] && a[i] <= 'F') v += (byte)(a[i] - 'A' + 0xA);
                else break;

                if ((i & 1) != 1)
                {
                    v <<= 4;
                }
                else
                {
                    vals.Add(v);
                    v = 0x00;
                }
            }
            return vals.ToArray();
        }

        private bool evalChecksum(byte[] vals)
        {
            var sum = 0x00;
            // except RecordType value [0]
            for (int i = 1; i < vals.Length; i++) sum += vals[i]; 
            return (sum & 0xff) == 0xff;
        }

        public Parser.Line DecodeLine(char[] a)
        {
            var l = new Parser.Line();
            var vals = genByteArray(a);
            var type = vals[0]; // "Sx"
            var len = vals[1];
            l.Valid = (vals.Length == len + 2) && evalChecksum(vals);
            switch (type)
            {
                case CODE_HEADER:
                    l.Type = Parser.LineType.Header;
                    l.Address = (vals[2] << 8) | vals[3];
                    l.Data = new byte[len-(2+1)]; // address(2)+checksum(1)
                    Array.Copy(vals, 4, l.Data, 0, l.Data.Length);
                    break;
                case CODE_DATA16:
                case CODE_TERMINATE16:
                    l.Type = (type == CODE_DATA16) ? Parser.LineType.Data : Parser.LineType.Termination;
                    l.Address = (vals[2] << 8) | vals[3];
                    l.Data = new byte[len - (2 + 1)]; // address(2)+checksum(1)
                    Array.Copy(vals, 4, l.Data, 0, l.Data.Length);
                    break;
                case CODE_DATA24:
                case CODE_TERMINATE24:
                    l.Type = (type == CODE_DATA24) ? Parser.LineType.Data : Parser.LineType.Termination;
                    l.Address = (vals[2] << 16) | (vals[3] << 8) | vals[4];
                    l.Data = new byte[len - (3 + 1)]; // address(3)+checksum(1)
                    Array.Copy(vals, 5, l.Data, 0, l.Data.Length);
                    break;
                case CODE_DATA32:
                case CODE_TERMINATE32:
                    l.Type = (type == CODE_DATA32)? Parser.LineType.Data: Parser.LineType.Termination;
                    l.Address = (vals[2] << 24) | (vals[3] << 16) | (vals[4] << 8) | vals[5];
                    l.Data = new byte[len - (4 + 1)]; // address(4)+checksum(1)
                    Array.Copy(vals, 6, l.Data, 0, l.Data.Length);
                    break;
                case CODE_COUNT16:
                case CODE_COUNT24:
                    throw new NotImplementedException();
            }
            return l;
        }


        private byte genChecksum(IEnumerable<byte> vals)
        {
            var sum = 0x00;
            for (var i = 1; i < vals.Count(); i++) sum += vals.ElementAt(i);
            return (byte)(~sum & 0xff);
        }

        private char[] genCharArray(IEnumerable<byte> vals)
        {
            var a = new List<char>();
            a.Add('S');
            a.Add(vals.ElementAt(0).ToString("X1").ToCharArray()[0]);
            for (var i = 1; i < vals.Count(); i++)
            {
                foreach (var c in vals.ElementAt(i).ToString("X2").ToCharArray())
                    a.Add(c);
            }
            return a.ToArray();
        }

        public char[] EncodeLine(Parser.Line l)
        {
            var vals = new List<byte>();
            var len = (byte)(l.Data.Count() + 4);
            switch (l.Type)
            {
                case Parser.LineType.Header:
                    vals.Add(CODE_HEADER);
                    vals.Add(len);
                    vals.Add(0x00);
                    vals.Add(0x00);
                    break;
                case Parser.LineType.Data:
                    vals.Add(CODE_DATA24);
                    vals.Add(len);
                    vals.Add((byte)(l.Address >> 16));
                    vals.Add((byte)(l.Address >> 8));
                    vals.Add((byte)(l.Address & 0xff));
                    break;
                case Parser.LineType.Offset:
                    return null; // no definition
                case Parser.LineType.Termination:
                    vals.Add(CODE_TERMINATE24);
                    vals.Add(len);
                    vals.Add(0x00);
                    vals.Add(0x00);
                    vals.Add(0x00);
                    break;
                default:
                    throw new NotImplementedException();
            }
            vals = vals.Concat(l.Data).ToList();
            vals.Add(genChecksum(vals));

            return genCharArray(vals);
        }
    }
}
