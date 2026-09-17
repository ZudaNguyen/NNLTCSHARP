using System.Collections.Generic;
using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class LietKeChanTests
    {
        [Fact]
        public void Test_LietKeSoChanTuPhaiSangTrai_DeBaiSample()
        {
            int[] input = new int[] { 2, 3, 5, 6 };
            List<int> expected = new List<int> { 6, 2 };

            List<int> actual = LietKeChanLib.LietKeSoChanTuPhaiSangTrai(input);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Test_LietKeSoChanTuPhaiSangTrai_KhongCoSoChan()
        {
            int[] input = new int[] { 1, 3, 5, 7 };
            List<int> actual = LietKeChanLib.LietKeSoChanTuPhaiSangTrai(input);

            Assert.Empty(actual);
        }
    }
}