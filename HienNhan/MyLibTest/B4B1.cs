using Xunit;
using MyLib; 

namespace MyLibTest
{
    public class B4B1Tests
    {
        [Fact]
        public void Test_DoiSangGioPhutGiay()
        {
            // Arrange
            int giay = 4810;
            string mongDoi = "4810 giay co dang 1:20:10";
            
            // Act
            string thucTe = B4B1.DoiSangGioPhutGiay(giay);
            
            // Assert
            Assert.Equal(mongDoi, thucTe);
        }
    }
}