using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Products
        public ActionResult Index(int? page)
        {
            //Begin số lượng hiển thị trên 1 page
            var pageSize = 16;
            //End số lượng hiển thị trên 1 page
            if (page == null)
            {
                page = 1;
            }
            IEnumerable<Product> items = db.Products.OrderByDescending(x => x.Id);
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items);
        }

        public ActionResult Detail(string alias, int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                db.Products.Attach(item);
                item.ViewCount = item.ViewCount + 1;
                db.Entry(item).Property(x => x.ViewCount).IsModified = true;
                db.SaveChanges();
            }

            return View(item);
        }
        public ActionResult ProductCategory(string alias, int id, int? page)
        {
            var items = db.Products.ToList();
            if (id > 0)
            {
                items = items.Where(x => x.ProductCategoryId == id).ToList();
            }
            var cate = db.ProductCategories.Find(id);
            if (cate != null)
            {
                ViewBag.CateName = cate.Title;
            }
            int pageSize = 16; // Số lượng sản phẩm trên mỗi trang
            int pageNumber = (page ?? 1); // Số trang hiện tại, mặc định là 1 nếu không có giá trị
            ViewBag.CateId = id;
            return View(items.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Partial_ItemsByCateId()
        {
            var items = db.Products.Where(x => x.IsHome && x.IsActive).Take(12).ToList();
            return PartialView(items);
        }

        public ActionResult Partial_ProductSales()
        {
            var items = db.Products.Where(x => x.IsSale && x.IsActive).Take(12).ToList();
            return PartialView(items);
        }
        public ActionResult Search(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                var allProducts = db.Products.ToList();
                return View(allProducts);
            }
            else
            {
                var products = db.Products.Where(p => p.Title.ToLower().Contains(keyword.ToLower())).ToList();
                return View(products);
            }
        }
    }
}