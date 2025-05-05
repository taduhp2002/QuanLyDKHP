using QuanLyDKHP.Filters;
using QuanLyDKHP.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyDKHP.Areas.Admin.Controllers
{
    [AuthorizeRole("Admin")]
    public class HocPhanController : Controller
    {
        private DKHPDbContext db = new DKHPDbContext();

        // 1. DANH SÁCH HỌC PHẦN - chỉ xem danh sách
        public ActionResult DanhSach()
        {
            var hocPhans = db.HocPhans.ToList();
            return View(hocPhans);
        }

        // 2. QUẢN LÝ HỌC PHẦN - CRUD + thống kê tổng/bắt buộc/tự chọn
        public ActionResult QuanLy()
        {
            var hocPhans = db.HocPhans.ToList();
            ViewBag.Tong = hocPhans.Count;
            ViewBag.BatBuoc = hocPhans.Count(h => h.LoaiHP == 1);
            ViewBag.TuChon = hocPhans.Count(h => h.LoaiHP == 2);
            return View(hocPhans);
        }

        [HttpPost]
        public ActionResult Them(HocPhan hocPhan)
        {
            if (ModelState.IsValid)
            {
                db.HocPhans.Add(hocPhan);
                db.SaveChanges();
                TempData["Success"] = "Thêm học phần thành công!";
            }
            else
            {
                TempData["Error"] = "Lỗi khi thêm học phần!";
            }
            return RedirectToAction("QuanLy");
        }

        [HttpPost]
        public ActionResult Sua(HocPhan hocPhan)
        {
            if (ModelState.IsValid)
            {
                db.Entry(hocPhan).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật học phần thành công!";
            }
            else
            {
                TempData["Error"] = "Lỗi khi cập nhật học phần!";
            }
            return RedirectToAction("QuanLy");
        }

        [HttpPost]
        public ActionResult Xoa(int id)
        {
            // Tìm học phần cần xóa
            var hocPhan = db.HocPhans.Find(id);

            // Kiểm tra nếu học phần tồn tại
            if (hocPhan != null)
            {
                // Kiểm tra xem học phần có lớp học phần liên quan không
                var relatedClasses = db.LopHPs.Where(l => l.ID_HP == hocPhan.ID_HP).ToList();

                if (relatedClasses.Any())
                {
                    // Nếu có lớp học phần liên quan, thông báo cho người quản trị
                    TempData["Error"] = "Không thể xóa học phần vì có lớp học phần liên quan. Vui lòng xử lý các lớp học phần trước.";
                }
                else
                {
                    // Nếu không có lớp học phần liên quan, tiến hành xóa học phần
                    db.HocPhans.Remove(hocPhan);
                    db.SaveChanges();
                    TempData["Success"] = "Xóa học phần thành công!";
                }
            }
            else
            {
                TempData["Error"] = "Không tìm thấy học phần!";
            }

            // Quay lại trang quản lý học phần
            return RedirectToAction("QuanLy");
        }

        // 3. ĐỀ CƯƠNG CHI TIẾT - Upload + Thống kê
        public ActionResult DeCuong()
        {
            var hocPhans = db.HocPhans.ToList();
            ViewBag.CoDeCuong = hocPhans.Count(h => !string.IsNullOrEmpty(h.DeCuong));
            ViewBag.KhongCoDeCuong = hocPhans.Count(h => string.IsNullOrEmpty(h.DeCuong));
            return View(hocPhans);
        }

        [HttpPost]
        public ActionResult UploadDeCuong(int id, HttpPostedFileBase file)
        {
            var hocPhan = db.HocPhans.Find(id);
            if (hocPhan == null)
            {
                TempData["Error"] = "Không tìm thấy học phần!";
                return RedirectToAction("DeCuong");
            }

            if (file != null && file.ContentLength > 0)
            {
                // Tạo tên file duy nhất để tránh trùng lặp
                string fileName = Path.GetExtension(file.FileName);
                string filePath = Path.Combine(Server.MapPath("~/Uploads/DeCuong"), fileName);
                file.SaveAs(filePath);

                hocPhan.DeCuong = "/Uploads/DeCuong/" + fileName;
                db.SaveChanges();
                TempData["Success"] = "Upload đề cương thành công!";
            }
            else
            {
                TempData["Error"] = "Vui lòng chọn file hợp lệ!";
            }

            return RedirectToAction("DeCuong");
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