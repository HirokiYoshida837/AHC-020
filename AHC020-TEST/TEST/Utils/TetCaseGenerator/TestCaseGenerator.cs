using AHC020.Solver;

namespace AHC020_TEST.TEST.Utils.TetCaseGenerator
{
    public class TestCaseGenerator
    {
        public static Input Generate(int seed = 1)
        {
            var input = new Input();

            // input.d = 365;
            //
            // // 一旦seed固定
            // var randomNumGenerator = new Random(seed);
            //
            // var cList = new List<long>();
            // for (int i = 0; i < 26; i++)
            // {
            //     cList.Add(randomNumGenerator.Next(InputStrict.minC, InputStrict.maxC));
            // }
            // input.c = cList.ToArray();
            //
            //
            // var sListList = new List<List<long>>();
            // for (int i = 0; i < input.d; i++)
            // {
            //
            //     var sList = new List<long>();
            //     for (int j = 0; j < 26; j++)
            //     {
            //         sList.Add(randomNumGenerator.Next(InputStrict.minS, InputStrict.maxS));
            //     }
            //     sListList.Add(sList);
            // }
            // input.s = sListList.Select(x => x.ToArray()).ToArray();


            return input;
        }
        
    }
}