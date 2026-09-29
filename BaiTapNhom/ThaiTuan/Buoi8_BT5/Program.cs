/*
* CHUONG TRINH DOC SO NGUYEN DUONG SANG TIENG VIET (TOI DA 12 CHU SO)
* Tac gia : ThaiTuan
* Ngay viet: 19/09/2026
*
* Phat bieu de bai: Nhap vao mot so nguyen duong n co toi da 12 chu so.
* Hay doc so nguyen n sang dang tieng Viet khong dau.
*
* Y tuong:
*  - Tach n thanh 4 nhom, moi nhom 3 chu so: ti, trieu, ngan, don vi.
*    nhomTi   = n / 1_000_000_000
*    nhomTrieu= (n / 1_000_000) % 1000
*    nhomNgan = (n / 1000) % 1000
*    nhomDonVi= n % 1000
*  - Bo qua cac nhom bang 0 hoan toan (khong doc).
*  - Doc tung nhom con lai theo quy tac doc so co 3 chu so:
*      + Neu hang tram khac 0: doc "X tram", neu hang chuc = 0 va don vi khac 0
*        thi them "le Y"; nguoc lai doc binh thuong phan chuc-don vi.
*      + Neu hang tram bang 0 va day KHONG phai nhom dau tien (nhom co nghia
*        lon nhat) thi them "le" truoc phan chuc-don vi.
*  - Ghep cac nhom lai, moi nhom trong dau ngoac vuong, kem hau to (ti/trieu/ngan).
*
* Ma gia:
*  Begin
*      Input n
*      Tach n thanh 4 nhom 3 chu so
*      For moi nhom (tu cao xuong thap) Do
*          If nhom != 0 Then doc nhom, ghep hau to, dua vao danh sach
*      End For
*      Print so co dau phay ngan cach hang nghin va cac nhom da doc
*  End
*/

using System;
using MyLib;

class Buoi8_BT5
{
    static void Main()
    {
        Console.Write("Moi ban nhap so nguyen n: ");
        long n = long.Parse(Console.ReadLine()!.Trim());

        string ketQua = Buoi08_DocSoTiLib.DocSo(n);
        Console.WriteLine(ketQua);
    }
}
