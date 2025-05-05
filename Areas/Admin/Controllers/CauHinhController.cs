using QuanLyDKHP.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace QuanLyDKHP.Areas.Admin.Controllers
{
    public class CauHinhController : Controller
    {
        private DKHPDbContext db = new DKHPDbContext();
        // GET: Admin/CauHinh
        public ActionResult Index()
        {
            var cauHinhDKHPs = db.CauHinhDKHPs.Include(c => c.HocKy).Include(c => c.NamHoc);
            return View(cauHinhDKHPs.ToList());
        }

        // GET: Admin/CauHinhDKHP/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var cauHinh = db.CauHinhDKHPs
                            .Include(c => c.HocKy)
                            .Include(c => c.NamHoc)
                            .FirstOrDefault(c => c.ID == id);
            if (cauHinh == null) return HttpNotFound();

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
        public ActionResult Create(CauHinhDKHP model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng học kỳ + năm học
                bool exists = db.CauHinhDKHPs.Any(c => c.ID_HK == model.ID_HK && c.ID_NH == model.ID_NH);
                if (exists)
                {
                    ModelState.AddModelError("", "Đã tồn tại cấu hình cho học kỳ và năm học này.");
                }
                else if (model.NgayBatDau >= model.NgayKetThuc)
                {
                    ModelState.AddModelError("", "Ngày bắt đầu phải nhỏ hơn ngày kết thúc.");
                }
                else
                {
                    db.CauHinhDKHPs.Add(model);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            ViewBag.ID_HK = new SelectList(db.HocKies, "ID_HK", "TenHK", model.ID_HK);
            ViewBag.ID_NH = new SelectList(db.NamHocs, "ID_NH", "TenNH", model.ID_NH);
            return View(model);
        }

        // GET: Admin/CauHinhDKHP/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var model = db.CauHinhDKHPs.Find(id);
            if (model == null) return HttpNotFound();

            ViewBag.ID_HK = new SelectList(db.HocKies, "ID_HK", "TenHK", model.ID_HK);
            ViewBag.ID_NH = new SelectList(db.NamHocs, "ID_NH", "TenNH", model.ID_NH);
            return View(model);
        }

        // POST: Admin/CauHinhDKHP/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CauHinhDKHP model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng (trừ chính nó)
                bool exists = db.CauHinhDKHPs.Any(c => c.ID != model.ID && c.ID_HK == model.ID_HK && c.ID_NH == model.ID_NH);
                if (exists)
                {
                    ModelState.AddModelError("", "Đã tồn tại cấu hình khác cho học kỳ và năm học này.");
                }
                else if (model.NgayBatDau >= model.NgayKetThuc)
                {
                    ModelState.AddModelError("", "Ngày bắt đầu phải nhỏ hơn ngày kết thúc.");
                }
                else
                {
                    db.Entry(model).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            ViewBag.ID_HK = new SelectList(db.HocKies, "ID_HK", "TenHK", model.ID_HK);
            ViewBag.ID_NH = new SelectList(db.NamHocs, "ID_NH", "TenNH", model.ID_NH);
            return View(model);
        }

        // GET: Admin/CauHinhDKHP/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var model = db.CauHinhDKHPs
                          .Include(c => c.HocKy)
                          .Include(c => c.NamHoc)
                          .FirstOrDefault(c => c.ID == id);
            if (model == null) return HttpNotFound();

            return View(model);
        }

        // POST: Admin/CauHinhDKHP/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var model = db.CauHinhDKHPs.Find(id);
            if (model != null)
            {
                db.CauHinhDKHPs.Remove(model);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
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