/*
* THƯ VIỆN TÍNH ĐIỂM TRUNG BÌNH VÀ XẾP LOẠI
* Tác giả : Võ Minh Vỹ
* Ngày viết: 15/09/2026
*
* Phát biểu đề bài:
*   Tính DTB = (Toán * 2 + Lý * 3 + Hóa) / 6 và xếp loại học sinh theo các ngưỡng:
*   [8, 10]: "Gioi", [6.5, 8): "Kha", [5, 6.5): "Trung binh", < 5: "Yeu".
*
* Ý tưởng:
*   Viết 2 hàm static độc lập: 1 hàm tính DTB trả về kiểu double, 1 hàm xếp loại trả về string.
*
* Mã giả:
*   Hàm TinhDTB(toan, ly, hoa):
*     Trả về (toan * 2 + ly * 3 + hoa) / 6.0
*
*   Hàm XepLoai(dtb):
*     Nếu dtb >= 8.0 và dtb <= 10.0 -> Trả về "Gioi"
*     Nếu dtb >= 6.5 -> Trả về "Kha"
*     Nếu dtb >= 5.0 -> Trả về "Trung binh"
*     Ngược lại -> Trả về "Yeu"
*/
using System;
using MyLib;

namespace Buoi4_BT1
{
public class DiemTrungBinh
{
    public static void Main(){
    Console.WriteLine("moi nhap diem cua 3 mon:");
    String[] diem=Console.ReadLine()!.Split(' ',StringSplitOptions.RemoveEmptyEntries);
    double toan=double.Parse(diem[0]);
    double ly=double.Parse(diem[1]);
    double hoa=double.Parse(diem[2]);

    double dtb=MyLib.DiemTrungBinh.TinhDTB(toan,ly,hoa);
  String xepLoai=MyLib.DiemTrungBinh.xepLoai(dtb);
    Console.WriteLine($"Ban co diem trung binh {dtb:0.00} duoc xep loai {xepLoai}.");
}
}
}
