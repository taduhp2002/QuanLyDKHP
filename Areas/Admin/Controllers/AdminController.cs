using QuanLyDKHP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyDKHP.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        private readonly DKHPDbContext db = new DKHPDbContext();
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string maAdmin, string matKhau)
        {
            if (string.IsNullOrEmpty(maAdmin) || string.IsNullOrEmpty(matKhau))
            {
                ViewBag.Error = "Vui lòng nhập mã admin và mật khẩu.";
                return View();
            }

            var admin = db.Admins
                .FirstOrDefault(a => a.MaAdmin == maAdmin && a.MatKhau == matKhau);

            if (admin != null)
            {
                Session["UserID"] = admin.ID_Admin;
                Session["UserRole"] = "Admin";
                Session["UserName"] = admin.HoTen;
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ViewBag.Error = "Mã admin hoặc mật khẩu không đúng.";
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