using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexFileParser
{
    internal class SRecordFormat : Parser.IFormat
    {
        public Parser.Line DecodeLine(char[] str)
        {
            throw new NotImplementedException();
        }

        public char[] EncodeLine(Parser.Line l)
        {
            throw new NotImplementedException();
        }
    }
}
