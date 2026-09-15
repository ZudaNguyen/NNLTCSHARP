using Xunit;
using MyLib;
        
namespace MyLib.Tests
{
    public class Buoi2_TongdoanTests
{
    [Theory]
    [InlineData(3, 5, 12)]
    [InlineData(1, 10, 55)]
    public void Test_TongDoan(long a,long b,long expected)
        {
            long actual=Buoi2_Tongdoan.Tongdoan(a,b);
            Assert.Equal(expected, actual);
        }

}

    
}


