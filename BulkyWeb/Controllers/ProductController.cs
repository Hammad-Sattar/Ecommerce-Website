using Bulky.DataAcess.Repository.IRepository;
using Bulky.Models.Models;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers
{
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        public ProductController(IUnitOfWork db)
        {
            _unitofwork = db;
        }
        public IActionResult Index()
        {
            List<Product> ProductList = _unitofwork.Product.GetAll().ToList();
            return View(ProductList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Product obj)
        {
            if (obj.Title == obj.ISBN.ToString())
            {

                ModelState.AddModelError("Name", "Value of Name And Display Order should not be same");
                return View();
            }



            _unitofwork.Product.Add(obj);
            _unitofwork.Save();
            TempData["success"] = "Product Created Succesfully";
            return RedirectToAction("Index");




        }
        [HttpGet]

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product? productfromdb = _unitofwork.Product.Get(u => u.Id == id);
            //Product? productfromdb = _db.Categories.FirstOrDefault(u=>u.Id==id);
            //Product? productfromdb = _db.Categories.Where(u=>u.Id== id).FirstOrDefault();

            if (productfromdb == null)
            {
                return NotFound();
            }


            return View("Edit", productfromdb);
        }
        [HttpPost]
        public IActionResult Edit(Product obj)
        {


            if (ModelState.IsValid)
            {

                _unitofwork.Product.Update(obj);
                _unitofwork.Save();
                TempData["success"] = "Product Updated Succesfully";
                return RedirectToAction("Index");
            }
            return View();

        }
        [HttpGet]

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product? productfromdb = _unitofwork.Product.Get(u => u.Id == id);

            if (productfromdb == null)
            {
                return NotFound();
            }


            return View(productfromdb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            if (id == null || id == 0) { return NotFound(); }
            Product? obj = _unitofwork.Product.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }


            _unitofwork.Product.Remove(obj);
            _unitofwork.Save();
            TempData["success"] = "Category Deleted Succesfully";

            return RedirectToAction("index");
        }
    }
}
