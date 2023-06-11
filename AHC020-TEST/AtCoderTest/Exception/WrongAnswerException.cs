using System;

namespace AHC020_TEST
{
    [Serializable]
    public class WrongAnswerException : System.Exception
    {
        public string Input;
        public string Expected;
        public string Output;
        public string Debug;

        public override string Message
        {
            get
            {
                return $"{(Input is null ? "" : $"\nInput: \n{Input.Trim()}")}" +
                       $"{(Expected is null ? "" : $"\nExpected: \n{Expected.Trim()}")}" +
                       $"{(Output is null ? "" : $"\nOutput: \n{Output.Trim()}")}" +
                       $"{(Debug is null ? "" : $"\nDebug: \n{Debug.Trim()}")}";
            }
        }

        public WrongAnswerException(string input, string expected, string output, string debug)
        {
            Input = input;
            Expected = expected;
            Output = output;
            Debug = debug;
        }

        protected WrongAnswerException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}