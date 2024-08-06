using Bulky.DataAcess.Repository.IRepository;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Collections.Generic;

namespace BulkyWeb.Controllers
{
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork db, IWebHostEnvironment webHostEnvironment)
        {
            _unitofwork = db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> ProductList = _unitofwork.Product.GetAll(includeproperties: "Category").ToList();

            return View(ProductList);
        }
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Categorylist = _unitofwork.Category.GetAll()
                    .Select(u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    }),
                Product = new Product()
            };

            if (id == null || id == 0)
            {
                // Create a new product
                return View(productVM);
            }
            else
            {
                // Update an existing product
                productVM.Product = _unitofwork.Product.Get(u => u.Id == id);
                return View(productVM);
            }

        }
        [HttpPost]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {

            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {

                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productpath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(obj.Product.ImageUrl))
                    { //delete the old image

                        var oldimagepath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldimagepath))
                        {
                            System.IO.File.Delete(oldimagepath);
                        }
                    }

                    using (var filestream = new FileStream(Path.Combine(productpath, filename), FileMode.Create))
                    {

                        file.CopyTo(filestream);
                        obj.Product.ImageUrl = @"\images\product\" + filename;

                    }
                }
                if (obj.Product.Id == 0)
                {
                    _unitofwork.Product.Add(obj.Product);
                    _unitofwork.Save();
                    TempData["success"] = "Product Created Succesfully";
                }
                else
                {
                    _unitofwork.Product.Update(obj.Product);
                    _unitofwork.Save();
                    TempData["success"] = "Product Update Succesfully";
                }

                //_unitofwork.Save();
                //TempData["success"] = "Product Created Succesfully";
                return RedirectToAction("Index");
            }
            else
            {

                obj.Categorylist = _unitofwork.Category.GetAll().
                Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(obj);
            }




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
            TempData["success"] = "Product Deleted Succesfully";

            return RedirectToAction("index");
        }
        #region Api Calls
        public IActionResult GetAll()
        {
            List<Product> ProductList = _unitofwork.Product.GetAll(includeproperties: "Category").ToList();
            return Json(new { data = ProductList });

        }
        #endregion

    }
}
