using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using QuanLyDKHP.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rotativa;
using QuanLyDKHP.Filters;
using System.Data.Entity;

namespace QuanLyDKHP.Areas.Admin.Controllers
{
    [AuthorizeRole("Admin")]
    public class ReportController : Controller
    {
        private DKHPDbContext db = new DKHPDbContext();

        public ActionResult StudentCountByClass(int? hocKyId, int? namHocId)
        {
            // 1. Lấy danh sách Học kỳ, Năm học cho Filter
            ViewBag.HocKyList = new SelectList(db.HocKies.ToList(), "ID_HK", "TenHK", hocKyId);
            ViewBag.NamHocList = new SelectList(db.NamHocs.ToList(), "ID_NH", "TenNH", namHocId);

            // 2. Truy vấn cơ sở dữ liệu với đầy đủ các Include cần thiết
            var query = db.LopHPs
                          .Include(lhp => lhp.HocPhan)       // Nạp thông tin Học phần
                          .Include(lhp => lhp.GiangVien)     // Nạp thông tin Giảng viên
                          .Include(lhp => lhp.DKHPs)         // Nạp danh sách ĐKHP
                          .AsQueryable();

            // Áp dụng filter nếu có
            if (hocKyId.HasValue)
            {
                query = query.Where(lhp => lhp.ID_HK == hocKyId.Value);
            }
            if (namHocId.HasValue)
            {
                query = query.Where(lhp => lhp.ID_NH == namHocId.Value);
            }

            // 3. Thực thi truy vấn và lấy kết quả
            var model = query.ToList();

            return View(model);
        }

        // GET: Report/StudentListByClass
        public ActionResult StudentListByClass(int id) // id là ID_LHP
        {
            var lopHP = db.LopHPs
                          .Include(l => l.HocPhan)
                          .FirstOrDefault(l => l.ID_LHP == id);

            if (lopHP == null)
            {
                return HttpNotFound();
            }

            ViewBag.LopHPInfo = $"Lớp: {lopHP.MaLop} - Học phần: {lopHP.HocPhan.TenHP}";

            // Lấy danh sách SinhVien trực tiếp
            var studentList = db.DKHPs
                .Where(dk => dk.ID_LHP == id && dk.TrangThai == 1)
                .Select(dk => dk.SinhVien) // Chọn đối tượng SinhVien
                .OrderBy(sv => sv.HoTen) // Sắp xếp nếu muốn
                .ToList(); // Lấy danh sách SinhVien

            // Truyền List<SinhVien> sang View
            return View(studentList);
        }

        // --- Export Actions ---

        // POST hoặc GET đều được, GET dễ gọi từ link hơn
        public ActionResult ExportStudentCountToExcel(int? hocKyId, int? namHocId)
        {
            // 1. Lấy dữ liệu (Tương tự StudentCountByClass, tạo anonymous type)
            var query = db.LopHPs
                         .Include(lhp => lhp.HocPhan)
                         .Include(lhp => lhp.GiangVien)
                         .AsQueryable();
            if (hocKyId.HasValue) { query = query.Where(lhp => lhp.ID_HK == hocKyId.Value); }
            if (namHocId.HasValue) { query = query.Where(lhp => lhp.ID_NH == namHocId.Value); }

            var reportData = query
                .Select(lhp => new
                {
                    lhp.MaLop,
                    TenHP = lhp.HocPhan.TenHP,
                    TenGV = lhp.GiangVien.HoTen,
                    SiSoToiDa = lhp.SiSo,
                    SoLuongDangKy = db.DKHPs.Count(dk => dk.ID_LHP == lhp.ID_LHP && dk.TrangThai == 1)
                    // Thêm cột Tỷ lệ nếu muốn
                    // TyLe = (lhp.SiSo > 0) ? (double)db.DKHPs.Count(dk => dk.ID_LHP == lhp.ID_LHP && dk.TrangThai == 1) / lhp.SiSo * 100 : 0
                })
                .ToList();

            // 2. Tạo file Excel dùng CloseXML
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ThongKeDKHP");
                var currentRow = 1;

                // Header
                worksheet.Cell(currentRow, 1).Value = "Mã Lớp";
                worksheet.Cell(currentRow, 2).Value = "Tên Học Phần";
                worksheet.Cell(currentRow, 3).Value = "Giảng Viên";
                worksheet.Cell(currentRow, 4).Value = "Sĩ Số Tối Đa";
                worksheet.Cell(currentRow, 5).Value = "SL Đăng Ký";
                // worksheet.Cell(currentRow, 6).Value = "Tỷ Lệ (%)"; // Nếu có

                // Body - CloseXML có thể chèn trực tiếp từ list anonymous type
                worksheet.Cell(currentRow + 1, 1).InsertData(reportData);

                // (Tùy chọn) Định dạng: Tự động điều chỉnh độ rộng cột
                worksheet.Columns().AdjustToContents();

                // 3. Xuất file
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string fileName = $"ThongKeDKHP_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        public ActionResult ExportStudentListToPdf(int id) // id là ID_LHP
        {
            var lopHP = db.LopHPs.Include(l => l.HocPhan).FirstOrDefault(l => l.ID_LHP == id);
            if (lopHP == null) return HttpNotFound();

            ViewBag.LopHPInfo = $"Lớp: {lopHP.MaLop} - Học phần: {lopHP.HocPhan.TenHP}";

            var studentList = db.DKHPs
                .Where(dk => dk.ID_LHP == id && dk.TrangThai == 1)
                .Select(dk => dk.SinhVien)
                .OrderBy(sv => sv.HoTen)
                .ToList();

            // Tạo một View riêng cho PDF (không có layout)
            // Đặt tên là "StudentListPdfView.cshtml" chẳng hạn
            return new ViewAsPdf("StudentListPdfView", studentList)
            {
                FileName = $"DSSV_{lopHP.MaLop}.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                // Thêm các tùy chọn khác nếu cần
                // CustomSwitches = "--print-media-type --load-error-handling ignore"
            };
        }

        // --- Chart Data Action ---
        public ActionResult GetCourseRegistrationChartData(int? hocKyId, int? namHocId)
        {
            // 1. Lấy dữ liệu thống kê theo Học Phần
            var query = db.LopHPs.AsQueryable();
            if (hocKyId.HasValue) { query = query.Where(lhp => lhp.ID_HK == hocKyId.Value); }
            if (namHocId.HasValue) { query = query.Where(lhp => lhp.ID_NH == namHocId.Value); }

            var chartData = query
               .Include(lhp => lhp.HocPhan)
               .GroupBy(lhp => lhp.HocPhan) // Nhóm theo đối tượng HocPhan
               .Select(g => new
               {
                   MaHP = g.Key.MaHP,
                   TenHP = g.Key.TenHP,
                   // Tính tổng số đăng ký từ tất cả các lớp của học phần này
                   TotalDangKy = g.Sum(lhp => db.DKHPs.Count(dk => dk.ID_LHP == lhp.ID_LHP && dk.TrangThai == 1))
                   // Hoặc nếu dùng navigation property và đã Include() DKHPs:
                   // TotalDangKy = g.Sum(lhp => lhp.DKHPs.Count(dk => dk.TrangThai == 1))
               })
               .Where(x => x.TotalDangKy > 0) // Chỉ lấy HP có người đăng ký
               .OrderBy(x => x.TenHP)
               .ToList();

            // 2. Chuẩn bị dữ liệu cho Chart.js
            var labels = chartData.Select(d => $"{d.MaHP} - {d.TenHP}").ToList(); // Nhãn trục X
            var data = chartData.Select(d => d.TotalDangKy).ToList();       // Dữ liệu trục Y

            // 3. Trả về dưới dạng JSON để JavaScript phía client xử lý
            return Json(new { success = true, labels = labels, data = data }, JsonRequestBehavior.AllowGet);
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