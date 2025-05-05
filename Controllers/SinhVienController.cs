using QuanLyDKHP.Filters;
using QuanLyDKHP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace QuanLyDKHP.Controllers
{
    [AuthorizeRole("SinhVien")]
    public class SinhVienController : Controller
    {
        private DKHPDbContext db = new DKHPDbContext();
        public ActionResult Index()
        {
            // Lấy ID sinh viên từ Session
            int? id_sv = Session["UserID"] as int?;
            if (id_sv == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Tìm sinh viên theo ID
            var sinhVien = db.SinhViens.Find(id_sv);
            if (sinhVien == null)
            {
                return HttpNotFound("Không tìm thấy sinh viên");
            }

            // Lấy danh sách các lớp học phần đang mở (TrangThai = 1)
            var lopDangMo = db.LopHPs
                .Include(l => l.HocPhan)
                .Include(l => l.GiangVien)
                .Include(l => l.HocKy)
                .Include(l => l.NamHoc)
                .Where(l => l.TrangThai == 1)
                .ToList();

            // Truyền qua ViewBag (vì bạn không dùng ViewModel)
            ViewBag.LopDangMo = lopDangMo;

            // Truyền SinhVien làm model chính
            return View(sinhVien);
        }
        public ActionResult LichHoc(int? idNH, int? idHK)
        {
           
            int? id_sv = Session["UserID"] as int?;

            // Danh sách lọc
            ViewBag.ID_NH = new SelectList(db.NamHocs.OrderByDescending(n => n.TenNH), "ID_NH", "TenNH", idNH);
            ViewBag.ID_HK = new SelectList(db.HocKies, "ID_HK", "TenHK", idHK);

            var dkhp = db.DKHPs.Where(d => d.ID_SV == id_sv && d.TrangThai == 1).Select(d => d.LopHP);

            if (idNH.HasValue)
                dkhp = dkhp.Where(l => l.ID_NH == idNH.Value);
            if (idHK.HasValue)
                dkhp = dkhp.Where(l => l.ID_HK == idHK.Value);

            var lichHocs = dkhp.SelectMany(l => l.LichHocs).ToList();

            return View(lichHocs);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && db != null)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}