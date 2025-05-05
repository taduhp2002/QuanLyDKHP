using QuanLyDKHP.Filters;
using QuanLyDKHP.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace QuanLyDKHP.Controllers
{
    [AuthorizeRole("SinhVien")]
    public class ThoiKhoaBieuController : Controller
    {
        private DKHPDbContext db = new DKHPDbContext();

        // GET: ThoiKhoaBieu
        public ActionResult Index(int? idHocKy, int? idNamHoc)
        {
            // Lấy ID sinh viên từ Session
            int? id_sv = Session["UserID"] as int?;
            if (!id_sv.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách học kỳ và năm học cho dropdown
            ViewBag.HocKys = new SelectList(db.HocKies, "ID_HK", "TenHK", idHocKy);
            ViewBag.NamHocs = new SelectList(db.NamHocs, "ID_NH", "TenNH", idNamHoc);

            // Truy vấn lịch học của sinh viên
            var schedules = db.LichHocs
                .Include(l => l.LopHP.HocPhan)
                .Include(l => l.LopHP.GiangVien)
                .Include(l => l.LopHP.HocKy)
                .Include(l => l.LopHP.NamHoc)
                .Where(l => l.LopHP.DKHPs.Any(d => d.ID_SV == id_sv && d.TrangThai == 1));

            // Lọc theo học kỳ và năm học nếu có
            if (idHocKy.HasValue)
            {
                schedules = schedules.Where(l => l.LopHP.ID_HK == idHocKy.Value);
            }
            if (idNamHoc.HasValue)
            {
                schedules = schedules.Where(l => l.LopHP.ID_NH == idNamHoc.Value);
            }

            var scheduleList = schedules.ToList();
            if (!scheduleList.Any())
            {
                ViewBag.Message = "Không tìm thấy lịch học cho học kỳ/năm học này.";
            }

            return View(scheduleList);
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