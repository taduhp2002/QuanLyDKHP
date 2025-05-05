using QuanLyDKHP.Filters;
using QuanLyDKHP.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace QuanLyDKHP.Areas.Admin.Controllers
{
    [AuthorizeRole("Admin")]
    public class CauHinhDKHPController : Controller
    {
        private DKHPDbContext db = new DKHPDbContext();

        // GET: Admin/CauHinhDKHP
        public ActionResult Index()
        {
            var cauHinhs = db.CauHinhDKHPs
                .Include(c => c.HocKy)
                .Include(c => c.NamHoc)
                .ToList();

            return View(cauHinhs);
        }

        // GET: Admin/CauHinhDKHP/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            CauHinhDKHP cauHinh = db.CauHinhDKHPs
                .Include(c => c.HocKy)
                .Include(c => c.NamHoc)
                .FirstOrDefault(c => c.ID == id);

            if (cauHinh == null)
            {
                return HttpNotFound();
            }

            return View(cauHinh);
        }

        // GET: Admin/CauHinhDKHP/Create
        public ActionResult Create()
        {
            ViewBag.ID_HK = new SelectList(db.HocKies, "ID_HK", "TenHK");
            ViewBag.ID_NH = new SelectList(db.NamHocs, "ID_NH", "TenNH");

            return View();
        }

        // POST: Admin/CauHinhDKHP/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,SoTC_ToiDa,NgayBatDau,NgayKetThuc,ID_HK,ID_NH")] CauHinhDKHP cauHinh)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra ngày kết thúc phải sau ngày bắt đầu
                if (cauHinh.NgayKetThuc <= cauHinh.NgayBatDau)
                {
                    ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải sau ngày bắt đầu");
                    ViewBag.ID_HK = new SelectList(db.HocKies, "ID_HK", "TenHK", cauHinh.ID_HK);
                    ViewBag.ID_NH = new SelectList(db.NamHocs, "ID_NH", "TenNH", cauHinh.ID_NH);
                    return View(cauHinh);
                }

                // Kiểm tra cấu hình cho học kỳ và năm học đã tồn tại chưa
                if (db.CauHinhDKHPs.Any(c => c.ID_HK == cauHinh.ID_HK && c.ID_NH == cauHinh.ID_NH))
                {
                    ModelState.AddModelError("", "Đã tồn tại cấu hình cho học kỳ và năm học này");
                    ViewBag.ID_HK = new SelectList(db.HocKies, "ID_HK", "TenHK", cauHinh.ID_HK);
                    ViewBag.ID_NH = new SelectList(db.NamHocs, "ID_NH", "TenNH", cauHinh.ID_NH);
                    return View(cauHinh);
                }

                db.CauHinhDKHPs.Add(cauHinh);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Thêm cấu hình đăng ký học phần thành công";
                return RedirectToAction("Index");
            }

            ViewBag.ID_HK = new SelectList(db.HocKies, "ID_HK", "TenHK", cauHinh.ID_HK);
            ViewBag.ID_NH = new SelectList(db.NamHocs, "ID_NH", "TenNH", cauHinh.ID_NH);

            return View(cauHinh);
        }

        // GET: Admin/CauHinhDKHP/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            CauHinhDKHP cauHinh = db.CauHinhDKHPs.Find(id);
            if (cauHinh == null)
            {
                return HttpNotFound();
            }

            ViewBag.ID_HK = new SelectList(db.HocKies, "ID_HK", "TenHK", cauHinh.ID_HK);
            ViewBag.ID_NH = new SelectList(db.NamHocs, "ID_NH", "TenNH", cauHinh.ID_NH);

            return View(cauHinh);
        }

        // POST: Admin/CauHinhDKHP/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,SoTC_ToiDa,NgayBatDau,NgayKetThuc,ID_HK,ID_NH")] CauHinhDKHP cauHinh)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra ngày kết thúc phải sau ngày bắt đầu
                if (cauHinh.NgayKetThuc <= cauHinh.NgayBatDau)
                {
                    ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải sau ngày bắt đầu");
                    ViewBag.ID_HK = new SelectList(db.HocKies, "ID_HK", "TenHK", cauHinh.ID_HK);
                    ViewBag.ID_NH = new SelectList(db.NamHocs, "ID_NH", "TenNH", cauHinh.ID_NH);
                    return View(cauHinh);
                }

                // Kiểm tra cấu hình cho học kỳ và năm học đã tồn tại chưa (trừ chính nó)
                if (db.CauHinhDKHPs.Any(c => c.ID_HK == cauHinh.ID_HK &&
                                           c.ID_NH == cauHinh.ID_NH &&
                                           c.ID != cauHinh.ID))
                {
                    ModelState.AddModelError("", "Đã tồn tại cấu hình cho học kỳ và năm học này");
                    ViewBag.ID_HK = new SelectList(db.HocKies, "ID_HK", "TenHK", cauHinh.ID_HK);
                    ViewBag.ID_NH = new SelectList(db.NamHocs, "ID_NH", "TenNH", cauHinh.ID_NH);
                    return View(cauHinh);
                }

                db.Entry(cauHinh).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật cấu hình đăng ký học phần thành công";
                return RedirectToAction("Index");
            }

            ViewBag.ID_HK = new SelectList(db.HocKies, "ID_HK", "TenHK", cauHinh.ID_HK);
            ViewBag.ID_NH = new SelectList(db.NamHocs, "ID_NH", "TenNH", cauHinh.ID_NH);

            return View(cauHinh);
        }

        // GET: Admin/CauHinhDKHP/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            CauHinhDKHP cauHinh = db.CauHinhDKHPs
                .Include(c => c.HocKy)
                .Include(c => c.NamHoc)
                .FirstOrDefault(c => c.ID == id);

            if (cauHinh == null)
            {
                return HttpNotFound();
            }

            return View(cauHinh);
        }

        // POST: Admin/CauHinhDKHP/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CauHinhDKHP cauHinh = db.CauHinhDKHPs.Find(id);

            // Kiểm tra xem có lớp học phần nào đang sử dụng cấu hình này không
            if (db.LopHPs.Any(l => l.ID_HK == cauHinh.ID_HK && l.ID_NH == cauHinh.ID_NH))
            {
                TempData["ErrorMessage"] = "Không thể xóa cấu hình đang được sử dụng bởi các lớp học phần";
                return RedirectToAction("Index");
            }

            db.CauHinhDKHPs.Remove(cauHinh);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Xóa cấu hình đăng ký học phần thành công";
            return RedirectToAction("Index");
        }

        // GET: Admin/CauHinhDKHP/CheckStatus
        public ActionResult CheckStatus()
        {
            var currentDate = DateTime.Now;
            var activeConfig = db.CauHinhDKHPs
                .Include(c => c.HocKy)
                .Include(c => c.NamHoc)
                .Where(c => c.NgayBatDau <= currentDate && c.NgayKetThuc >= currentDate)
                .FirstOrDefault();

            if (activeConfig != null)
            {
                ViewBag.StatusMessage = $"Hiện đang trong thời gian đăng ký học phần cho {activeConfig.HocKy.TenHK} Năm học {activeConfig.NamHoc.TenNH} ";
                ViewBag.IsActive = true;
            }
            else
            {
                ViewBag.StatusMessage = "Hiện không có đợt đăng ký học phần nào đang hoạt động";
                ViewBag.IsActive = false;
            }

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