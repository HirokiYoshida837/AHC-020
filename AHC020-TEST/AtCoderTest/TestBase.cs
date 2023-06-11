using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AHC020_TEST
{
    public interface ITest
    {
        public void Test(string input, string expected, Action runner);
    }


    public abstract class TestBase : ITest
    {
        // refs https://blog.yucchiy.com/2020/11/csharp-embedded-resources/

        public void Test(string input, string expected, Action runner)
        {
            // setup input/output streams.
            var inStream = new MemoryStream();
            var outBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();
            Console.SetIn(new StreamReader(inStream));
            Console.SetOut(new StringWriter(outBuilder));
            Trace.Listeners.Add(new TextWriterTraceListener(new StringWriter(errorBuilder)));
            Console.SetError(new StringWriter(errorBuilder));
            var bytes = Encoding.UTF8.GetBytes(input);
            inStream.Write(bytes, 0, bytes.Length);
            inStream.Position = 0;
            SetStreamToReader(inStream);

            // execute Main program.
            runner.Invoke();

            var res = outBuilder.ToString();

            var assertion = Assertion(res, expected);

            if (!assertion)
            {
                throw new WrongAnswerException(input, expected, res, errorBuilder.ToString());
            }
        }

        private bool Assertion(string res, string expected)
        {
            var sharpedRes = string.Join("\n",
                res.Trim((char) 0x0d, (char) 0x0a).Split('\n').Select(x => x.Trim()).Where(x => x.Length != 0));
            var sharpedExpected = string.Join("\n",
                expected.Trim((char) 0x0d, (char) 0x0a).Split('\n').Select(x => x.Trim()).Where(x => x.Length != 0));

            return sharpedRes == sharpedExpected;
        }

        private static void SetStreamToReader(Stream stream)
        {
            var reader = Type.GetType("Reader, C-Sharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            if (reader is null) return;
            var streamField = reader.GetField("Stream", BindingFlags.NonPublic | BindingFlags.Static);
            streamField.SetValue(null, stream);
        }
    }
}