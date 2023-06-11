using System;
using System.IO;
using NUnit.Framework;

namespace AHC020_TEST
{
    public class TestClass : TestBase
    {
        [Test]
        [TestCase("1")]
        [TestCase("2")]
        [TestCase("3")]
        public void Test1(string path)
        {
            var input = File.ReadAllText($@"Resources\Cases\{path}\input.txt");
            var expected = File.ReadAllText($@"Resources\Cases\{path}\expected.txt");

            // Test(input, expected, () => { ABC234A.Program.Main(new String[] { }); });
        }
    }
}