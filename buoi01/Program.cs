using System;
namespace NMLT.Buoi01
{
class HamSo
{
public static void Main(string []args)
{
// Khai báo biến
double x, fx;
// Nhập dữ liệu
Console.Write("Moi ban nhap vao gia tri cua bien so x: ");
x = double.Parse(Console.ReadLine());
// Xử lý
fx = x + Math.Pow(x, 5) / (1 * 2 * 3 * 4 * 5) + Math.Sqrt(Math.Abs(x)) /
Math.Pow(x, 3.0 / 2);
// Xuất dữ liệu
Console.WriteLine("Gia tri cua ham so f({0}) = {1:#.00}.", x, fx);
// Dừng chương trình chờ nhập phím
Console.Read();
}
}
}