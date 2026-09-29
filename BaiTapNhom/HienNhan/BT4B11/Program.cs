/*
* CHƯƠNG TRÌNH ĐẢO NGƯỢC DÃY SỐ
* Tác giả : Nguyễn Văn Hiền Nhân
* Ngày viết: 16/09/2026
*
* Phát biểu đề bài: Cho số nguyên n (n <= 500), dãy n số nguyên. Hãy đảo ngược dãy số này.
* Ý tưởng: 
*  - Tạo một mảng mới cùng kích thước n.
*  - Dùng vòng lặp duyệt i từ 0 đến n-1, gán mảng mới tại vị trí i bằng mảng cũ tại vị trí (n - 1 - i).
* Mã giả:
*  Begin
*      Input n, arr[n]
*      Create result[n]
*      For i = 0 To n - 1 Do
*          result[i] = arr[n - 1 - i]
*      End For
*      Print arr
*      Print result
*  End
*/

using System;

class BT4B11
{
    static void Main()
    {
        Console.WriteLine("// Nhap day so (cac so cach nhau boi khoang trang):");
        string input = Console.ReadLine();
        
        string[] parts = input?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts != null && parts.Length > 0 && parts.Length <= 500)
        {
            int n = parts.Length;
            int[] arr = new int[n];
            bool nhapDung = true;

            for (int i = 0; i < n; i++)
            {
                if (!int.TryParse(parts[i], out arr[i]))
                {
                    Console.WriteLine($"Phan tu '{parts[i]}' khong phai la so nguyen.");
                    nhapDung = false;
                    break;
                }
            }

            if (nhapDung)
            {
                Console.WriteLine("* Mang truoc đảo:");
                Console.WriteLine($"Day so co {n} phan tu: " + string.Join(" ", arr));
                
                // Gọi hàm đảo ngược từ thư viện
                int[] mangDaDao = MyLib.B4B11.DaoNguocDaySo(arr);
                
                Console.WriteLine("* Mang sau khi đảo:");
                Console.WriteLine($"Day so co {n} phan tu: " + string.Join(" ", mangDaDao));
            }
        }
        else
        {
            Console.WriteLine("Day so khong hop le hoac vuot qua 500 phan tu.");
        }
    }
}