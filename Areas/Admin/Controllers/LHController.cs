using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using QuanLyDKHP.Models;
using QuanLyDKHP.Filters;

namespace QuanLyDKHP.Areas.Admin.Controllers
{
    [AuthorizeRole("Admin")]
    public class LHController : Controller
    {
        private DKHPDbContext db = new DKHPDbContext();

        public ActionResult Index(int? id_hk, int? id_nh)
        {
            var lichHocs = db.LichHocs
                .Include("LopHP.HocPhan")
                .Include("LopHP.HocKy")
                .Include("LopHP.NamHoc")
                .AsQueryable();

            if (id_hk.HasValue)
                lichHocs = lichHocs.Where(lh => lh.LopHP.ID_HK == id_hk.Value);
            if (id_nh.HasValue)
                lichHocs = lichHocs.Where(lh => lh.LopHP.ID_NH == id_nh.Value);

            ViewBag.HocKys = db.HocKies.ToList();
            ViewBag.NamHocs = db.NamHocs.ToList();
            return View(lichHocs.ToList());
        }

        // GET: LichHoc/Create
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.LopHPs = db.LopHPs
                .Select(lhp => new { lhp.ID_LHP, Display = lhp.MaLop + " - " + lhp.HocPhan.TenHP })
                .ToList();
            return PartialView("_Create");
        }

        // POST: LichHoc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LichHoc lichHoc)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra TuNgay < DenNgay
                if (lichHoc.TuNgay >= lichHoc.DenNgay)
                {
                    ModelState.AddModelError("", "Ngày bắt đầu phải nhỏ hơn ngày kết thúc.");
                    ViewBag.LopHPs = db.LopHPs
                        .Select(lhp => new { lhp.ID_LHP, Display = lhp.MaLop + " - " + lhp.HocPhan.TenHP })
                        .ToList();
                    return PartialView("_Create", lichHoc);
                }

                // Kiểm tra trùng lịch
                var conflict = db.LichHocs.Any(lh =>
                    lh.ID_LHP == lichHoc.ID_LHP &&
                    lh.Thu == lichHoc.Thu &&
                    lh.Tiet == lichHoc.Tiet);
                if (conflict)
                {
                    ModelState.AddModelError("", "Lớp học phần đã có lịch học trùng thứ và tiết.");
                    ViewBag.LopHPs = db.LopHPs
                        .Select(lhp => new { lhp.ID_LHP, Display = lhp.MaLop + " - " + lhp.HocPhan.TenHP })
                        .ToList();
                    return PartialView("_Create", lichHoc);
                }

                db.LichHocs.Add(lichHoc);
                db.SaveChanges();
                return Json(new { success = true });
            }
            ViewBag.LopHPs = db.LopHPs
                .Select(lhp => new { lhp.ID_LHP, Display = lhp.MaLop + " - " + lhp.HocPhan.TenHP })
                .ToList();
            return PartialView("_Create", lichHoc);
        }

        // GET: LichHoc/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var lichHoc = db.LichHocs.Find(id);
            if (lichHoc == null)
                return HttpNotFound();

            ViewBag.LopHPs = db.LopHPs
                .Select(lhp => new { lhp.ID_LHP, Display = lhp.MaLop + " - " + lhp.HocPhan.TenHP })
                .ToList();
            return PartialView("_Edit", lichHoc);
        }

        // POST: LichHoc/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LichHoc lichHoc)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra TuNgay < DenNgay
                if (lichHoc.TuNgay >= lichHoc.DenNgay)
                {
                    ModelState.AddModelError("", "Ngày bắt đầu phải nhỏ hơn ngày kết thúc.");
                    ViewBag.LopHPs = db.LopHPs
                        .Select(lhp => new { lhp.ID_LHP, Display = lhp.MaLop + " - " + lhp.HocPhan.TenHP })
                        .ToList();
                    return PartialView("_Edit", lichHoc);
                }

                // Kiểm tra trùng lịch (loại trừ chính bản ghi đang sửa)
                var conflict = db.LichHocs.Any(lh =>
                    lh.ID_LH != lichHoc.ID_LH &&
                    lh.ID_LHP == lichHoc.ID_LHP &&
                    lh.Thu == lichHoc.Thu &&
                    lh.Tiet == lichHoc.Tiet);
                if (conflict)
                {
                    ModelState.AddModelError("", "Lớp học phần đã có lịch học trùng thứ và tiết.");
                    ViewBag.LopHPs = db.LopHPs
                        .Select(lhp => new { lhp.ID_LHP, Display = lhp.MaLop + " - " + lhp.HocPhan.TenHP })
                        .ToList();
                    return PartialView("_Edit", lichHoc);
                }

                db.Entry(lichHoc).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true });
            }
            ViewBag.LopHPs = db.LopHPs
                .Select(lhp => new { lhp.ID_LHP, Display = lhp.MaLop + " - " + lhp.HocPhan.TenHP })
                .ToList();
            return PartialView("_Edit", lichHoc);
        }

        // POST: LichHoc/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var lichHoc = db.LichHocs.Find(id);
            if (lichHoc != null)
            {
                db.LichHocs.Remove(lichHoc);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, error = "Không tìm thấy lịch học." });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
