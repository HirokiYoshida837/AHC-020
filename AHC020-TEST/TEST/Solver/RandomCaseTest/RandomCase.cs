using AHC020_TEST.TEST.Utils.TetCaseGenerator;
using NUnit.Framework;

namespace AHC020_TEST.TEST.Solver.RandomCaseTest
{
    public class RandomCase
    {
        [Test]
        public void test_defaultSeed()
        {
            var generatedCase = TestCaseGenerator.Generate(0);
            var solved = AHC020.Program.SolveProblem(generatedCase);
            
            
            
        }
    }
}