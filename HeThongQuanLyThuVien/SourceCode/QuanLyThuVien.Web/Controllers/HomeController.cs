using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QuanLyThuVien.Web.Models;

namespace QuanLyThuVien.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Manager()
    {
        return View();
    }

    public IActionResult Admin()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
