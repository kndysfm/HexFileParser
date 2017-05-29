using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HexFileParser;

namespace UnitTestHexFileParser
{
    [TestClass]
    public class UnitTestHexBinary
    {
        [TestMethod]
        public void TestMethod1()
        {
            byte blank = 0xAA;
            Binary.DefaultValueInBlank = blank;
            var bin = new Binary();
            bin.AddData(0x0000, new byte[] { 0x01, 0x01, 0x01 });
            bin.AddData(0x0004, new byte[] { 0x02, 0x02 });

            var data = bin.GetData(0x00, 0x06); 
            CollectionAssert.AreEqual(data, new byte[] { 0x01, 0x01, 0x01, blank, 0x02, 0x02 });
        }

        [TestMethod]
        public void TestMethod2()
        {
            var bin = new Binary();
            bin.AddData(0x0004, new byte[] { 0x02, 0x02 });
            bin.AddData(0x0008, new byte[] { 0x03, 0x03, 0x03, 0x03 });
            bin.AddData(0x000C, new byte[] { 0x04 });
            var blank = bin.ValueInBlank = 0x55;

            var data = bin.GetData(0x06, 0x08);
            CollectionAssert.AreEqual(data, new byte[] { blank, blank,  0x03, 0x03, 0x03, 0x03,  0x4, blank, });
        }

        [TestMethod]
        public void TestMethod3()
        {
            var bin = new Binary();
            var blank = bin.ValueInBlank = 0x55;
            var data = bin.GetData(0x00, 0x04);
            CollectionAssert.AreEqual(data, new byte[] {blank, blank, blank, blank});
        }
    }
}
