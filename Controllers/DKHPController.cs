using QuanLyDKHP.Filters;
using QuanLyDKHP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyDKHP.Controllers
{
    [AuthorizeRole("SinhVien")]
    public class DKHPController : Controller
    {
        private DKHPDbContext db = new DKHPDbContext();
        public ActionResult DangKyHocPhan()
        {
            int? id_sv = Session["UserID"] as int?;
            var now = DateTime.Now;

            var cauHinh = db.CauHinhDKHPs
                            .Where(ch => ch.NgayBatDau <= now && ch.NgayKetThuc >= now)
                            .FirstOrDefault();

            if (cauHinh == null)
            {
                ViewBag.ThongBao = "Chưa tới thời gian đăng ký hoặc đã hết hạn.";
                return View(new List<LopHP>());
            }

            var lopHPs = db.LopHPs
                           .Where(l => l.ID_HK == cauHinh.ID_HK
                                    && l.ID_NH == cauHinh.ID_NH
                                    && l.TrangThai == 1) // chỉ lấy lớp MỞ
                           .ToList();

            return View(lopHPs);
        }
        [HttpPost]
        public ActionResult XuLyDangKy(int idLHP)
        {
            int? id_sv = Session["UserID"] as int?;
            var sinhVien = db.SinhViens.Find(id_sv);
            var lopHP = db.LopHPs.Find(idLHP);

            if (lopHP == null || lopHP.TrangThai != 1)
            {
                TempData["Loi"] = "Lớp học phần không tồn tại hoặc không mở.";
                return RedirectToAction("DangKyHocPhan");
            }

            // ✅ Kiểm tra đã đăng ký học phần này chưa
            bool daDangKy = db.DKHPs.Any(d => d.ID_SV == id_sv && d.ID_LHP == idLHP && d.TrangThai == 1);
            if (daDangKy)
            {
                TempData["Loi"] = "Bạn đã đăng ký lớp học phần này rồi.";
                return RedirectToAction("DangKyHocPhan");
            }

            // ❌ Kiểm tra trùng lịch học
            var dangKy = db.DKHPs.Where(d => d.ID_SV == id_sv && d.TrangThai == 1)
                                 .Select(d => d.LopHP.ID_LHP).ToList();

            var lichTrung = db.LichHocs
                .Where(l => dangKy.Contains(l.ID_LHP))
                .Any(l =>
                    db.LichHocs.Any(l2 => l2.ID_LHP == idLHP && l2.Tiet == l.Tiet && l2.Thu == l.Thu));

            if (lichTrung)
            {
                TempData["Loi"] = "Lịch học bị trùng.";
                return RedirectToAction("DangKyHocPhan");
            }

            // ❌ Kiểm tra điều kiện tiên quyết
            var hpDangKy = lopHP.ID_HP;
            var dkTienQuyet = db.HocPhans.Find(hpDangKy).HocPhans.ToList(); // HP tiên quyết

            foreach (var hpTQ in dkTienQuyet)
            {
                var daHoc = db.DKHPs.Any(d => d.ID_SV == id_sv && d.LopHP.ID_HP == hpTQ.ID_HP && d.TrangThai == 1);
                if (!daHoc)
                {
                    TempData["Loi"] = "Bạn chưa học học phần tiên quyết: " + hpTQ.TenHP;
                    return RedirectToAction("DangKyHocPhan");
                }
            }

            // ❌ Kiểm tra quá số tín chỉ
            var tongTC = db.DKHPs
    .Where(d => d.ID_SV == id_sv
             && d.TrangThai == 1
             && d.LopHP.ID_HK == lopHP.ID_HK
             && d.LopHP.ID_NH == lopHP.ID_NH)
    .Sum(d => (int?)d.LopHP.HocPhan.SoTC) ?? 0;


            int? maxTC = db.CauHinhDKHPs
                .Where(c => c.ID_HK == lopHP.ID_HK && c.ID_NH == lopHP.ID_NH)
                .Select(c => (int?)c.SoTC_ToiDa)
                .FirstOrDefault();

            if (maxTC == null)
            {
                TempData["Loi"] = "Cấu hình số tín chỉ tối đa chưa được thiết lập.";
                return RedirectToAction("DangKyHocPhan");
            }

            int soTC_HocPhanMoi = (lopHP.HocPhan != null) ? lopHP.HocPhan.SoTC : 0;

            if (tongTC + soTC_HocPhanMoi > maxTC.Value)
            {
                TempData["Loi"] = "Vượt quá số tín chỉ cho phép.";
                return RedirectToAction("DangKyHocPhan");
            }

            // ❌ Đủ sĩ số
            var soDangKy = db.DKHPs.Count(d => d.ID_LHP == idLHP && d.TrangThai == 1);
            if (soDangKy >= lopHP.SiSo)
            {
                TempData["Loi"] = "Lớp học phần đã đủ số lượng.";
                return RedirectToAction("DangKyHocPhan");
            }

            // ✅ Lưu vào DB
            var dkhp = new DKHP
            {
                ID_SV = id_sv.Value,
                ID_LHP = idLHP,
                TrangThai = 1,
                ThoiGianDangKy = DateTime.Now
            };
            db.DKHPs.Add(dkhp);
            db.SaveChanges();

            TempData["ThanhCong"] = "Đăng ký thành công.";
            return RedirectToAction("DangKyHocPhan");
        }
        [HttpPost]
        public ActionResult HuyHocPhan(int idDK)
        {
            int? id_sv = Session["UserID"] as int?;


            var dkhp = db.DKHPs.FirstOrDefault(d => d.ID_DK == idDK && d.ID_SV == id_sv && d.TrangThai == 1);
            if (dkhp == null)
            {
                TempData["Loi"] = "Không tìm thấy đăng ký.";
                return RedirectToAction("XemDangKy");
            }

            var cauHinh = db.CauHinhDKHPs
                .FirstOrDefault(c => c.ID_HK == dkhp.LopHP.ID_HK && c.ID_NH == dkhp.LopHP.ID_NH);

            if (cauHinh == null || DateTime.Now > cauHinh.NgayKetThuc)
            {
                TempData["Loi"] = "Quá hạn hủy học phần.";
                return RedirectToAction("XemDangKy");
            }

            dkhp.TrangThai = 2;
            dkhp.ThoiGianHuy = DateTime.Now;
            db.SaveChanges();

            TempData["ThanhCong"] = "Hủy thành công.";
            return RedirectToAction("XemDangKy");
        }
        public ActionResult XemDangKy(int? idHK, int? idNH)
        {
            int? id_sv = Session["UserID"] as int?;

            var hocKyList = db.HocKies.ToList();
            var namHocList = db.NamHocs.ToList();

            ViewBag.HocKys = new SelectList(hocKyList, "ID_HK", "TenHK", idHK);
            ViewBag.NamHocs = new SelectList(namHocList, "ID_NH", "TenNH", idNH);

            var ds = db.DKHPs.Where(d => d.ID_SV == id_sv);

            if (idHK.HasValue)
                ds = ds.Where(d => d.LopHP.ID_HK == idHK.Value);

            if (idNH.HasValue)
                ds = ds.Where(d => d.LopHP.ID_NH == idNH.Value);

            return View(ds.OrderByDescending(d => d.ThoiGianDangKy).ToList());
        }

    }
}