/*
* CHƯƠNG TRÌNH TÌM KIẾM SỐ THỰC TRONG MẢNG
* Tác giả : Nguyễn Văn Hiền Nhân
* Ngày viết: 16/09/2026
*
* Phát biểu đề bài: Cho dãy số thực có n phần tử (n <= 500) và số thực x. Tìm vị trí đầu tiên, cuối cùng và số lần xuất hiện.
* Ý tưởng: 
*  - Duyệt mảng từ đầu đến cuối. Dùng epsilon để so sánh 2 số thực.
*  - Nếu bằng x và viTriDau == -1 thì gán viTriDau = i.
*  - Luôn cập nhật viTriCuoi = i và tăng soLan mỗi khi tìm thấy.
* Mã giả:
*  Begin
*      Input n, array[n], x
*      viTriDau = -1, viTriCuoi = -1, count = 0
*      For i = 0 To n-1 Do
*          If array[i] == x Then
*              If viTriDau == -1 Then viTriDau = i
*              viTriCuoi = i
*              count = count + 1
*          End If
*      End For
*      Print result
*  End
*/

using System;

class BT4B9
{
    static void Main()
    {
        Console.Write("Moi ban nhap so luong phan tu: ");
        if (int.TryParse(Console.ReadLine(), out int n) && n > 0 && n <= 500)
        {
            double[] arr = new double[n];
            for (int i = 0; i < n; i++)
            {
                Console.Write($"Phan tu {i}: ");
                while (!double.TryParse(Console.ReadLine(), out arr[i]))
                {
                    Console.Write("Vui long nhap so thuc. Nhap lai phan tu {i}: ");
                }
            }

            Console.WriteLine($"Day so co {n} phan tu: " + string.Join(" ", arr));
            Console.WriteLine("* Tim kiem");
            
            Console.Write("Moi ban nhap gia tri x can tim: ");
            if (double.TryParse(Console.ReadLine(), out double x))
            {
                string ketQua = MyLib.B4B9.TimKiemX(arr, x);
                Console.WriteLine(ketQua);
            }
            else
            {
                Console.WriteLine("Vui long nhap vao mot so thuc x hop le.");
            }
        }
        else
        {
            Console.WriteLine("So luong phan tu phai lon hon 0 va nho hon hoac bang 500.");
        }
    }
}