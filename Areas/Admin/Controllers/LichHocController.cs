using QuanLyDKHP.Filters;
using QuanLyDKHP.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ClosedXML.Excel;
using System.IO;

namespace QuanLyDKHP.Areas.Admin.Controllers
{
    [AuthorizeRole("Admin")]
    public class LichHocController : Controller
    {
        private readonly DKHPDbContext db = new DKHPDbContext();

        // GET: Admin/LichHoc
        public ActionResult Index(int? ID_NH, int? ID_HK)
        {
            // Lưu giá trị lọc để sử dụng khi thêm/sửa/xuất Excel
            TempData["ID_NH"] = ID_NH;
            TempData["ID_HK"] = ID_HK;

            // Tạo dropdown lists cho năm học và học kỳ
            ViewBag.NamHocs = new SelectList(db.NamHocs, "ID_NH", "TenNH", ID_NH);
            ViewBag.HocKies = new SelectList(db.HocKies, "ID_HK", "TenHK", ID_HK);

            // Truy vấn danh sách lớp học phần
            var lopHPs = db.LopHPs
                .Include(l => l.HocPhan)
                .Include(l => l.GiangVien)
                .Include(l => l.HocKy)
                .Include(l => l.NamHoc)
                .Include(l => l.LichHocs)
                .Where(l => l.TrangThai != 3) // Không lấy lớp bị hủy
                .AsQueryable();

            // Lọc theo năm học và học kỳ
            if (ID_NH.HasValue)
                lopHPs = lopHPs.Where(l => l.ID_NH == ID_NH);
            if (ID_HK.HasValue)
                lopHPs = lopHPs.Where(l => l.ID_HK == ID_HK);

            return View(lopHPs.ToList());
        }

        // GET: Admin/LichHoc/Create
        public ActionResult Create(int? ID_LHP)
        {
            if (ID_LHP == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var lopHP = db.LopHPs.Find(ID_LHP);
            if (lopHP == null || lopHP.TrangThai == 3)
            {
                TempData["ErrorMessage"] = "Lớp học phần không tồn tại hoặc đã bị hủy.";
                return RedirectToAction("Index");
            }

            // Kiểm tra lớp học phần đã có lịch học chưa
            if (db.LichHocs.Any(l => l.ID_LHP == ID_LHP))
            {
                TempData["ErrorMessage"] = "Lớp học phần đã có lịch học. Vui lòng sửa lịch học hiện có.";
                return RedirectToAction("Index");
            }

            var model = new LichHoc
            {
                ID_LHP = ID_LHP.Value,
                TuNgay = DateTime.Today,
                DenNgay = DateTime.Today.AddMonths(3),
                Thu = 2,
                Tiet = "123"
            };

            ViewBag.LopHP = $"{lopHP.MaLop} - {lopHP.HocPhan.TenHP}";
            LoadDropdownLists(model);
            return View(model);
        }

        // POST: Admin/LichHoc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_LHP,Tiet,Thu,PhongHoc,TuNgay,DenNgay")] LichHoc lichHoc)
        {
            var lopHP = db.LopHPs
                .Include(l => l.HocKy)
                .Include(l => l.NamHoc)
                .FirstOrDefault(l => l.ID_LHP == lichHoc.ID_LHP);

            if (lopHP == null || lopHP.TrangThai == 3)
            {
                TempData["ErrorMessage"] = "Lớp học phần không tồn tại hoặc đã bị hủy.";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                if (lopHP.TrangThai != 1)
                {
                    ModelState.AddModelError("", "Chỉ có thể thêm lịch học cho lớp học phần đang mở.");
                }
                else
                {
                    // Kiểm tra phòng học
                    var validPhongHocs = new[] { "A101", "A102", "B201", "B202" };
                    if (!validPhongHocs.Contains(lichHoc.PhongHoc))
                    {
                        ModelState.AddModelError("PhongHoc", "Phòng học không hợp lệ. Vui lòng chọn phòng trong danh sách cho phép.");
                    }

                    // Kiểm tra ngày hợp lệ
                    if (lichHoc.TuNgay > lichHoc.DenNgay)
                    {
                        ModelState.AddModelError("DenNgay", "Ngày kết thúc phải sau ngày bắt đầu.");
                    }

                    // Kiểm tra ngày trong khoảng cấu hình học kỳ
                    var cauHinh = db.CauHinhDKHPs
                        .FirstOrDefault(c => c.ID_HK == lopHP.ID_HK && c.ID_NH == lopHP.ID_NH);
                    if (cauHinh != null && (lichHoc.TuNgay < cauHinh.NgayBatDau || lichHoc.DenNgay > cauHinh.NgayKetThuc))
                    {
                        ModelState.AddModelError("", "Lịch học phải nằm trong khoảng thời gian của học kỳ.");
                    }

                    // Kiểm tra trùng lịch học trong cùng học kỳ và năm học
                    if (IsScheduleConflict(lichHoc, lopHP.ID_HK, lopHP.ID_NH))
                    {
                        ModelState.AddModelError("", "Lịch học bị trùng với lịch khác trong cùng phòng học, học kỳ và năm học.");
                    }

                    if (ModelState.IsValid)
                    {
                        try
                        {
                            db.LichHocs.Add(lichHoc);
                            db.SaveChanges();
                            TempData["SuccessMessage"] = "Thêm lịch học thành công.";
                            return RedirectToAction("Index", new { ID_NH = lopHP.ID_NH, ID_HK = lopHP.ID_HK });
                        }
                        catch (Exception ex)
                        {
                            TempData["ErrorMessage"] = "Đã xảy ra lỗi khi thêm lịch học: " + ex.Message;
                        }
                    }
                }
            }

            ViewBag.LopHP = $"{lopHP.MaLop} - {lopHP.HocPhan.TenHP}";
            LoadDropdownLists(lichHoc);
            return View(lichHoc);
        }

        // GET: Admin/LichHoc/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var lichHoc = db.LichHocs
                .Include(l => l.LopHP)
                .Include(l => l.LopHP.HocPhan)
                .FirstOrDefault(l => l.ID_LH == id);

            if (lichHoc == null)
                return HttpNotFound();

            if (lichHoc.LopHP.TrangThai == 3)
            {
                TempData["ErrorMessage"] = "Lớp học phần đã bị hủy, không thể sửa lịch học.";
                return RedirectToAction("Index");
            }

            ViewBag.LopHP = $"{lichHoc.LopHP.MaLop} - {lichHoc.LopHP.HocPhan.TenHP}";
            LoadDropdownLists(lichHoc);
            return View(lichHoc);
        }

        // POST: Admin/LichHoc/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_LH,ID_LHP,Tiet,Thu,PhongHoc,TuNgay,DenNgay")] LichHoc lichHoc)
        {
            var lopHP = db.LopHPs
                .Include(l => l.HocKy)
                .Include(l => l.NamHoc)
                .FirstOrDefault(l => l.ID_LHP == lichHoc.ID_LHP);

            if (lopHP == null || lopHP.TrangThai == 3)
            {
                TempData["ErrorMessage"] = "Lớp học phần không tồn tại hoặc đã bị hủy.";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                if (lopHP.TrangThai != 1)
                {
                    ModelState.AddModelError("", "Chỉ có thể sửa lịch học cho lớp học phần đang mở.");
                }
                else
                {
                    // Kiểm tra phòng học
                    var validPhongHocs = new[] { "A101", "A102", "B201", "B202" };
                    if (!validPhongHocs.Contains(lichHoc.PhongHoc))
                    {
                        ModelState.AddModelError("PhongHoc", "Phòng học không hợp lệ. Vui lòng chọn phòng trong danh sách cho phép.");
                    }

                    // Kiểm tra ngày hợp lệ
                    if (lichHoc.TuNgay > lichHoc.DenNgay)
                    {
                        ModelState.AddModelError("DenNgay", "Ngày kết thúc phải sau ngày bắt đầu.");
                    }

                    // Kiểm tra ngày trong khoảng cấu hình học kỳ
                    var cauHinh = db.CauHinhDKHPs
                        .FirstOrDefault(c => c.ID_HK == lopHP.ID_HK && c.ID_NH == lopHP.ID_NH);
                    if (cauHinh != null && (lichHoc.TuNgay < cauHinh.NgayBatDau || lichHoc.DenNgay > cauHinh.NgayKetThuc))
                    {
                        ModelState.AddModelError("", "Lịch học phải nằm trong khoảng thời gian của học kỳ.");
                    }

                    // Kiểm tra trùng lịch học
                    if (IsScheduleConflict(lichHoc, lopHP.ID_HK, lopHP.ID_NH, lichHoc.ID_LH))
                    {
                        ModelState.AddModelError("", "Lịch học bị trùng với lịch khác trong cùng phòng học, học kỳ và năm học.");
                    }

                    if (ModelState.IsValid)
                    {
                        try
                        {
                            db.Entry(lichHoc).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["SuccessMessage"] = "Cập nhật lịch học thành công.";
                            return RedirectToAction("Index", new { ID_NH = lopHP.ID_NH, ID_HK = lopHP.ID_HK });
                        }
                        catch (Exception ex)
                        {
                            TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật lịch học: " + ex.Message;
                        }
                    }
                }
            }

            ViewBag.LopHP = $"{lopHP.MaLop} - {lopHP.HocPhan.TenHP}";
            LoadDropdownLists(lichHoc);
            return View(lichHoc);
        }

        // GET: Admin/LichHoc/ExportExcel
        public ActionResult ExportExcel(int? ID_NH, int? ID_HK)
        {
            var lichHocs = db.LichHocs
                .Where(l => l.LopHP.TrangThai != 3)
                .Select(l => new
                {
                    l.ID_LH,
                    l.Tiet,
                    l.Thu,
                    l.PhongHoc,
                    l.TuNgay,
                    l.DenNgay,
                    LopHP = new
                    {
                        l.LopHP.MaLop,
                        l.LopHP.HocPhan.TenHP,
                        l.LopHP.HocPhan.MaHP,
                        l.LopHP.HocPhan.SoTC,
                        l.LopHP.GiangVien.HoTen,
                        l.LopHP.HocKy.TenHK,
                        l.LopHP.HocKy.ID_HK,
                        l.LopHP.NamHoc.TenNH,
                        l.LopHP.NamHoc.ID_NH
                    }
                })
                .AsQueryable();

            if (ID_NH.HasValue)
                lichHocs = lichHocs.Where(l => l.LopHP.ID_NH == ID_NH);
            if (ID_HK.HasValue)
                lichHocs = lichHocs.Where(l => l.LopHP.ID_HK == ID_HK);

            var data = lichHocs.OrderBy(l => l.Thu).ThenBy(l => l.Tiet).ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ThoiKhoaBieu");

                // Thiết lập font mặc định cho toàn bộ sheet
                worksheet.Style.Font.FontName = "Times New Roman";
                worksheet.Style.Font.FontSize = 13;

                // Header Section
                worksheet.Cell(1, 1).Value = "BỘ TÀI NGUYÊN VÀ MÔI TRƯỜNG";
                worksheet.Range(1, 1, 1, 5).Merge();
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                worksheet.Cell(2, 1).Value = "TRƯỜNG ĐẠI HỌC TÀI NGUYÊN VÀ MÔI TRƯỜNG TP. HỒ CHÍ MINH";
                worksheet.Range(2, 1, 2, 5).Merge();
                worksheet.Cell(2, 1).Style.Font.Bold = true;
                worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                worksheet.Cell(1, 6).Value = "CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM";
                worksheet.Range(1, 6, 1, 11).Merge();
                worksheet.Cell(1, 6).Style.Font.Bold = true;
                worksheet.Cell(1, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                worksheet.Cell(2, 6).Value = "Độc lập - Tự do - Hạnh phúc";
                worksheet.Range(2, 6, 2, 11).Merge();
                worksheet.Cell(2, 6).Style.Font.SetUnderline(XLFontUnderlineValues.Single);
                worksheet.Cell(2, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                worksheet.Cell(4, 1).Value = $"THỜI KHÓA BIỂU THỰC HÀNH Đợt 1 - Học kỳ {db.HocKies.Find(ID_HK)?.TenHK} Năm học {db.NamHocs.Find(ID_NH)?.TenNH}";
                worksheet.Range(4, 1, 4, 11).Merge();
                worksheet.Cell(4, 1).Style.Font.Bold = true;
                worksheet.Cell(4, 1).Style.Font.FontSize = 16;
                worksheet.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Table Headers
                worksheet.Cell(6, 1).Value = "STT";
                worksheet.Cell(6, 2).Value = "Lớp";
                worksheet.Cell(6, 3).Value = "Mã môn học";
                worksheet.Cell(6, 4).Value = "Môn học";
                worksheet.Cell(6, 5).Value = "Giảng viên";
                worksheet.Cell(6, 6).Value = "Số TC";
                worksheet.Cell(6, 7).Value = "Thứ";
                worksheet.Cell(6, 8).Value = "Tiết";
                worksheet.Cell(6, 9).Value = "Phòng";
                worksheet.Cell(6, 10).Value = "Thời gian";
                worksheet.Cell(6, 11).Value = "Ghi chú";

                var headerRange = worksheet.Range(6, 1, 6, 11);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Table Data
                int row = 7;
                int stt = 1;
                foreach (var item in lichHocs)
                {
                    worksheet.Cell(row, 1).Value = stt++;
                    worksheet.Cell(row, 2).Value = item.LopHP.MaLop;
                    worksheet.Cell(row, 3).Value = item.LopHP.MaHP;
                    worksheet.Cell(row, 4).Value = item.LopHP.TenHP;
                    worksheet.Cell(row, 5).Value = item.LopHP.HoTen;
                    worksheet.Cell(row, 6).Value = item.LopHP.SoTC;
                    worksheet.Cell(row, 7).Value = item.Thu;
                    worksheet.Cell(row, 8).Value = item.Tiet;
                    worksheet.Cell(row, 9).Value = item.PhongHoc;
                    worksheet.Cell(row, 10).Value = $"{item.TuNgay:dd/MM/yyyy}-{item.DenNgay:dd/MM/yyyy}";
                    worksheet.Cell(row, 11).Value = "";

                    var rowRange = worksheet.Range(row, 1, row, 11);
                    rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    rowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    row++;
                }

                // Footer Section
                worksheet.Cell(row + 1, 1).Value = $"TP. Hồ Chí Minh, ngày 19 tháng 02 năm 2025";
                worksheet.Range(row + 1, 1, row + 1, 5).Merge();
                worksheet.Cell(row + 1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row + 2, 1).Value = "Nguồn lập biểu";
                worksheet.Range(row + 2, 1, row + 2, 5).Merge();
                worksheet.Cell(row + 2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row + 3, 1).Value = "KT. TRƯỜNG PHÒNG ĐÀO TẠO";
                worksheet.Range(row + 3, 1, row + 3, 5).Merge();
                worksheet.Cell(row + 3, 1).Style.Font.Bold = true;
                worksheet.Cell(row + 3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row + 4, 1).Value = "PHÓ TRƯỜNG PHÒNG";
                worksheet.Range(row + 4, 1, row + 4, 5).Merge();
                worksheet.Cell(row + 4, 1).Style.Font.Bold = true;
                worksheet.Cell(row + 4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row + 5, 1).Value = "Hà Anh Đông";
                worksheet.Range(row + 5, 1, row + 5, 5).Merge();
                worksheet.Cell(row + 5, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row + 3, 6).Value = "Võ Thị Thúy Mai";
                worksheet.Range(row + 3, 6, row + 3, 11).Merge();
                worksheet.Cell(row + 3, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Điều chỉnh chiều rộng cột
                worksheet.Column(1).Width = 5; // STT
                worksheet.Columns(2, 11).AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    string fileName = $"ThoiKhoaBieu_{DateTime.Now:yyyyMMdd}.xlsx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        // ===== Helper Methods =====

        private void LoadDropdownLists(LichHoc lichHoc = null)
        {
            ViewBag.TietOptions = new SelectList(new[]
            {
                new { Value = "123", Text = "Tiết 1-3" },
                new { Value = "456", Text = "Tiết 4-6" },
                new { Value = "789", Text = "Tiết 7-9" },
                new { Value = "012", Text = "Tiết 10-12" }
            }, "Value", "Text", lichHoc?.Tiet);

            ViewBag.ThuOptions = new SelectList(new[]
            {
                new { Value = 2, Text = "Thứ 2" },
                new { Value = 3, Text = "Thứ 3" },
                new { Value = 4, Text = "Thứ 4" },
                new { Value = 5, Text = "Thứ 5" },
                new { Value = 6, Text = "Thứ 6" },
                new { Value = 7, Text = "Thứ 7" }
            }, "Value", "Text", lichHoc?.Thu);

            ViewBag.PhongHocs = new SelectList(new[] { "A101", "A102", "B201", "B202" }, lichHoc?.PhongHoc);
        }

        private bool IsScheduleConflict(LichHoc newLichHoc, int idHK, int idNH, int? excludeId = null)
        {
            return db.LichHocs
                .Any(l =>
                    l.ID_LH != excludeId &&
                    l.LopHP.ID_HK == idHK &&
                    l.LopHP.ID_NH == idNH &&
                    l.PhongHoc == newLichHoc.PhongHoc &&
                    l.Thu == newLichHoc.Thu &&
                    l.Tiet == newLichHoc.Tiet &&
                    (newLichHoc.TuNgay <= l.DenNgay && newLichHoc.DenNgay >= l.TuNgay));
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