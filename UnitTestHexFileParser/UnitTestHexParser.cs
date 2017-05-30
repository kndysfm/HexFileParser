using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HexFileParser;

namespace UnitTestHexFileParser
{
    [TestClass]
    public class UnitTestHexParser
    {
        static Random _rnd = new Random();

        [TestMethod]
        public void TestMethodParseLines()
        {
            var str =
                "S0160000752E7565632E706D6F5C69736C74746E5C726562\r\n" +
                "S214100100000C0104BF04D9047F003301049F04814E\r\n" +
                "S214100110049D0033000509051B052700330005293B\r\n" +
                "S2141001200539053F000F010673065A0658000001F0\r\n" +
                "S2141001300967091109060005010E3B0E790DB60078\r\n" +
                "S20C10014005010E120DBA0DB8F0\r\n" +
                "S804000000FB\r\n";

            var p = new Parser();
            p.DecodeLines(str);

            var dat = p.DecodedData.GetData(0x100140, 8);
            CollectionAssert.AreEqual(
                new byte[] { 0x05, 0x01, 0x0E, 0x12, 0x0D, 0xBA, 0x0D, 0xB8 }, dat);
        }
        [TestMethod]
        public void TestMethodEncoding()
        {
            var str =
                ":020000021000EC\r\n" +
                ":10010000000C0104BF04D9047F003301049F048163\r\n" +
                ":10011000049D0033000509051B0527003300052950\r\n" +
                ":100120000539053F000F010673065A065800000105\r\n" +
                ":100130000967091109060005010E3B0E790DB6008D\r\n" +
                ":0801400005010E120DBA0DB805\r\n" +
                ":00000001FF\r\n";

            var p = new Parser();
            p.DecodeLines(str);
            var hex = p.EncodeStoredData(Parser.HexFormat.INTEL_HEX, 0x10);
            Assert.AreEqual(str, hex);
        }
        [TestMethod]
        public void TestMethodIhexToSrec()
        {
            var str =
                ":020000021000EC\r\n" +
                ":10010000000C0104BF04D9047F003301049F048163\r\n" +
                ":10011000049D0033000509051B0527003300052950\r\n" +
                ":100120000539053F000F010673065A065800000105\r\n" +
                ":100130000967091109060005010E3B0E790DB6008D\r\n" +
                ":0801400005010E120DBA0DB805\r\n" +
                ":00000001FF\r\n";

            var p = new Parser();
            p.DecodeLines(str);
            var hex = p.EncodeStoredData(Parser.HexFormat.S_RECORD, (byte)_rnd.Next(8, 30));
            p.DecodeLines(hex);
            var dat = p.DecodedData.GetData(0x100120, 8);
            CollectionAssert.AreEqual(
                new byte[] { 0x05, 0x39, 0x05, 0x3F, 0x00, 0x0F, 0x01, 0x06 }, dat);
        }
        [TestMethod]
        public void TestMethodSrecToIhex()
        {
            var str =
                "S0160000752E7565632E706D6F5C69736C74746E5C726562\r\n" +
                "S214100100000C0104BF04D9047F003301049F04814E\r\n" +
                "S214100110049D0033000509051B052700330005293B\r\n" +
                "S2141001200539053F000F010673065A0658000001F0\r\n" +
                "S2141001300967091109060005010E3B0E790DB60078\r\n" +
                "S20C10014005010E120DBA0DB8F0\r\n" +
                "S804000000FB\r\n";

            var p = new Parser();
            p.DecodeLines(str);
            var hex = p.EncodeStoredData(Parser.HexFormat.INTEL_HEX, (byte)_rnd.Next(8, 30));
            p.DecodeLines(hex);
            var dat = p.DecodedData.GetData(0x100120, 8);
            CollectionAssert.AreEqual(
                new byte[] { 0x05, 0x39, 0x05, 0x3F, 0x00, 0x0F, 0x01, 0x06 }, dat);
        }

        [TestMethod]
        public void TestMethodModifying()
        {
            var str =
                ":020000021000EC\r\n" +
                ":10010000000C0104BF04D9047F003301049F048163\r\n" +
                ":10011000049D0033000509051B0527003300052950\r\n" +
                ":100120000539053F000F010673065A065800000105\r\n" +
                ":100130000967091109060005010E3B0E790DB6008D\r\n" +
                ":0801400005010E120DBA0DB805\r\n" +
                ":00000001FF\r\n";

            var p = new Parser();
            p.DecodeLines(str);
            p.DecodedData.ModifyData(0x100142,
                new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44 }, 2, 3);

            var dat = p.DecodedData.GetData(0x100140, 8);
            CollectionAssert.AreEqual(
                new byte[] { 0x05, 0x01, 0x22, 0x33, 0x44, 0xBA, 0x0D, 0xB8 }, dat);
        }
    }
}
