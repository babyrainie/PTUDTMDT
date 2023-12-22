using dailybook.Data;
using dailybook.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace dailybook.Controllers
{
    public class ProductController : Controller
    {
        private readonly SneakerdailyContext db;
        public ProductController(SneakerdailyContext context)
        {
            db = context;
        }
        public IActionResult Index(int? Id)
        {
            var product = db.Products.AsQueryable();
            if (Id.HasValue)
            {
            product = product.Where(p=>p.CatId == Id.Value);
            }
            var result = product.Select(p => new ProductVM
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Price = p.Price ?? 0,
                Image = p.Thumb ?? "",
                ShortDescription = p.ShortDesc ?? "",
                CatName = p.Cat.CatName,

            });

            return View(result);
        }
        public IActionResult Search(string? query)
        {
            var product = db.Products.AsQueryable();
            if (query!=null)
            {
                product = product.Where(p => p.ProductName.Contains(query) );
            }
            var result = product.Select(p => new ProductVM
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Price = p.Price ?? 0,
                Image = p.Thumb ?? "",
                ShortDescription = p.ShortDesc ?? "",
                CatName = p.Cat.CatName,

            });

            return View(result);
        }
        public IActionResult Detail(int id)
        {
            var data = db.Products
                .Include(p => p.Cat)
                .SingleOrDefault(p => p.ProductId == id);
            if (data == null)
            {
                TempData["Message"] = "Product not found";
                return Redirect("/404");
            }
            var result = new ProductDetailVM
            {
                ProductId = data.ProductId,
                ProductName = data.ProductName,
                Price = data.Price ?? 0,
                Description = data.Description ?? string.Empty,
                Image = data.Thumb ?? "",
                ShortDescription = data.ShortDesc ?? "",
                CatName = data.Cat.CatName,
                Stock = data.UnitsInStock ?? 0,
                Score =5,


            };
            return View(result);
        }

    }
}
