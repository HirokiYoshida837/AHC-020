using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AHC020_TEST.TEST.Utils;
using NUnit.Framework;

namespace AHC020_TEST.TEST.Solver.FileReadTest
{
    public class FileTest
    {
        [Test]
        public void seed0_TEST()
        {
            var reading = File.ReadAllLines($@"Resources\Cases\1\seed0.txt");
            // checkInputFile(reading);

            var input = TestUtils.ReadFileToInput(reading);
            var solvedRes = AHC020.Program.SolveProblem(input, new AHC020.Solver.Implementation.MySolver(5000));

            solvedRes.AnswerWrite();
            
            var computeScore = AHC020.Solver.Implementation.Utils.ComputeScore(input, solvedRes);

            Console.WriteLine(computeScore);
            
        }

        [Test]
        public void ケース10個のテスト()
        {

            var scoreList = new List<long>();
            
            for (int i = 0; i < 10; i++)
            {
                var reading = File.ReadAllLines($@"Resources\Cases\1\seed{i}.txt");
                
                var input = TestUtils.ReadFileToInput(reading);
                var solvedRes = AHC020.Program.SolveProblem(input, new AHC020.Solver.Implementation.MySolver(5000));


                
                

                // solveProblem.AnswerWrite();
                
                // scoreList.Add(computeScore);
            }

            Console.WriteLine($"### last score sum : {scoreList.Sum()} ###");
            
            
        }
        
    }
}