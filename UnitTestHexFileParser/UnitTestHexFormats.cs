using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HexFileParser;

namespace UnitTestHexFileParser
{
    [TestClass]
    public class UnitTestHexFormats
    {
        [TestMethod]
        public void TestIntelHex1()
        {
            var p = new Parser();
            var l = p.DecodeLineWithFormat(":10010000000C0104BF04D9047F003301049F048163");
            Assert.IsTrue(l.Valid && l.Type == Parser.LineType.Data && l.Address == 0x0100);
            CollectionAssert.AreEqual(l.Data,
                new byte[] { 0x00, 0x0C, 0x01, 0x04, 0xBF, 0x04, 0xD9, 0x04, 0x7F, 0x00, 0x33, 0x01, 0x04, 0x9F, 0x04, 0x81 });
        }

        [TestMethod]
        public void TestIntelHex2()
        {
            var p = new Parser();
            var l = p.DecodeLineWithFormat(":020000021000EC");
            Assert.IsTrue(l.Valid && l.Type == Parser.LineType.Offset && l.Address == 0x100000);
        }

        [TestMethod]
        public void TestIntelHex3()
        {
            var p = new Parser();
            var l = p.DecodeLineWithFormat(":00000001FF");
            Assert.IsTrue(l.Valid && l.Type == Parser.LineType.Termination && l.Address == 0x0000);
        }

        [TestMethod]
        public void TestSrecord1()
        {
            var p = new Parser();
            var l = p.DecodeLineWithFormat("S214000100000C0104BF04D9047F003301049F04815E");
            Assert.IsTrue(l.Valid && l.Address == 0x000100);
            CollectionAssert.AreEqual(l.Data,
                new byte[] { 0x00, 0x0C, 0x01, 0x04, 0xBF, 0x04, 0xD9, 0x04, 0x7F, 0x00, 0x33, 0x01, 0x04, 0x9F, 0x04, 0x81 });
        }

        [TestMethod]
        public void TestSrecord2()
        {
            var p = new Parser();
            var l = p.DecodeLineWithFormat("S01600002E5C6F75745C6D756C746970656E2E7372656362");
            Assert.IsTrue(l.Valid && l.Type == Parser.LineType.Header && l.Address == 0x0000);
            CollectionAssert.AreEqual(l.Data,
                new byte[] { 0x2E, 0x5C, 0x6F, 0x75, 0x74, 0x5C, 0x6D, 0x75, 0x6C, 0x74, 0x69, 0x70, 0x65, 0x6E, 0x2E, 0x73, 0x72, 0x65, 0x63, });
        }
        [TestMethod]
        public void TestSrecord3()
        {
            var p = new Parser();
            var l = p.DecodeLineWithFormat("S804000000FB");
            Assert.IsTrue(l.Valid && l.Type == Parser.LineType.Termination && l.Address == 0x0000);
        }
    }
}
