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
            public bool Valid;
        }

        internal interface IFormat
        {
            Line DecodeLine(char[] str);
            char[] EncodeLine(Line l);
        }

        private IFormat _ihex;
        private IFormat _srec;

        public Parser()
        {
            _ihex = new InterlHexFormat();
            _srec = new SRecordFormat();
            ResetDecoding();
        }

        private int _addressOffset;
        public Binary DecodedData { get; private set; }
        public void ResetDecoding()
        {
            _addressOffset = 0x0;
            DecodedData = new Binary();
        }

        internal Line DecodeLineWithFormat(string str)
        {
            Line l;
            var a = str.ToCharArray();
            switch (a[0])
            {
                case ':':
                    l = _ihex.DecodeLine(a);
                    break;
                case 'S':
                    l = _srec.DecodeLine(a);
                    break;
                default: return null;
            }
            return l;
        }

        public void DecodeLine(string str)
        {
            Line l = DecodeLineWithFormat(str);
            if (l == null) return;

            switch (l.Type)
            {
                case LineType.Header:
                    _addressOffset = 0x0;
                    break;
                case LineType.Data:
                    DecodedData.AddData(l.Address + _addressOffset, l.Data);
                    break;
                case LineType.Offset:
                    _addressOffset = l.Address;
                    break;
                case LineType.Termination:
                    DecodedData.SetTermination();
                    break;
                default: return;
            }
        }
    }
}
