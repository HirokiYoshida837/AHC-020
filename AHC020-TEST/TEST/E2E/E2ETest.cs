using System;
using System.IO;
using NUnit.Framework;

namespace AHC020_TEST.TEST.E2E
{
    public class E2ETest : TestBase
    {
        [Test]
        public void 実行が正常に動くかどうかのテスト()
        {
            // テストがエラー出ずに動くかどうかのテストだけ
            
            // Resources\Cases\Tests\E2E\case1
            // \Resources\Tests\E2E\case1\input.txt'.
            var input = File.ReadAllText($@"Resources\Cases\Tests\E2E\case1\input.txt");
            Test(input, null, () => { AHC020.Program.Main(new String[] { }); });
        }
    }
}