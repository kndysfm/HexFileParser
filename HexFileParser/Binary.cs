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

            internal void Copy(int address, byte[] src, int idx_src, int length)
            {
                var idx_dst = address - StartAddress;
                for (int i = 0; i < length; i++)
                {
                    _data[idx_dst++] = src[idx_src++];
                }
            }
        }

        public static byte DefaultValueInBlank { get; set; }
        static Binary()
        {
            DefaultValueInBlank = 0xff;
        }

        public byte ValueInBlank { get; set; }
        internal List<Block> Blocks { get; private set; }
        public bool IsTerminated { get; internal set; }
        public string HeaderString { get; set; }
        
        internal Binary()
        {
            ValueInBlank = DefaultValueInBlank;
            Blocks = new List<Block>();
            IsTerminated = false;
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
            if (IsTerminated) return; // cannot add data anymore

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

        public bool ModifyData(int startAddress, byte[] src, int length)
        {
            return ModifyData(startAddress, src, 0, length);
        }

        public bool ModifyData(int startAddress, byte[] src, int indexSrc, int length)
        {
            if (!IsTerminated) return false; // loading is not completed
            if (length > src.Length - indexSrc) return false;

            var region_start = startAddress;
            var region_end = startAddress + length;
            var blk = Blocks.FirstOrDefault(b => b.StartAddress <= region_start && region_end <= b.EndAddress);
            if (blk == null) return false;

            blk.Copy(startAddress, src, indexSrc, length);
            return true;
        }
    }
}
