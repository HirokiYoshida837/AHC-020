using AHC020_TEST.TEST.Utils.TetCaseGenerator;
using NUnit.Framework;

namespace AHC020_TEST.TEST.Solver.MySolver
{
    public class MySolverTest
    {
        [Test]
        public void test_defaultSeed()
        {
            var generatedInput = TestCaseGenerator.Generate();
            var solved = AHC020.Program.SolveProblem(generatedInput, new AHC020.Solver.Implementation.MySolver());

            solved.AnswerWrite();
            solved.DebugAnswerWrite();
        }
        
    }
}