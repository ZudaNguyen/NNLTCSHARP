using Xunit;
using MyLib;

namespace MyLib.Tests
{
    public class SapChanLeTests
    {
        [Fact]
        public void Test_SapXepChanLe_MauDeBai()
        {
            int[] input = new int[] { 2, 4, 3, 4, 2, 8, 2, 5 };
            int[] expected = new int[] { 2, 2, 2, 4, 4, 8, 5, 3 };

            int[] actual = SapChanLeLib.SapXepChanLe(input);

            Assert.Equal(expected, actual);
        }
    }
}