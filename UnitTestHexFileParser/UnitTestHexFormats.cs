using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HexFileParser;
using System.Linq;

namespace UnitTestHexFileParser
{
    [TestClass]
    public class UnitTestHexFormats
    {
        [TestMethod]
        public void TestIntelHexDecode1()
        {
            var p = new Parser();
            var l = p.DecodeLineWithFormat(":10010000000C0104BF04D9047F003301049F048163");
            Assert.IsTrue(l.Valid && l.Type == Parser.LineType.Data && l.Address == 0x0100);
            CollectionAssert.AreEqual(
                new byte[] { 0x00, 0x0C, 0x01, 0x04, 0xBF, 0x04, 0xD9, 0x04, 0x7F, 0x00, 0x33, 0x01, 0x04, 0x9F, 0x04, 0x81 }, l.Data);
        }

        [TestMethod]
        public void TestIntelHexDecode2()
        {
            var p = new Parser();
            var l = p.DecodeLineWithFormat(":020000021000EC");
            Assert.IsTrue(l.Valid && l.Type == Parser.LineType.Offset && l.Address == 0x010000);
        }

        [TestMethod]
        public void TestIntelHexDecode3()
        {
            var p = new Parser();
            var l = p.DecodeLineWithFormat(":00000001FF");
            Assert.IsTrue(l.Valid && l.Type == Parser.LineType.Termination && l.Address == 0x0000);
        }

        [TestMethod]
        public void TestIntelHexEncode()
        {
            var l = new Parser.Line();
            l.Data = new byte[] { 0x00, 0x0C, 0x01, 0x04, 0xBF, 0x04, 0xD9, 0x04, 0x7F, 0x00, 0x33, 0x01, 0x04, 0x9F, 0x04, 0x81 };
            l.Address = 0x0100;
            l.Type = Parser.LineType.Data;

            var p = new Parser();
            var str = p.EncodeLineWithFormat(l, Parser.HexFormat.INTEL_HEX);
            Assert.AreEqual(":10010000000C0104BF04D9047F003301049F048163\r\n", str);
        }

        [TestMethod]
        public void TestIntelHexRand()
        {
            var rnd = new Random();
            var dat = new byte[0x10];
            rnd.NextBytes(dat);

            var l_src = new Parser.Line();
            l_src.Data = dat;
            l_src.Address = (rnd.Next() & 0xffff);
            l_src.Type = Parser.LineType.Data;

            var p = new Parser();
            var str = p.EncodeLineWithFormat(l_src, Parser.HexFormat.INTEL_HEX);
            var l_dst = p.DecodeLineWithFormat(str);

            Assert.AreEqual(l_src.Address, l_dst.Address);
            CollectionAssert.AreEqual(l_src.Data, l_dst.Data);
        }

        [TestMethod]
        public void TestSrecordDecode1()
        {
            var p = new Parser();
            var l = p.DecodeLineWithFormat("S214000100000C0104BF04D9047F003301049F04815E");
            Assert.IsTrue(l.Valid && l.Address == 0x000100);
            CollectionAssert.AreEqual(
                new byte[] { 0x00, 0x0C, 0x01, 0x04, 0xBF, 0x04, 0xD9, 0x04, 0x7F, 0x00, 0x33, 0x01, 0x04, 0x9F, 0x04, 0x81 }, l.Data);
        }

        [TestMethod]
        public void TestSrecordDecode2()
        {
            var p = new Parser();
            var l = p.DecodeLineWithFormat("S01600002E5C6F75745C6D756C746970656E2E7372656362");
            Assert.IsTrue(l.Valid && l.Type == Parser.LineType.Header && l.Address == 0x0000);
            CollectionAssert.AreEqual(
                new byte[] { 0x2E, 0x5C, 0x6F, 0x75, 0x74, 0x5C, 0x6D, 0x75, 0x6C, 0x74, 0x69, 0x70, 0x65, 0x6E, 0x2E, 0x73, 0x72, 0x65, 0x63, }, l.Data);
        }
        [TestMethod]
        public void TestSrecordDecode3()
        {
            var p = new Parser();
            var l = p.DecodeLineWithFormat("S804000000FB");
            Assert.IsTrue(l.Valid && l.Type == Parser.LineType.Termination && l.Address == 0x0000);
        }
        [TestMethod]
        public void TestSrecordEncode()
        {
            var l = new Parser.Line();
            l.Data = new byte[] { 0x00, 0x0C, 0x01, 0x04, 0xBF, 0x04, 0xD9, 0x04, 0x7F, 0x00, 0x33, 0x01, 0x04, 0x9F, 0x04, 0x81 };
            l.Address = 0x0100;
            l.Type = Parser.LineType.Data;

            var p = new Parser();
            var str = p.EncodeLineWithFormat(l, Parser.HexFormat.S_RECORD);
            Assert.AreEqual("S214000100000C0104BF04D9047F003301049F04815E\r\n", str);
        }

        [TestMethod]
        public void TestSrecordRand()
        {
            var rnd = new Random();
            var dat = new byte[0x10];
            rnd.NextBytes(dat);

            var l_src = new Parser.Line();
            l_src.Data = dat;
            l_src.Address = (rnd.Next() & 0xffffff);
            l_src.Type = Parser.LineType.Data;

            var p = new Parser();
            var str = p.EncodeLineWithFormat(l_src, Parser.HexFormat.S_RECORD);
            var l_dst = p.DecodeLineWithFormat(str);

            Assert.AreEqual(l_src.Address, l_dst.Address);
            CollectionAssert.AreEqual(l_src.Data, l_dst.Data);
        }
    }
}
