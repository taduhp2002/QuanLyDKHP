using QuanLyDKHP.Filters;
using QuanLyDKHP.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyDKHP.Areas.Admin.Controllers
{
    [AuthorizeRole("Admin")]
    public class LopHPController : Controller
    {
        private DKHPDbContext db = new DKHPDbContext();

        // GET: Admin/LopHP
        public ActionResult Index(int? ID_NH, int? ID_HK)
        {
            {
                ViewBag.NamHocs = new SelectList(db.NamHocs, "ID_NH", "TenNH", ID_NH);
                ViewBag.HocKies = new SelectList(db.HocKies, "ID_HK", "TenHK", ID_HK);

                var lopHPs = db.LopHPs
                    .Include(l => l.HocPhan)
                    .Include(l => l.GiangVien)
                    .Include(l => l.HocKy)
                    .Include(l => l.NamHoc)
                    .AsQueryable();

                // Lọc nếu có chọn học kỳ hoặc năm học
                if (ID_NH.HasValue)
                {
                    lopHPs = lopHPs.Where(l => l.ID_NH == ID_NH);
                }

                if (ID_HK.HasValue)
                {
                    lopHPs = lopHPs.Where(l => l.ID_HK == ID_HK);
                }

                AutoCloseExpiredClasses(); // vẫn giữ tính năng auto đóng

                return View(lopHPs.ToList());
            }
        }

        // GET: Admin/LopHP/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            LopHP lopHP = db.LopHPs
                .Include(l => l.HocPhan)
                .Include(l => l.GiangVien)
                .Include(l => l.HocKy)
                .Include(l => l.NamHoc)
                .Include(l => l.LichHocs)
                .FirstOrDefault(l => l.ID_LHP == id);

            if (lopHP == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách sinh viên đã đăng ký
            ViewBag.DanhSachDangKy = db.DKHPs
                .Where(d => d.ID_LHP == id && d.TrangThai == 1)
                .Include(d => d.SinhVien)
                .ToList();

            return View(lopHP);
        }

        // GET: Admin/LopHP/Create
        public ActionResult Create()
        {
            LoadDropdownLists();

            return View();
        }

        // POST: Admin/LopHP/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_LHP,MaLop,ID_HP,ID_HK,ID_NH,ID_GV,SiSo,TrangThai")] LopHP lopHP)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra mã lớp không trùng
                if (db.LopHPs.Any(l => l.MaLop == lopHP.MaLop))
                {
                    ModelState.AddModelError("MaLop", "Mã lớp đã tồn tại");
                    LoadDropdownLists(lopHP);
                    return View(lopHP);
                }

                // Xác định trạng thái lớp theo cấu hình
                DateTime now = DateTime.Now;
                var cauHinh = db.CauHinhDKHPs.FirstOrDefault(c => c.ID_HK == lopHP.ID_HK && c.ID_NH == lopHP.ID_NH);

                if (cauHinh != null)
                {
                    if (now >= cauHinh.NgayBatDau && now <= cauHinh.NgayKetThuc)
                    {
                        lopHP.TrangThai = 1; // Mở đăng ký
                    }
                    else
                    {
                        lopHP.TrangThai = 2; // Đóng
                    }
                }
                else
                {
                    lopHP.TrangThai = 2; // Đóng nếu không có cấu hình
                }

                db.LopHPs.Add(lopHP);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Thêm lớp học phần thành công";
                return RedirectToAction("Index");
            }

            LoadDropdownLists(lopHP);
            return View(lopHP);
        }

        private void LoadDropdownLists(LopHP lopHP = null)
        {
            ViewBag.ID_HP = new SelectList(db.HocPhans, "ID_HP", "TenHP", lopHP?.ID_HP);
            ViewBag.ID_GV = new SelectList(db.GiangViens, "ID_GV", "HoTen", lopHP?.ID_GV);
            ViewBag.ID_HK = new SelectList(db.HocKies, "ID_HK", "TenHK", lopHP?.ID_HK);
            ViewBag.ID_NH = new SelectList(db.NamHocs, "ID_NH", "TenNH", lopHP?.ID_NH);
        }


        [HttpGet]
        public ActionResult Edit(int id)
        {
            var lopHP = db.LopHPs.Find(id);
            if (lopHP == null)
            {
                return HttpNotFound();
            }

            LoadDropdownLists(lopHP);

            return View(lopHP);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LopHP lopHP)
        {
            if (ModelState.IsValid)
            {
                DateTime now = DateTime.Now;

                var cauHinh = db.CauHinhDKHPs
                    .FirstOrDefault(c => c.ID_HK == lopHP.ID_HK && c.ID_NH == lopHP.ID_NH);

                if (cauHinh != null)
                {
                    if (now < cauHinh.NgayBatDau)
                    {
                        lopHP.TrangThai = 2; // Đã đóng (chưa mở đăng ký)
                    }
                    else if (now > cauHinh.NgayKetThuc)
                    {
                        var soLuongDangKy = db.DKHPs.Count(d => d.ID_LHP == lopHP.ID_LHP && d.TrangThai == 1);
                        if (soLuongDangKy < lopHP.SiSo * 0.3)
                        {
                            lopHP.TrangThai = 3; // Đã hủy do không đủ 30% sĩ số
                        }
                        else
                        {
                            lopHP.TrangThai = 2; // Đã đóng (đủ sĩ số)
                        }
                    }
                    else
                    {
                        lopHP.TrangThai = 1; // Mở đăng ký (trong thời gian đăng ký)
                    }
                }
                else
                {
                    lopHP.TrangThai = 2; // Mặc định nếu không có cấu hình
                }
                db.Entry(lopHP).State = EntityState.Modified;
                db.SaveChanges();

                TempData["success"] = "Cập nhật lớp học phần thành công.";
                return RedirectToAction("Index");
            }

            // Nếu model không hợp lệ thì load lại dropdown
            LoadDropdownLists(lopHP);

            return View(lopHP);
        }



        // POST: Admin/LopHP/ChangeStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeStatus(int id, int status)
        {
            LopHP lopHP = db.LopHPs.Find(id);
            if (lopHP == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy lớp học phần";
                return RedirectToAction("Index");
            }

            if (status == 2) // Đóng
            {
                var config = db.CauHinhDKHPs.FirstOrDefault(c => c.ID_HK == lopHP.ID_HK && c.ID_NH == lopHP.ID_NH);
                if (config != null && config.NgayKetThuc > DateTime.Now)
                {
                    TempData["ErrorMessage"] = "Chưa đến hạn đóng đăng ký";
                    return RedirectToAction("Index");
                }
            }
            else if (status == 3) // Hủy
            {
                var count = db.DKHPs.Count(d => d.ID_LHP == id && d.TrangThai == 1);
                if (count >= lopHP.SiSo * 0.3)
                {
                    TempData["ErrorMessage"] = "Số lượng đăng ký đủ để mở lớp, không thể hủy.";
                    return RedirectToAction("Index");
                }
            }

            lopHP.TrangThai = (byte)status;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật trạng thái thành công";
            return RedirectToAction("Index");
        }


        // GET: Admin/LopHP/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            LopHP lopHP = db.LopHPs
                .Include(l => l.HocPhan)
                .Include(l => l.GiangVien)
                .Include(l => l.HocKy)
                .Include(l => l.NamHoc)
                .FirstOrDefault(l => l.ID_LHP == id);

            if (lopHP == null)
            {
                return HttpNotFound();
            }

            return View(lopHP);
        }

        // POST: Admin/LopHP/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            LopHP lopHP = db.LopHPs.Find(id);

            // Kiểm tra nếu lớp đã có sinh viên đăng ký thì không xóa
            if (db.DKHPs.Any(d => d.ID_LHP == id))
            {
                TempData["ErrorMessage"] = "Không thể xóa lớp đã có sinh viên đăng ký";
                return RedirectToAction("Index");
            }

            // Xóa các lịch học liên quan trước
            var lichHocs = db.LichHocs.Where(l => l.ID_LHP == id).ToList();
            db.LichHocs.RemoveRange(lichHocs);

            db.LopHPs.Remove(lopHP);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Xóa lớp học phần thành công";
            return RedirectToAction("Index");
        }

        // Tự động đóng các lớp hết hạn đăng ký
        private void AutoCloseExpiredClasses()
        {
            var currentDate = DateTime.Now;
            var expiredClasses = db.LopHPs
                .Where(l => l.TrangThai == 1 &&
                           db.CauHinhDKHPs.Any(c => c.ID_HK == l.ID_HK &&
                                                   c.ID_NH == l.ID_NH &&
                                                   c.NgayKetThuc < currentDate))
                .ToList();

            foreach (var cls in expiredClasses)
            {
                cls.TrangThai = 2; // Đóng
            }

            if (expiredClasses.Any())
            {
                db.SaveChanges();
            }
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