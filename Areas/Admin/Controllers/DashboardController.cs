using QuanLyDKHP.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using QuanLyDKHP.Models;

namespace QuanLyDKHP.Areas.Admin.Controllers
{
    [AuthorizeRole("Admin")]
    public class DashboardController : Controller
    {
        private DKHPDbContext db = new DKHPDbContext();
        public ActionResult Index()
        {
            // Thông tin tổng quan
            var currentHocKy = db.HocKies.OrderByDescending(h => h.ID_HK).FirstOrDefault();
            var currentNamHoc = db.NamHocs.OrderByDescending(n => n.ID_NH).FirstOrDefault();
            var cauHinh = db.CauHinhDKHPs
                .Where(c => c.ID_HK == currentHocKy.ID_HK && c.ID_NH == currentNamHoc.ID_NH)
                .FirstOrDefault();
            var openCourses = db.LopHPs
                .Where(l => l.TrangThai == 1 && l.ID_HK == currentHocKy.ID_HK && l.ID_NH == currentNamHoc.ID_NH)
                .Count();
            var registeredStudents = db.DKHPs
                .Where(d => d.TrangThai == 1 && d.LopHP.ID_HK == currentHocKy.ID_HK && d.LopHP.ID_NH == currentNamHoc.ID_NH)
                .Select(d => d.ID_SV)
                .Distinct()
                .Count();

            // Dữ liệu cho biểu đồ
            var courseRegistrations = db.LopHPs
                .Where(l => l.ID_HK == currentHocKy.ID_HK && l.ID_NH == currentNamHoc.ID_NH)
                .Select(l => new
                {
                    TenHP = l.HocPhan.TenHP,
                    Registrations = l.DKHPs.Count(d => d.TrangThai == 1)
                })
                .OrderByDescending(x => x.Registrations)
                .Take(5)
                .ToList();

            var courseStatus = new
            {
                Open = db.LopHPs.Count(l => l.TrangThai == 1 && l.ID_HK == currentHocKy.ID_HK && l.ID_NH == currentNamHoc.ID_NH),
                Closed = db.LopHPs.Count(l => l.TrangThai == 2 && l.ID_HK == currentHocKy.ID_HK && l.ID_NH == currentNamHoc.ID_NH),
                Canceled = db.LopHPs.Count(l => l.TrangThai == 3 && l.ID_HK == currentHocKy.ID_HK && l.ID_NH == currentNamHoc.ID_NH)
            };

            // Danh sách nhanh
            var recentRegistrations = db.DKHPs
                .Include(d => d.SinhVien)
                .Include(d => d.LopHP.HocPhan)
                .Where(d => d.TrangThai == 1)
                .OrderByDescending(d => d.ThoiGianDangKy)
                .Take(5)
                .ToList();

            // Truyền dữ liệu sang View
            ViewBag.CurrentHocKy = currentHocKy;
            ViewBag.CurrentNamHoc = currentNamHoc;
            ViewBag.CauHinh = cauHinh;
            ViewBag.OpenCourses = openCourses;
            ViewBag.RegisteredStudents = registeredStudents;
            ViewBag.CourseRegistrations = courseRegistrations;
            ViewBag.CourseStatus = courseStatus;
            ViewBag.RecentRegistrations = recentRegistrations;

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}