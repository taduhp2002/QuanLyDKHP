using QuanLyDKHP.Filters;
using QuanLyDKHP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace QuanLyDKHP.Controllers
{
    [AuthorizeRole("SinhVien")]
    public class TrangChuController : Controller
    {
        private DKHPDbContext db = new DKHPDbContext();

        // Trang chủ sinh viên sau khi đăng nhập
        public ActionResult Index()
        {
            int? id_sv = Session["UserID"] as int?;

            // Lấy thông tin sinh viên
            var sinhVien = db.SinhViens.Find(id_sv);

            // Lấy cấu hình đăng ký hiện tại (mới nhất)
            var now = DateTime.Now;
            var cauHinh = db.CauHinhDKHPs
                            .Where(c => c.NgayBatDau <= now && c.NgayKetThuc >= now)
                            .OrderByDescending(c => c.NgayBatDau)
                            .FirstOrDefault();

            // Đếm số học phần đã đăng ký trong kỳ hiện tại
            var soTC = 0;
            if (cauHinh != null)
            {
                var dkhpTrongKy = from d in db.DKHPs
                                  join l in db.LopHPs on d.ID_LHP equals l.ID_LHP
                                  join hp in db.HocPhans on l.ID_HP equals hp.ID_HP
                                  where d.ID_SV == id_sv &&
                                        l.ID_HK == cauHinh.ID_HK &&
                                        l.ID_NH == cauHinh.ID_NH &&
                                        d.TrangThai == 1
                                  select hp.SoTC;

                soTC = dkhpTrongKy.Any() ? dkhpTrongKy.Sum() : 0;
            }

            // Truyền dữ liệu qua ViewBag
            ViewBag.SinhVien = sinhVien;
            ViewBag.CauHinh = cauHinh;
            ViewBag.SoTC = soTC;

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
    }
}