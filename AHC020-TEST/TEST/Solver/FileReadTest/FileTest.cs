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
        public void seed0_MySolver_TEST()
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
        public void seed0_RandomSolver_TEST()
        {
            var reading = File.ReadAllLines($@"Resources\Cases\1\seed1.txt");
            // checkInputFile(reading);

            var input = TestUtils.ReadFileToInput(reading);
            var solvedRes = AHC020.Program.SolveProblem(input, new AHC020.Solver.Implementation.RandomSolver(1250));

            
            solvedRes.AnswerWrite();
            
            var computeScore = AHC020.Solver.Implementation.Utils.ComputeScore(input, solvedRes);

            Console.WriteLine(computeScore);
            
        }
        
        [Test]
        public void seed0_PowerAndSwitchRandomSolver_TEST()
        {
            var reading = File.ReadAllLines($@"Resources\Cases\1\seed1.txt");
            // checkInputFile(reading);

            var input = TestUtils.ReadFileToInput(reading);
            var solvedRes = AHC020.Program.SolveProblem(input, new AHC020.Solver.Implementation.PowerAndSwitchRandomSolver(0, 2500));

            solvedRes.AnswerWrite();
            
            var computeScore = AHC020.Solver.Implementation.Utils.ComputeScore(input, solvedRes);

            Console.WriteLine(computeScore);
            
        }
        
        [Test]
        public void seed1_ClimbingSolver_TEST()
        {
            var reading = File.ReadAllLines($@"Resources\Cases\1\seed1.txt");
            // checkInputFile(reading);

            var input = TestUtils.ReadFileToInput(reading);
            var solvedRes = AHC020.Program.SolveProblem(input, new AHC020.Solver.Implementation.ClimbingSolver(5000));

            solvedRes.AnswerWrite();
            
            var computeScore = AHC020.Solver.Implementation.Utils.ComputeScore(input, solvedRes);

            Console.WriteLine(computeScore);
            
        }
        
        [Test]
        public void seed1_GreedySolver_TEST()
        {
            var reading = File.ReadAllLines($@"Resources\Cases\1\seed0.txt");
            // checkInputFile(reading);

            var input = TestUtils.ReadFileToInput(reading);
            var solvedRes = AHC020.Program.SolveProblem(input, new AHC020.Solver.Implementation.GreedySolver());

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
                var solvedRes = AHC020.Program.SolveProblem(input, new AHC020.Solver.Implementation.GreedySolver());
                
                var computeScore = AHC020.Solver.Implementation.Utils.ComputeScore(input, solvedRes);
                
                scoreList.Add(computeScore);
            }
            
            for (var i = 0; i < scoreList.Count; i++)
            {
                var score = scoreList[i];
                
                Console.WriteLine($"case: {i} \t score: \t\t {score}");
            }
            

            Console.WriteLine($"### last score sum : {scoreList.Sum()} ###");
            Console.WriteLine($"### last score sum : {scoreList.Average()} ###");
            
            
        }
        
    }
}