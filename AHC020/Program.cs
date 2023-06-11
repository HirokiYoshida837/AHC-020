﻿using System;
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


            var computeScore = Utils.ComputeScore(input, ans);

            Console.WriteLine(computeScore);


            if (IS_DEBUG_ENABLED)
            {
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
            return new MySolver();
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
                        var score = (long) (1000000L * (1.0 + (100000000L / (cost + 10000000L))));

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


                    return 0;
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

                    return new Response
                    {
                        plist = plist,
                        bList = bList
                    };
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