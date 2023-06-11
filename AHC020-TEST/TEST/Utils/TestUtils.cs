using System.Collections.Generic;
using System.Linq;
using AHC020.Solver;

namespace AHC020_TEST.TEST.Utils
{
    public class TestUtils
    {
        public static Input ReadFileToInput(string[] reading)
        {
            var nmk = reading[0].Split().Select(int.Parse).ToArray();


            var xyList = new List<(int, int)>();
            for (int i = 0; i < nmk[0]; i++)
            {
                var xy = reading[i + 1];
                var array = xy.Split().Select(int.Parse).ToArray();
                xyList.Add((array[0], array[1]));
            }

            var uvwList = new List<(int, int, long)>();
            for (int i = 0; i < nmk[1]; i++)
            {
                var uvw = reading[i + 1 + nmk[0]];
                var array = uvw.Split().Select(int.Parse).ToArray();
                uvwList.Add((array[0], array[1], array[2]));
            }

            var abList = new List<(int, int)>();
            for (int i = 0; i < nmk[2]; i++)
            {
                var ab = reading[i + 1 + nmk[0] + nmk[1]];
                var array = ab.Split().Select(int.Parse).ToArray();
                abList.Add((array[0], array[1]));
            }


            var input = new Input
            {
                n = nmk[0], m = nmk[1], k = nmk[2],
                xyList = xyList.Select(x => new Pos {x = x.Item1, y = x.Item2}).ToArray(),
                uvwList = uvwList.Select(item => new Edge {u = item.Item1 -1, v = item.Item2 - 1, w = item.Item3}).ToArray(),
                abList = abList.Select(x => new Pos {x = x.Item1, y = x.Item2}).ToArray()
            };

            return input;
        }
    }
}