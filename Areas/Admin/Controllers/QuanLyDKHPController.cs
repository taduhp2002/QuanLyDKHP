using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using QuanLyDKHP.Models;
using QuanLyDKHP.Filters; // Nếu bạn có dùng [AuthorizeRole]

namespace QuanLyDKHP.Areas.Admin.Controllers
{
    [AuthorizeRole("Admin")] // Nếu bạn có phân quyền
    public class QuanLyDKHPController : Controller
    {
        private DKHPDbContext db = new DKHPDbContext();

        // GET: Admin/QuanLyDKHP
        public ActionResult Index(int? idNamHoc, int? idHocKy)
        {
            ViewBag.ID_NamHoc = new SelectList(db.NamHocs, "ID_NH", "TenNH", idNamHoc);
            ViewBag.ID_HocKy = new SelectList(db.HocKies, "ID_HK", "TenHK", idHocKy);

            List<LopHP> lopHPs = new List<LopHP>();
            if (idNamHoc.HasValue && idHocKy.HasValue)
            {
                lopHPs = db.LopHPs
                    .Where(l => l.ID_NH == idNamHoc.Value && l.ID_HK == idHocKy.Value)
                    .Include(l => l.HocPhan)
                    .ToList();
            }

            return View(lopHPs);
        }

        // GET: Admin/QuanLyDKHP/DanhSachSinhVien/5
        public ActionResult DanhSachSinhVien(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Tải danh sách đăng ký học phần
            var dkhps = db.DKHPs
                .Where(d => d.ID_LHP == id && d.TrangThai == 1)
                .Include(d => d.SinhVien)
                .Include(d => d.LopHP.HocPhan) // Bao gồm HocPhan khi lấy thông tin LopHP
                .ToList();

            // Tải lớp học phần và thông tin học phần liên quan
            var lopHP = db.LopHPs
                .Include(l => l.HocPhan) // Tải thông tin HocPhan
                .FirstOrDefault(l => l.ID_LHP == id);

            if (lopHP == null)
                return HttpNotFound();

            // Truyền thông tin lớp học phần vào ViewBag
            ViewBag.LopHP = lopHP;
            ViewBag.ID_LHP = id;

            return View(dkhps);
        }

        // GET: Admin/QuanLyDKHP/ThemSinhVien/5
        public ActionResult ThemSinhVien(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ViewBag.ID_LHP = id;

            var sinhViens = db.SinhViens
                .Where(sv => !db.DKHPs.Any(dk => dk.ID_LHP == id && dk.ID_SV == sv.ID_SV && dk.TrangThai == 1))
                .ToList();

            return View(sinhViens);
        }

        // POST: Admin/QuanLyDKHP/ThemSinhVien
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemSinhVien(int idLHP, int idSV)
        {
            if (ModelState.IsValid)
            {
                var dkhp = new DKHP
                {
                    ID_LHP = idLHP,
                    ID_SV = idSV,
                    TrangThai = 1,
                    ThoiGianDangKy = DateTime.Now
                };

                db.DKHPs.Add(dkhp);
                db.SaveChanges();

                return RedirectToAction("DanhSachSinhVien", new { id = idLHP });
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // POST: Admin/QuanLyDKHP/XoaSinhVien/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XoaSinhVien(int idDK, int idLHP)
        {
            var dkhp = db.DKHPs.Find(idDK);
            if (dkhp == null)
                return HttpNotFound();

            dkhp.TrangThai = 2;
            dkhp.ThoiGianHuy = DateTime.Now;
            db.Entry(dkhp).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("DanhSachSinhVien", new { id = idLHP });
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
