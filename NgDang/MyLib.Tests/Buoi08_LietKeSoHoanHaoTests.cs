using Xunit;
using MyLib;
using System.Collections.Generic;

namespace MyLib.Tests
{
    [Trait("Buoi", "08")]
    public class LietKeSoHoanHaoTests
    {
        [Theory]
        [InlineData(10, new int[] { 6 })]             // Test case đề bài
        [InlineData(30, new int[] { 28, 6 })]         // Kiểm tra thứ tự giảm dần
        [InlineData(5, new int[] { })]                // Trường hợp không có số nào
        [InlineData(10000, new int[] { 8128, 496, 28, 6 })] // Dải số lớn hơn
        public void KiemTra_TimSoHoanHaoGiamDan(int n, int[] expectedArray)
        {
            LietKeSoHoanHaoLib lib = new LietKeSoHoanHaoLib();
            
            // Chuyển mảng expectedArray thành List để dùng cho hàm Assert
            List<int> expectedList = new List<int>(expectedArray);
            
            Assert.Equal(expectedList, lib.TimSoHoanHaoGiamDan(n));
        }
    }
}