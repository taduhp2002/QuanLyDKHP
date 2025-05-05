using QuanLyDKHP.Filters;
using QuanLyDKHP.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyDKHP.Areas.Admin.Controllers
{
    [AuthorizeRole("Admin")]
    public class QLHocPhanController : Controller
    {
        private DKHPDbContext db = new DKHPDbContext();

        // 1. DANH SÁCH HỌC PHẦN - chỉ xem danh sách
        public ActionResult DanhSach()
        {
            var hocPhans = db.HocPhans.ToList();
            return View(hocPhans);
        }

        // 2. QUẢN LÝ HỌC PHẦN - CRUD + thống kê
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
                var old = db.HocPhans.Find(hocPhan.ID_HP);
                if (old != null)
                {
                    hocPhan.DeCuong = old.DeCuong; // giữ lại đề cương cũ
                    db.Entry(old).CurrentValues.SetValues(hocPhan);
                    db.SaveChanges();
                    TempData["Success"] = "Cập nhật học phần thành công!";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy học phần để cập nhật!";
                }
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
            var hocPhan = db.HocPhans.Find(id);
            if (hocPhan != null)
            {
                var relatedClasses = db.LopHPs.Where(l => l.ID_HP == hocPhan.ID_HP).ToList();
                if (relatedClasses.Any())
                {
                    TempData["Error"] = "Không thể xóa học phần vì có lớp học phần liên quan.";
                }
                else
                {
                    db.HocPhans.Remove(hocPhan);
                    db.SaveChanges();
                    TempData["Success"] = "Xóa học phần thành công!";
                }
            }
            else
            {
                TempData["Error"] = "Không tìm thấy học phần!";
            }
            return RedirectToAction("QuanLy");
        }

        // 3. ĐỀ CƯƠNG CHI TIẾT
        public ActionResult DeCuong()
        {
            var hocPhans = db.HocPhans.ToList();
            ViewBag.CoDeCuong = hocPhans.Count(h => !string.IsNullOrEmpty(h.DeCuong));
            ViewBag.KhongCoDeCuong = hocPhans.Count(h => string.IsNullOrEmpty(h.DeCuong));
            return View(hocPhans);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadDeCuong(int id, HttpPostedFileBase file)
        {
            try
            {
                const int maxFileSize = 10 * 1024 * 1024;
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };

                var hocPhan = db.HocPhans.Find(id);
                if (hocPhan == null)
                {
                    TempData["Error"] = "Không tìm thấy học phần!";
                    return RedirectToAction("DeCuong");
                }

                if (file == null || file.ContentLength == 0)
                {
                    TempData["Error"] = "Vui lòng chọn file hợp lệ!";
                    return RedirectToAction("DeCuong");
                }

                if (file.ContentLength > maxFileSize)
                {
                    TempData["Error"] = "File không được vượt quá 10MB!";
                    return RedirectToAction("DeCuong");
                }

                var ext = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                {
                    TempData["Error"] = "Chỉ chấp nhận file .pdf, .doc, .docx!";
                    return RedirectToAction("DeCuong");
                }

                if (!string.IsNullOrEmpty(hocPhan.DeCuong))
                {
                    var oldFilePath = Server.MapPath(hocPhan.DeCuong);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                string fileName = $"{Guid.NewGuid()}{ext}";
                string relativePath = "/Uploads/DeCuong/" + fileName;
                string fullPath = Server.MapPath(relativePath);

                string dir = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                file.SaveAs(fullPath);

                hocPhan.DeCuong = relativePath;
                db.Entry(hocPhan).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Upload đề cương thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi upload file!";
                System.Diagnostics.Debug.WriteLine("Upload Error: " + ex.ToString());
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
