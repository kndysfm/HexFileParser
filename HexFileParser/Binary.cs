using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("UnitTestHexFileParser")]

namespace HexFileParser
{
    public class Binary
    {
        internal class Block
        {
            private byte[] _data;
            internal IEnumerable<byte> Data { get { return _data; } }
            internal int StartAddress { get; private set; }
            internal int EndAddress { get { return StartAddress + Length; } }
            internal int Length { get { return _data.Length; } }

            internal Block(int address, byte[] data)
            {
                StartAddress = address;
                _data = (byte[]) data.Clone();
            }

            internal void Append(byte[] data)
            {
                _data = _data.Concat(data).ToArray();
            }
        }

        public static byte DefaultValueInBlank { get; set; }
        static Binary()
        {
            DefaultValueInBlank = 0xff;
        }

        public byte ValueInBlank { get; set; }
        internal List<Block> Blocks { get; private set; }
        private bool _terminated = false;
        
        internal Binary()
        {
            ValueInBlank = DefaultValueInBlank;
            Blocks = new List<Block>();
        }

        public byte[] GetData(int startAddress, int length)
        {
            var region_start = startAddress;
            var region_end = startAddress + length;
            // filter
            var blocks = Blocks.Where(b => region_start < b.StartAddress + b.Length && b.StartAddress < region_end).ToList();
            blocks.Sort((a, b) => a.StartAddress - b.StartAddress);

            var data = Enumerable.Repeat<byte>(ValueInBlank, length).ToArray(); // fill by blank value
            
            foreach(var b in blocks)
            {
                int addr_start = Math.Max(region_start, b.StartAddress);
                int addr_end = Math.Min(region_end, b.StartAddress + b.Length);
                int len_cpy = addr_end - addr_start ;
                Array.Copy(b.Data.ToArray(), addr_start - b.StartAddress, data, addr_start - region_start, len_cpy);
            }
            
            return data.ToArray();
        }

        internal void AddData(int startAddress, byte[] data)
        {
            if (_terminated) return;

            var blk = Blocks.FirstOrDefault(b => b.EndAddress == startAddress);
            if (blk != null)
            {
                blk.Append(data);
            }
            else
            {
                Blocks.Add(new Block(startAddress, data));
            }
        }

        internal void SetTermination()
        {
            _terminated = true;
        }
    }
}
