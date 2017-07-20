using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexFileParser
{
    internal class IntelHexFormat : Parser.IFormat
    {
        public const byte CODE_DATA = 0x00; // Data
        public const byte CODE_EOF = 0x01; // End Of Filr
        public const byte CODE_EX_SEG_ADDR = 0x02; // Extended Segment Address
        public const byte CODE_ST_SEG_ADDR = 0x03; // Start Segment Address
        public const byte CODE_EX_LIN_ADDR = 0x04; // Extended Linear Address
        public const byte CODE_ST_LIN_ADDR = 0x05; // Start Linear Address

        private uint _regCS, _regIP, _regEIP; // for 80x86 system (ignored)

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

                if ((i & 1) == 1)
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
            foreach (var v in vals) sum += v;
            return (sum & 0xff) == 0x00;
        }

        public Parser.Line DecodeLine(char[] a)
        {
            var l = new Parser.Line();
            var vals = genByteArray(a);
            var len = vals[0];
            l.Valid = (vals.Length == len + 5) && evalChecksum(vals);
            var addr = (vals[1] << 8) | vals[2];

            switch (vals[3])
            {
                case CODE_DATA:
                    l.Type = Parser.LineType.Data;
                    l.Address = addr;
                    l.Data = new byte[len];
                    Array.Copy(vals, 4, l.Data, 0, len);
                    break;
                case CODE_EOF:
                    l.Type = Parser.LineType.Termination;
                    break;
                case CODE_EX_SEG_ADDR:
                    l.Type = Parser.LineType.Offset;
                    if (len != 2) l.Valid = false;
                    else l.Address = (vals[4] << 12) | (vals[5] << 4);
                    break;
                case CODE_ST_SEG_ADDR:
                    if (len != 4) l.Valid = false;
                    else
                    {
                        _regCS = ((uint)vals[4] << 8) | ((uint)vals[5] << 0);
                        _regIP = ((uint)vals[6] << 8) | ((uint)vals[7] << 0);
                    }
                    break;
                case CODE_EX_LIN_ADDR:
                    l.Type = Parser.LineType.Offset;
                    if (len != 2) l.Valid = false;
                    else l.Address = (vals[4] << 24) | (vals[5] << 16);
                    break;
                case CODE_ST_LIN_ADDR:
                    if (len != 2) l.Valid = false;
                    else _regEIP = ((uint)vals[4] << 24) | ((uint)vals[5] << 16)| ((uint)vals[6] << 8) | ((uint)vals[7] << 0);
                    break;
            }

            return l;
        }

        private byte genChecksum(IEnumerable<byte> vals)
        {
            var sum = 0x00;
            foreach (var v in vals) sum += v;
            return (byte) ((0x00 - sum) & 0xff);
        }

        private char[] genCharArray(IEnumerable<byte> vals)
        {
            var a = new List<char>();
            a.Add(':');
            foreach(var v in vals)
            {
                foreach (var c in v.ToString("X2").ToCharArray())
                    a.Add(c);
            }
            return a.ToArray();
        }

        public char[] EncodeLine(Parser.Line l)
        {
            var vals = new List<byte>();
            switch (l.Type)
            {
                case Parser.LineType.Header:
                    return null; // no definition
                case Parser.LineType.Data:
                    vals.Add((byte)l.Data.Count());
                    vals.Add((byte)(l.Address >> 8));
                    vals.Add((byte)(l.Address & 0xff));
                    vals.Add(CODE_DATA);
                    vals = vals.Concat(l.Data).ToList();
                    break;
                case Parser.LineType.Offset:
                    vals.Add(0x02); // byte count
                    vals.Add(0x00); // address H
                    vals.Add(0x00); // address L
                    if ((l.Address & 0xFFF00000) != 0)
                    {   // xxxx0000H
                        vals.Add(CODE_EX_LIN_ADDR);
                        vals.Add((byte)(l.Address >> 24));
                        vals.Add((byte)(l.Address >> 16));
                    }
                    else
                    {   // 000xxxx0H
                        vals.Add(CODE_EX_SEG_ADDR);
                        vals.Add((byte)(l.Address >> 12));
                        vals.Add((byte)(l.Address >> 4));
                    }
                    break;
                case Parser.LineType.Termination:
                    vals.Add(0x00); // byte count
                    vals.Add(0x00); // address H
                    vals.Add(0x00); // address L
                    vals.Add(CODE_EOF);
                    break;
                default:
                    throw new NotImplementedException();
            }
            vals.Add(genChecksum(vals));

            return genCharArray(vals);
        }
    }
}
