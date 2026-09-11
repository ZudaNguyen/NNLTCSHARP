/*
* CHƯƠNG TRÌNH RÚT GỌN PHÂN SỐ
* Tác giả : Nguyễn Huỳnh Hoàng Vũ
* Ngày viết: 10/09/2026
*
* Phát biểu đề bài: Nhập vào tử số và mẫu số của một phân số. Hãy rút gọn phân số đã nhập.
* Ý tưởng: 
*   - Lưu lại tử số và mẫu số ban đầu để in ra màn hình.
*   - Dùng thuật toán Euclid để tìm Ước chung lớn nhất (UCLN) của giá trị tuyệt đối tử số và mẫu số.
*   - Chia cả tử số và mẫu số cho UCLN để được phân số rút gọn.
*   - Đưa dấu trừ (nếu có) từ mẫu số lên tử số để phân số hiển thị đẹp mắt hơn (ví dụ: thay vì 2/-3 thì đổi thành -2/3).
*
* Mã giả:
*   Nhập tuSo, mauSo
*   Nếu (mauSo == 0) thì:
*       In ra thông báo lỗi và kết thúc chương trình
*   Lưu tuSoBanDau = tuSo, mauSoBanDau = mauSo
*   Nếu (mauSo < 0) thì:
*       tuSo = -tuSo
*       mauSo = -mauSo
*   a = Giá trị tuyệt đối của tuSo
*   b = Giá trị tuyệt đối của mauSo
*   Trong khi (b != 0) thực hiện:
*       temp = b
*       b = a % b
*       a = temp
*   ucln = a
*   tuSoRutGon = tuSo / ucln
*   mauSoRutGon = mauSo / ucln
*   Nếu (mauSoRutGon == 1 hoặc tuSoRutGon == 0) thì:
*       In kết quả chỉ gồm tuSoRutGon
*   Ngược lại:
*       In kết quả dạng tuSoRutGon/mauSoRutGon
*/

using System;

namespace Buoi8_BT3
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Moi ban nhap tu so: ");
            int tuSo = int.Parse(Console.ReadLine()!);

            Console.Write("Moi ban nhap mau so: ");
            int mauSo = int.Parse(Console.ReadLine()!);

            // Kiểm tra điều kiện mẫu số phải khác 0
            if (mauSo == 0)
            {
                Console.WriteLine("Loi: Mau so phai khac 0.");
                return;
            }

            // Lưu lại giá trị ban đầu để in kết quả lúc sau
            int tuSoBanDau = tuSo;
            int mauSoBanDau = mauSo;

            // Đưa dấu trừ lên tử số nếu mẫu số âm
            if (mauSo < 0)
            {
                tuSo = -tuSo;
                mauSo = -mauSo;
            }

            // Tìm UCLN bằng thuật toán Euclid
            int a = Math.Abs(tuSo);
            int b = Math.Abs(mauSo);
            
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            
            int ucln = a;

            // Thực hiện rút gọn
            int tuSoRutGon = tuSo / ucln;
            int mauSoRutGon = mauSo / ucln;

            // In kết quả theo định dạng đề bài
            if (mauSoRutGon == 1 || tuSoRutGon == 0)
            {
                // Nếu mẫu số sau khi rút gọn là 1 hoặc tử số bằng 0, chỉ in ra số nguyên
                Console.WriteLine($"Phan so {tuSoBanDau}/{mauSoBanDau} duoc rut gon thanh {tuSoRutGon}.");
            }
            else
            {
                Console.WriteLine($"Phan so {tuSoBanDau}/{mauSoBanDau} duoc rut gon thanh {tuSoRutGon}/{mauSoRutGon}.");
            }
        }
    }
}
