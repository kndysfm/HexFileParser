using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("UnitTestHexFileParser")]

namespace HexFileParser
{
    public class Parser
    {
        internal enum LineType
        {
            Undefined, Header, Data, Offset, Termination
        }

        internal class Line
        {
            public LineType Type { set; get; }
            public int Address { set; get; }
            public byte[] Data { get; set; }
            public bool Valid { set; get; }
        }

        internal interface IFormat
        {
            Line DecodeLine(char[] str);
            char[] EncodeLine(Line l);
        }

        private IFormat _ihex;
        private IFormat _srec;

        public enum HexFormat
        {
            INTEL_HEX = ':', S_RECORD = 'S'
        }

        public Parser()
        {
            _ihex = new IntelHexFormat();
            _srec = new SRecordFormat();
            Reset();
        }

        private int _addressOffset;
        public Binary DecodedData { get; private set; }
        public void Reset()
        {
            _addressOffset = 0x0;
            DecodedData = new Binary();
        }

        internal Line DecodeLineWithFormat(string str)
        {
            Line l = null;
            var a = str.ToCharArray();
            if (a.Length != 0)
            {
                switch (a[0])
                {
                    case ':':
                        l = _ihex.DecodeLine(a);
                        break;
                    case 'S':
                        l = _srec.DecodeLine(a);
                        break;
                }
            }
            return l;
        }

        public void DecodeLine(string str)
        {
            Line l = DecodeLineWithFormat(str);
            if (l == null || !l.Valid) return;
            switch (l.Type)
            {
                case LineType.Header:
                    _addressOffset = 0x0;
                    DecodedData.HeaderString = new String(l.Data.Select(b => (char)b).ToArray());
                    break;
                case LineType.Data:
                    DecodedData.AddData(l.Address + _addressOffset, l.Data);
                    break;
                case LineType.Offset:
                    _addressOffset = l.Address;
                    break;
                case LineType.Termination:
                    DecodedData.IsTerminated = true;
                    _addressOffset = 0x0;
                    break;
                default: return;
            }
        }

        public void DecodeLines(string str)
        {
            var lns = str.Split('\n');
            foreach (var l in lns) DecodeLine(l);
        }

        internal string EncodeLineWithFormat(Line l, HexFormat fmt)
        {
            if (l.Type == LineType.Data)
                l.Address = l.Address - _addressOffset; // cuurent offset
            else if (l.Type == LineType.Offset)
                _addressOffset = l.Address;

            char[] a = null;
            switch(fmt)
            {
                case HexFormat.INTEL_HEX:
                    a = _ihex.EncodeLine(l);
                    break;
                case HexFormat.S_RECORD:
                    a = _srec.EncodeLine(l);
                    break;
            }
            return (a == null)? "": new String(a) + "\r\n";
        }

        public string EncodeStoredData(HexFormat fmt, byte lineLength)
        {
            var str = "";
            Line l = new Line();
            l.Valid = true;
            foreach (var blk in DecodedData.Blocks)
            {
                int remains = blk.Length;
                int addr = blk.StartAddress;
                if ((addr & 0xffff0000) != 0)
                {
                    l.Type = LineType.Offset;
                    l.Address = (int)(addr & 0xffff0000);
                    l.Data = new byte[] { };
                    str += EncodeLineWithFormat(l, fmt);
                }
                while (remains > 0)
                {
                    l.Type = LineType.Data;
                    l.Address = addr;
                    l.Data = DecodedData.GetData(addr, Math.Min(lineLength, remains));
                    str += EncodeLineWithFormat(l, fmt);
                    remains -= l.Data.Length;
                    addr += l.Data.Length;
                }
            }
            l.Type = LineType.Termination;
            l.Address = 0x0;
            l.Data = new byte[] { };
            str += EncodeLineWithFormat(l, fmt);

            return str;
        }
    }
}
