using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Products
        public ActionResult Index(int? page, string Searchtext)
        {

            var pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            IEnumerable<Product> items = db.Products.OrderByDescending(x => x.Id);
            if (!string.IsNullOrEmpty(Searchtext))
            {
                items = items.Where(x => x.Title.ToLower().Contains(Searchtext.ToLower()));

            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items);
        }

        public ActionResult Add()
        {
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Product model, List<string> Images, List<int> rDefault)
        {
            if (ModelState.IsValid)
            {
                if (Images != null && Images.Count > 0)
                {
                    for (int i = 0; i < Images.Count; i++)
                    {
                        if (i + 1 == rDefault[0])
                        {
                            model.Image = Images[i];
                            model.ProductImage.Add(new ProductImage
                            {
                                ProductId = model.Id,
                                Image = Images[i],
                                IsDefault = true
                            });
                        }
                        else
                        {
                            model.ProductImage.Add(new ProductImage
                            {
                                ProductId = model.Id,
                                Image = Images[i],
                                IsDefault = false
                            });
                        }
                    }
                }
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                if (string.IsNullOrEmpty(model.SeoTitle))
                {
                    model.SeoTitle = model.Title;
                }
                if (string.IsNullOrEmpty(model.Alias))
                    model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                db.Products.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            return View(model);
        }


        public ActionResult Edit(int id)
        {
            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product model)
        {
            if (ModelState.IsValid)
            {
                var product = db.Products.Find(model.Id);
                if (product == null)
                {
                    return HttpNotFound();
                }

                // Cập nhật thông tin sản phẩm từ model
                product.Title = model.Title;
                product.Alias = model.Alias;
                product.ProductCode = model.ProductCode;
                product.Description = model.Description;
                product.Detail = model.Detail;
                // Không cập nhật trường Image ở đây để giữ nguyên hình ảnh
                product.OriginalPrice = model.OriginalPrice;
                product.Price = model.Price;
                product.PriceSale = model.PriceSale;
                product.Quantity = model.Quantity;
                product.ViewCount = model.ViewCount;
                product.IsHome = model.IsHome;
                product.IsSale = model.IsSale;
                product.IsFeature = model.IsFeature;
                product.IsHot = model.IsHot;
                product.IsActive = model.IsActive;
                product.ProductCategoryId = model.ProductCategoryId;
                product.SeoTitle = model.SeoTitle;
                product.SeoDescription = model.SeoDescription;
                product.SeoKeywords = model.SeoKeywords;
                // Cập nhật các trường thông tin khác ở đây

                // Cập nhật ngày sửa đổi
                product.ModifiedDate = DateTime.Now;

                // Lưu thay đổi vào cơ sở dữ liệu
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Nếu dữ liệu không hợp lệ, hiển thị lại form chỉnh sửa với thông báo lỗi
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            return View(model);
        }



        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                var productImages = db.ProductImages.Where(x => x.ProductId == item.Id).ToList();
                foreach (var img in productImages)
                {
                    db.ProductImages.Remove(img);
                }
                db.SaveChanges(); // Xóa tất cả hình ảnh của sản phẩm

                db.Products.Remove(item);
                db.SaveChanges(); // Xóa sản phẩm

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult DeleteAll()
        {
            try
            {
                var allProducts = db.Products.ToList();
                foreach (var product in allProducts)
                {
                    var productImages = db.ProductImages.Where(x => x.ProductId == product.Id).ToList();
                    foreach (var img in productImages)
                    {
                        db.ProductImages.Remove(img);
                    }
                    db.Products.Remove(product);
                }
                db.SaveChanges(); // Xóa tất cả sản phẩm và hình ảnh

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public ActionResult IsActive(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                item.IsActive = !item.IsActive;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, isAcive = item.IsActive });
            }

            return Json(new { success = false });
        }
        [HttpPost]
        public ActionResult IsHome(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                item.IsHome = !item.IsHome;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, IsHome = item.IsHome });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult IsSale(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                item.IsSale = !item.IsSale;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, IsSale = item.IsSale });
            }

            return Json(new { success = false });
        }
    }
}