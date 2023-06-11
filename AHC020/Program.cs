using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AHC020.Solver;
using AHC020.Solver.Implementation;
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
            // var d = ReadValue<int>();
            // var cList = ReadList<long>().ToArray();
            // var sList = Enumerable.Range(0, d)
            //     .Select(_ => ReadList<long>().ToArray())
            //     .ToArray();
            //
            // var input = new Input {d = d, c = cList, s = sList};
            //
            // var solvedResult = SolveProblem(input);

            // 出力
            // solvedResult.AnswerWrite();

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
        public struct Input
        {
            // // 開催される日数
            // public int d { get; set; }
            //
            // // コンテストが開かれなかったときのscoreの下がりやすさ
            // public long[] c { get; set; }
            //
            // // i日目にコンテストが開かれたときに得られるscore
            // public long[][] s { get; set; }
        }

        /// <summary>
        /// Solverとやり取りするための型
        /// </summary>
        public struct Response
        {
            // public int[] answerList;
            // public long lastScore;
            //
            public void AnswerWrite()
            {
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
            }


            public class MySolver : ISolver
            {
                public Response Solve(Input input)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}