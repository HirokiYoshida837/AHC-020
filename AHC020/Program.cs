using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AHC020.Solver;
using AHC020.Solver.Implementation;
using Microsoft.VisualBasic;
using static System.Math;

namespace AHC020
{
    public static class Program
    {
        // スコアを自分で計算する場合は true に設定
        public static readonly bool IS_DEBUG_ENABLED = false;

        public static void Main(string[] args)
        {
            if (IS_DEBUG_ENABLED)
            {
                Console.WriteLine("### [WARNING] DEBUG MODE IS ENABLE!! ###");
            }

            // Read Input
            var (n, m, k) = ReadValue<int, int, int>();

            // 基地局のリスト
            var xyList = Enumerable.Range(0, n)
                .Select(_ => ReadValue<int, int>())
                .Select(item => new Pos {x = item.Item1, y = item.Item2})
                .ToArray();

            // 辺がu<->v で貼られる。 wは辺の重み
            var uvwList = Enumerable.Range(0, m)
                .Select(_ => ReadValue<int, int, long>())
                .Select(item => new Edge {u = item.Item1 - 1, v = item.Item2 - 1, w = item.Item3})
                .ToArray();

            // 配信を届けたい住民の座標
            var abList = Enumerable.Range(0, k)
                .Select(_ => ReadValue<int, int>())
                .Select(item => new Pos {x = item.Item1, y = item.Item2})
                .ToArray();

            var input = new Input
            {
                n = n, m = m, k = k,
                xyList = xyList,
                abList = abList,
                uvwList = uvwList,
            };

            // solve problem
            var ans = SolveProblem(input);

            // answer write
            ans.AnswerWrite();

            if (IS_DEBUG_ENABLED)
            {
                var computeScore = Utils.ComputeScore(input, ans);
                Console.WriteLine(computeScore);

                // Console.WriteLine($"[DEBUG] ### Last Score : {solvedResult.lastScore} ### ");
            }
        }

        // 外部からInjectしてテストできるようにする。
        public static Response SolveProblem(Input input)
        {
            return SolveProblem(input, GetDefaultSolver());
        }

        // 外部からInjectしてテストできるようにする。
        public static Response SolveProblem(Input input, ISolver solver)
        {
            return solver.Solve(input);
        }

        // ここを変える。
        public static ISolver GetDefaultSolver()
        {
            // return new EditorialGreedySolver(10);
            // return new RandomSolver();
            // return new MySolver();
            // return new ClimbingSolver();
            return new GreedySolver();
        }


        public static T ReadValue<T>()
        {
            var input = Console.ReadLine();
            return (T) Convert.ChangeType(input, typeof(T));
        }

        public static (T1, T2) ReadValue<T1, T2>()
        {
            var input = Console.ReadLine().Split();
            return (
                (T1) Convert.ChangeType(input[0], typeof(T1)),
                (T2) Convert.ChangeType(input[1], typeof(T2))
            );
        }

        public static (T1, T2, T3) ReadValue<T1, T2, T3>()
        {
            var input = Console.ReadLine().Split();
            return (
                (T1) Convert.ChangeType(input[0], typeof(T1)),
                (T2) Convert.ChangeType(input[1], typeof(T2)),
                (T3) Convert.ChangeType(input[2], typeof(T3))
            );
        }

        /// <summary>
        /// 指定した型として、一行読み込む。
        /// </summary>
        /// <param name="separator"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
#nullable enable
        public static IEnumerable<T> ReadList<T>(params char[]? separator)
        {
            return Console.ReadLine()
                .Split(separator)
                .Select(x => (T) Convert.ChangeType(x, typeof(T)));
        }
#nullable disable
    }

    namespace Solver
    {
        public struct Pos
        {
            public int x;
            public int y;
        }

        public struct Edge
        {
            public int u;
            public int v;
            public long w;
        }

        public struct Input
        {
            // 基地局の数
            public int n;

            // 辺の数
            public int m;

            // 住人の数
            public int k;

            // 基地局の座標リスト
            public Pos[] xyList;

            // 辺の情報
            public Edge[] uvwList;

            // 住人の座標リスト
            public Pos[] abList;
        }

        /// <summary>
        /// Solverとやり取りするための型
        /// </summary>
        public struct Response
        {
            // 各頂点の出力強度
            public int[] plist;

            // 各辺のスイッチをONにするか/OFFにするかを 0/1 で表す。
            public int[] bList;

            public void AnswerWrite()
            {
                Console.WriteLine(Strings.Join(plist.Select(x => x.ToString()).ToArray(), " "));
                Console.WriteLine(Strings.Join(bList.Select(x => x.ToString()).ToArray(), " "));
            }

            public void DebugAnswerWrite()
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public interface ISolver
        {
            Response Solve(Input input);
        }

        namespace Implementation
        {
            public static class Utils
            {
                public static long ComputeScore(Input input, Response response)
                {
                    var broadcastedCount = GetBroadcastedCount(input, response);

                    if (broadcastedCount < input.k)
                    {
                        var score = (1000000L * (broadcastedCount + 1L) / (long) (input.k));
                        return score;
                    }
                    else
                    {
                        var cost = CalculateCost(input, response);
                        var score = (long) (1000000L * (1.0d + (10L / (cost / 10000000d + 1L))));

                        return score;
                    }
                }

                public static long GetBroadcastedCount(Input input, Response response)
                {
                    // root基地局と繋がってるかのリスト
                    var isConnectedList = GetConnectionStatus(input, response);

                    var broadcastedStatus = GetBroadcastedStatus(input, response, isConnectedList);

                    var count = broadcastedStatus.Count(x => x == true);

                    return count;
                }

                public static bool[] GetConnectionStatus(Input input, Response response)
                {
                    var dsu = new UnionFind(input.n + 1);

                    for (var j = 0; j < response.bList.Length; j++)
                    {
                        var used = response.bList[j] == 1;
                        if (!used)
                        {
                            continue;
                        }

                        var edge = input.uvwList[j];

                        dsu.TryUnite(edge.u, edge.v);
                    }

                    var ret = new bool[input.n];
                    for (int i = 0; i < input.n; i++)
                    {
                        var root = dsu.FindRoot(0);
                        var findRoot = dsu.FindRoot(i);

                        if (root == findRoot)
                        {
                            ret[i] = true;
                        }
                    }

                    return ret;
                }

                public static bool[] GetBroadcastedStatus(Input input, Response response, bool[] isConnected)
                {
                    var ret = new bool[input.k];

                    for (int i = 0; i < input.n; i++)
                    {
                        if (!isConnected[i])
                        {
                            continue;
                        }

                        for (int k = 0; k < input.k; k++)
                        {
                            // Console.WriteLine($"{i}, {k}");
                            var distSq = CalcSqDist(input.xyList[i], input.abList[k]);
                            var power = response.plist[i];

                            ret[k] |= distSq <= power * power;
                        }
                    }

                    return ret;
                }

                public static long CalculateCost(Input input, Response response)
                {
                    var cost = 0L;

                    for (int i = 0; i < input.n; i++)
                    {
                        cost += CalculatePowerCost(response, i);
                    }


                    for (var j = 0; j < response.bList.Length; j++)
                    {
                        var used = response.bList[j] == 1;

                        if (!used)
                        {
                            continue;
                        }

                        var edge = input.uvwList[j];
                        cost += edge.w;
                    }


                    return cost;
                }

                public static long CalculatePowerCost(Response response, int index)
                {
                    var p = (long) response.plist[index];
                    return p * p;
                }


                // 距離の二乗を返すよ
                public static int CalcSqDist(Pos posA, Pos posB)
                {
                    var dx = posA.x - posB.x;
                    var dy = posA.y - posB.y;

                    return dx * dx + dy * dy;
                }

                // 全部の基地局について、繋がってる
            }


            /// <summary>
            /// 出力固定で適当に返す。
            /// </summary>
            public class MySolver : ISolver
            {
                public int power;

                public MySolver(int power = 5000)
                {
                    this.power = power;
                }

                public Response Solve(Input input)
                {
                    // 出力固定
                    var plist = Enumerable.Range(0, input.n).Select(_ => power).ToArray();
                    var bList = Enumerable.Range(0, input.m).Select(_ => 1).ToArray();

                    // 重複辺がある場合、片方はOFFにしていいはず。
                    var dictionary = input.uvwList
                        .Select((item, i) => (item, i))
                        .GroupBy(x => (Math.Max(x.item.u, x.item.v), Math.Min(x.item.u, x.item.v)))
                        .ToDictionary(x => x.Key, x => x.Select(y => y.i).OrderBy(x => x).ToArray());

                    foreach (var kv in dictionary.Where(x => x.Value.Count() > 1))
                    {
                        foreach (var i in kv.Value.Skip(1))
                        {
                            bList[i] = 0;
                        }
                    }

                    return new Response
                    {
                        plist = plist,
                        bList = bList
                    };
                }
            }

            public class RandomSolver : ISolver
            {
                public int power;

                public RandomSolver(int power = 5000)
                {
                    this.power = power;
                }

                public Response Solve(Input input)
                {
                    // power固定
                    var plist = Enumerable.Range(0, input.n).Select(_ => power).ToArray();

                    // スイッチON/OFFを適当にする
                    var rng = new Random(0);
                    var bList = new List<int>();
                    for (int i = 0; i < input.m; i++)
                    {
                        if (rng.NextDouble() > 0.5)
                        {
                            bList.Add(0);
                        }
                        else
                        {
                            bList.Add(1);
                        }
                    }

                    // 重複辺がある場合、片方はOFFにしていいはず。
                    var dictionary = input.uvwList
                        .Select((item, i) => (item, i))
                        .GroupBy(x => (Math.Max(x.item.u, x.item.v), Math.Min(x.item.u, x.item.v)))
                        .ToDictionary(x => x.Key, x => x.Select(y => y.i).OrderBy(x => x).ToArray());

                    foreach (var kv in dictionary.Where(x => x.Value.Count() > 1))
                    {
                        foreach (var i in kv.Value.Skip(1))
                        {
                            bList[i] = 0;
                        }
                    }


                    return new Response
                    {
                        plist = plist,
                        bList = bList.ToArray()
                    };
                }
            }

            public class PowerAndSwitchRandomSolver : ISolver
            {
                public int powerMax;
                public int powerMin;

                public PowerAndSwitchRandomSolver(int powerMin = 2500, int powerMax = 5000)
                {
                    this.powerMax = powerMax;
                    this.powerMin = powerMin;
                }

                public Response Solve(Input input)
                {
                    var rng = new Random(0);

                    // powerもランダム
                    var plist = Enumerable.Range(0, input.n).Select(_ => rng.Next(powerMin, powerMax)).ToArray();

                    // スイッチON/OFFを適当にする
                    var bList = new List<int>();
                    for (int i = 0; i < input.m; i++)
                    {
                        if (rng.NextDouble() > 0.5)
                        {
                            bList.Add(0);
                        }
                        else
                        {
                            bList.Add(1);
                        }
                    }

                    // 重複辺がある場合、片方はOFFにしていいはず。
                    var dictionary = input.uvwList
                        .Select((item, i) => (item, i))
                        .GroupBy(x => (Math.Max(x.item.u, x.item.v), Math.Min(x.item.u, x.item.v)))
                        .ToDictionary(x => x.Key, x => x.Select(y => y.i).OrderBy(x => x).ToArray());

                    foreach (var kv in dictionary.Where(x => x.Value.Count() > 1))
                    {
                        foreach (var i in kv.Value.Skip(1))
                        {
                            bList[i] = 0;
                        }
                    }


                    return new Response
                    {
                        plist = plist,
                        bList = bList.ToArray()
                    };
                }
            }

            public class GreedySolver : ISolver
            {
                // Greedyじゃないよねこれ
                public Response Solve(Input input)
                {
                    // タイマー 1.8 秒
                    var start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var timeLimit = 1800L;


                    // root から幅優先探索で順に繋いでいく

                    // 全部0で初期化
                    var response = new MySolver(5000).Solve(input);
                    response.bList = new int[input.m];

                    var maxScore = Utils.ComputeScore(input, response);


                    var uf = new UnionFind(input.n + 1);

                    // 各放送局の無向グラフをつくる
                    // 重複辺がある場合、片方はOFFにしていいはず。
                    var dictionary = input.uvwList
                        .Select((item, i) => (item, i))
                        .GroupBy(x => (Math.Max(x.item.u, x.item.v), Math.Min(x.item.u, x.item.v)))
                        // .ToDictionary(x => x.Key, x => x.Select(x => x.i).First());
                        .ToDictionary(x => x.Key, x => x.OrderBy(x => x.item.w).ToArray());

                    // var graph = new bool[input.n][];

                    // 繋がってる辺の番号でメモ。
                    var graph = Enumerable.Range(0, input.n).Select(_ => new int[input.n].Select(_ => -1).ToArray())
                        .ToArray();
                    foreach (var kv in dictionary)
                    {
                        graph[kv.Key.Item1][kv.Key.Item2] = kv.Value.First().i;
                        graph[kv.Key.Item2][kv.Key.Item1] = kv.Value.First().i;
                    }

                    // BFSする
                    var bList = new int[input.m];

                    var depthList = new int[input.n].Select(_ => -1).ToArray();
                    depthList[0] = 0;

                    response.plist[0] = 5000;


                    var queue = new Queue<int>();
                    for (var i = 0; i < graph[0].Length; i++)
                    {
                        if (graph[0][i] != -1)
                        {
                            uf.TryUnite(0, i);
                            bList[graph[0][i]] = 1;
                            queue.Enqueue(i);

                            response.plist[i] = 2500;

                            depthList[i] = 1;
                        }
                    }

                    var powerList = new[] {2500, 3000, 4000, 5000}.OrderByDescending(x => x).ToArray();


                    var rng = new Random(0);

                    while (queue.Count != 0)
                    {
                        var dequeue = queue.Dequeue();

                        for (int i = 0; i < input.n; i++)
                        {
                            if (i == dequeue)
                            {
                                continue;
                            }

                            if (graph[dequeue][i] != -1)
                            {
                                // まだrootと繋がってないんだったら探索する
                                var findRoot = uf.FindRoot(i);
                                var root = uf.FindRoot(0);

                                if (findRoot != root)
                                {
                                    // blistを1にしてみてスコアが上がるのであれば、くっつける意味ある。そうじゃなかったらやらなくていいかも。
                                    bList[graph[dequeue][i]] = 1;

                                    response.bList = bList;

                                    // foreach (var pow in powerList)
                                    // {
                                    //     var currentPow = response.plist[i];
                                    //     response.plist[i] = pow;

                                    var newScore = Utils.ComputeScore(input, response);

                                    if (newScore > maxScore)
                                    {
                                        // スコア上がってるんだったら更新
                                        maxScore = newScore;

                                        uf.TryUnite(dequeue, i);
                                        queue.Enqueue(i);
                                    }
                                    else
                                    {
                                        // スコア上がらなくても、確率で使う。

                                        if (rng.NextDouble() > 0.8)
                                        {
                                            maxScore = newScore;

                                            uf.TryUnite(dequeue, i);
                                            queue.Enqueue(i);
                                        }
                                        else
                                        {
                                            // スコア上がらないんだったら、戻して打ち切り。
                                            bList[graph[dequeue][i]] = 0;
                                            response.bList = bList;

                                            // response.plist[i] = currentPow;
                                        }
                                    }
                                    
                                    response.AnswerWrite();
                                    
                                    // }
                                }
                            }
                        }
                    }

                    // startから1.8秒までの間、探索し続ける。
                    while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < start + 1800)
                    {
                        var nextDouble = rng.NextDouble();

                        // if (nextDouble < 0.5)
                        // {
                        var next = rng.Next(0, input.n);
                        if (response.plist[next] > 0)
                        {
                            var last = response.plist[next];
                            
                            // 一旦 powerを10にして見てみる。
                            response.plist[next] = 0;
                            var computeScore1 = Utils.ComputeScore(input, response);
                            if (computeScore1 > maxScore)
                            {
                                response.AnswerWrite();
                                continue;
                            }

                            
                            // powerを0.7倍にする。
                            response.plist[next] = (last / 10) * 7;

                            var computeScore2 = Utils.ComputeScore(input, response);

                            if (computeScore2 > maxScore)
                            {
                                response.AnswerWrite();
                                continue;
                            }
                            else
                            {
                                response.plist[next] = last;
                            }
                        }
                        // }
                        // else if (nextDouble < 0.6)
                        // {
                        //     // 辺を入れ替える
                        //     var pivot = rng.Next(1,input.n);
                        //
                        //     if (response.plist[pivot] == 0)
                        //     {
                        //         // どこにも繋がってない基地局なのでスキップ
                        //         continue;
                        //     }
                        //
                        //     // 他に使える辺のリストはある？
                        //     if (graph[pivot].Count(x=>x >=0 ) > 0)
                        //     {
                        //         for (var i = 0; i < graph[pivot].Length; i++)
                        //         {
                        //             
                        //         }
                        //     }
                        // }
                        // else
                        // {
                        //     // do nothing
                        // }
                    }

                    var zeroRoot = uf.FindRoot(0);
                    for (int i = 1; i < input.n; i++)
                    {
                        // 繋がってないノードの出力を全部0にする
                        var findRoot = uf.FindRoot(i);

                        if (findRoot != zeroRoot)
                        {
                            response.plist[i] = 0;
                        }
                    }


                    return response;
                }
            }


            public class ClimbingSolver : ISolver
            {
                public int power;

                // power 5000固定で一旦Greedy。
                public ClimbingSolver(int power = 5000)
                {
                    this.power = power;
                }

                public Response Solve(Input input)
                {
                    // タイマー 1.8 秒
                    var start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var timeLimit = 1800L;


                    // var powList = new[]
                    // {
                    //     (0, 1250),
                    //     (0, 2500),
                    //     (1250, 3750),
                    //     (0, 5000),
                    //     (1250, 5000)
                    // };
                    //
                    var maxScore = long.MinValue;
                    var response = new Response();
                    //
                    // // 一旦、ランダムに初期を決める
                    // foreach (var (min, max) in powList)
                    // {
                    //     var current = new PowerAndSwitchRandomSolver(0, 1250).Solve(input);
                    //
                    //     var computeScore = Utils.ComputeScore(input, current);
                    //
                    //     if (computeScore >= maxScore)
                    //     {
                    //         response = current;
                    //     }
                    // }

                    var powList = new[] {0, 1250, 2500, 3750, 5000};

                    foreach (var p in powList)
                    {
                        var current = new MySolver(p).Solve(input);
                        var computeScore = Utils.ComputeScore(input, current);

                        if (computeScore >= maxScore)
                        {
                            response = current;
                        }
                    }


                    // startから1.8秒までの間、探索し続ける。
                    while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < start + 1800)
                    {
                        // どこかの辺をOFFにしてスコアが下がるかどうかを確認

                        var rng = new Random(0);

                        // var nextDouble = rng.NextDouble();
                        // if (nextDouble > 0.5)
                        // {
                        var d1 = rng.Next(0, response.bList.Length);

                        var currentState = response.bList[d1];

                        var newState = currentState == 1 ? 0 : 1;
                        response.bList[d1] = newState;

                        var newScore = Utils.ComputeScore(input, response);

                        if (newScore >= maxScore)
                        {
                            maxScore = newScore;
                        }
                        else
                        {
                            // 戻す
                            response.bList[d1] = currentState;
                        }

                        // }
                        // else
                        // {
                        //     // do nothing
                        //     
                        // }
                    }
                    
                    response.AnswerWrite();

                    return response;
                }
            }
        }
    }

    public class UnionFind
    {
        private int Size { get; set; }
        private int GroupCount { get; set; }

        private int[] Parent;
        private int[] Sizes;

        /// <summary>
        /// n要素のUnionFind-Treeを生成する
        /// </summary>
        /// <param name="count"></param>
        public UnionFind(int count)
        {
            Size = count;
            GroupCount = count;

            Parent = new int[count];
            Sizes = new int[count];

            for (int i = 0; i < count; i++)
            {
                Parent[i] = i;
                Sizes[i] = 1;
            }
        }

        public bool TryUnite(int x, int y)
        {
            return TryUnite(x, y, out var p);
        }

        /// <summary>
        /// xとyの属している木を合併する。<br/>
        /// xとyがすでに同じ木にある場合、falseを返す。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="parent"> xとyの合併後のroot </param>
        /// <returns></returns>
        public bool TryUnite(int x, int y, out int parent)
        {
            //       ans += map.TryGetValue(cum[r] - k, out var val) ? val : 0;

            var xp = FindRoot(x);
            var yp = FindRoot(y);
            // rootが同じであれば、すでにUnite済なのでエラー。
            if (xp == yp)
            {
                parent = xp;
                return false;
            }

            if (Sizes[xp] < Sizes[yp])
            {
                (yp, xp) = (xp, yp);
            }

            GroupCount--;

            Parent[yp] = xp;
            Sizes[xp] += Sizes[yp];

            parent = xp;
            return true;
        }

        /// <summary>
        /// xのrootを返す
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindRoot(int x)
        {
            // if (x == Parent[x]) return x;
            // return FindRoot(Parent[x]);

            // 再帰しないので速そう
            while (x != Parent[x])
            {
                x = (Parent[x] = Parent[Parent[x]]);
            }

            return x;
        }

        /// <summary>
        /// rootになっているノードをすべて返す
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<int> AllRepresents()
        {
            return Parent.Where((x, y) => x == y);
        }

        /// <summary>
        /// xが属している木のサイズを返す
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(int x)
        {
            return Sizes[FindRoot(x)];
        }
    }
}