using Xunit;
using MyLib;

namespace MyLibTest
{
    public class B4B9Tests
    {
        [Fact]
        public void Test_TimKiemX_TonTaiNhieuLan()
        {
            // Arrange
            double[] arr = { -2.9, 5, -4, -2.9, -5 };
            double x = -2.9;
            string mongDoi = "Gia tri -2.9 xuat hien 2 lan o vi tri dau tien 0 va cuoi cung 3.";
            

            // Act
            string thucTe = MyLib.B4B9.TimKiemX(arr, x);

            // Assert
            Assert.Equal(mongDoi, thucTe);
        }
[Fact]
        public void Test_TimKiemX_TatCaPhanTuDeuLaX()
        {
            // Trường hợp toàn bộ mảng đều là giá trị cần tìm
            double[] arr = { 5.5, 5.5, 5.5, 5.5 };
            double x = 5.5;
            string mongDoi = "Gia tri 5.5 xuat hien 4 lan o vi tri dau tien 0 va cuoi cung 3.";

            Assert.Equal(mongDoi, MyLib.B4B9.TimKiemX(arr, x));
        }

        [Fact]
        public void Test_TimKiemX_ChiCoMotPhanTu_DungX()
        {
            // Trường hợp mảng chỉ có đúng 1 phần tử và nó chính là X
            double[] arr = { -2.9 };
            double x = -2.9;
            string mongDoi = "Gia tri -2.9 xuat hien 1 lan o vi tri dau tien 0 va cuoi cung 0.";

            Assert.Equal(mongDoi, B4B9.TimKiemX(arr, x));
        }

        [Fact]
        public void Test_TimKiemX_MangRong()
        {
            // Trường hợp mảng được khởi tạo nhưng không có phần tử nào (length = 0)
            double[] arr = new double[0]; 
            double x = 1.0;
            string mongDoi = "Day so rong.";

            Assert.Equal(mongDoi, MyLib.B4B9.TimKiemX(arr, x));
        }

        [Fact]
        public void Test_TimKiemX_MangNull()
        {
            // Trường hợp mảng chưa hề được khởi tạo (null) -> Rất dễ gây lỗi Exception nếu không rào trước
            double[] arr = null; 
            double x = 1.0;
            string mongDoi = "Day so rong.";

            Assert.Equal(mongDoi, MyLib.B4B9.TimKiemX(arr, x));
        }
        [Fact]
        public void Test_TimKiemX_KhongTonTai()
        {
            // Arrange
            double[] arr = { 1.5, 2.0, 3.2 };
            double x = 10.0;
            string mongDoi = "Gia tri 10 khong ton tai trong day so.";

            // Act & Assert
            Assert.Equal(mongDoi, MyLib.B4B9.TimKiemX(arr, x));
        }
    }
}