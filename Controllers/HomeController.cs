using QuanLyDKHP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyDKHP.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        private readonly DKHPDbContext db = new DKHPDbContext();

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string maSV, string matKhau)
        {
            if (string.IsNullOrEmpty(maSV) || string.IsNullOrEmpty(matKhau))
            {
                ViewBag.Error = "Vui lòng nhập mã sinh viên và mật khẩu.";
                return View();
            }

            var sinhVien = db.SinhViens
                .FirstOrDefault(sv => sv.MaSV == maSV && sv.MatKhau == matKhau);

            if (sinhVien != null)
            {
                Session["UserID"] = sinhVien.ID_SV;
                Session["UserRole"] = "SinhVien";
                Session["UserName"] = sinhVien.HoTen;
                return RedirectToAction("Index", "SinhVien");
            }
            else
            {
                ViewBag.Error = "Mã sinh viên hoặc mật khẩu không đúng.";
                return View();
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}