using ClosedXML.Excel;
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
    public class ThongKeController : Controller
    {
        private DKHPDbContext db = new DKHPDbContext();

        // GET: /ThongKe
        public ActionResult Index()
        {
            ViewBag.NamHocs = new SelectList(db.NamHocs.ToList(), "ID_NH", "TenNH");
            ViewBag.HocKys = new SelectList(db.HocKies.ToList(), "ID_HK", "TenHK");
            return View();
        }

        public ActionResult SoLuongDangKy(int? idNH, int? idHK)
        {
            var result = db.LopHPs
                .Where(x => x.ID_NH == idNH && x.ID_HK == idHK)
                .Select(lhp => new
                {
                    MaLop = lhp.MaLop,
                    HocPhan = lhp.HocPhan.TenHP,
                    SoLuongDangKy = lhp.DKHPs.Count(dk => dk.TrangThai == 1)
                }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // Xuất Excel
        public ActionResult ExportExcel(int? idNH, int? idHK)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ThongKe");
                worksheet.Cell(1, 1).Value = "Mã lớp";
                worksheet.Cell(1, 2).Value = "Học phần";
                worksheet.Cell(1, 3).Value = "Số sinh viên đăng ký";

                var data = db.LopHPs
                    .Where(x => x.ID_NH == idNH && x.ID_HK == idHK)
                    .Select(lhp => new
                    {
                        MaLop = lhp.MaLop,
                        TenHP = lhp.HocPhan.TenHP,
                        SoLuong = lhp.DKHPs.Count(dk => dk.TrangThai == 1)
                    }).ToList();

                int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = item.MaLop;
                    worksheet.Cell(row, 2).Value = item.TenHP;
                    worksheet.Cell(row, 3).Value = item.SoLuong;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "ThongKe.xlsx");
                }
            }
        }

        // Xuất PDF
        public ActionResult ExportPDF(int? idNH, int? idHK)
        {
            var data = db.LopHPs
                .Where(x => x.ID_NH == idNH && x.ID_HK == idHK)
                .Select(lhp => new
                {
                    MaLop = lhp.MaLop,
                    TenHP = lhp.HocPhan.TenHP,
                    SoLuong = lhp.DKHPs.Count(dk => dk.TrangThai == 1)
                }).ToList();

            return new Rotativa.ViewAsPdf("ThongKePDF", data);
        }
    }
}