using QuanLyDKHP.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyDKHP.Models;

namespace QuanLyDKHP.Controllers
{
    [AuthorizeRole("SinhVien")]
    public class TraCuuController : Controller
    {
        private DKHPDbContext db = new DKHPDbContext();
        // GET: TraCuu
        // GET: HocPhan
        public ActionResult Index(string loaiHPFilter = "", string searchTerm = "")
        {
            // Lấy dữ liệu từ database
            var hocPhans = db.HocPhans.AsQueryable();

            // Áp dụng bộ lọc
            if (!string.IsNullOrEmpty(loaiHPFilter))
            {
                byte loaiHP = byte.Parse(loaiHPFilter);
                hocPhans = hocPhans.Where(h => h.LoaiHP == loaiHP);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                hocPhans = hocPhans.Where(h =>
                    h.MaHP.Contains(searchTerm) ||
                    h.TenHP.Contains(searchTerm)
                );
            }

            // Chuyển sang danh sách và truyền vào View
            var model = hocPhans.ToList();
            ViewBag.LoaiHPFilter = loaiHPFilter;
            ViewBag.SearchTerm = searchTerm;

            return View(model);
        }

        public ActionResult ChiTiet(int id)
        {
            var hocPhan = db.HocPhans.Find(id);
            if (hocPhan == null)
            {
                return HttpNotFound();
            }
            return PartialView("_ChiTietHocPhan", hocPhan);
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