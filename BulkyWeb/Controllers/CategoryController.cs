using Bulky.DataAcess.Repository.IRepository;
using Bulky.Utility;
using BulkyWeb.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers
{
    [Authorize(Roles =SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        public CategoryController(IUnitOfWork db)
        {
            _unitofwork = db;
        }
        public IActionResult Index()
        {
            List<Category> CategoryList = _unitofwork.Category.GetAll().ToList();
            
            return View(CategoryList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create( Category obj)
        {
            if (obj.Name == obj.Displayorder.ToString()) {

                ModelState.AddModelError("Name", "Value of Name And Display Order should not be same");
                return View();
            }



            _unitofwork.Category.Add(obj);
            _unitofwork.Save();
                TempData["success"] = "Category Created Succesfully";
                return RedirectToAction("Index");
            
          
           
            
        }
        [HttpGet]

        public IActionResult Edit( int? id)
        {
            if (id == null || id==0) 
            {
                return NotFound();
            }
            Category? categoryfromdb = _unitofwork.Category.Get(u => u.Id == id);
            //Category? categoryfromdb1 = _db.Categories.FirstOrDefault(u=>u.Id==id);
            //Category? categoryfromdb3 = _db.Categories.Where(u=>u.Id== id).FirstOrDefault();

            if (categoryfromdb == null)
            {
                return NotFound();
            }


            return View("Edit",categoryfromdb);
        }
        [HttpPost]
        public IActionResult Edit(Category obj)
        {
           

            if (ModelState.IsValid)
            {

                _unitofwork.Category.Update(obj);
                _unitofwork.Save();
                TempData["success"] = "Category Updated Succesfully";
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
            Category? categoryfromdb = _unitofwork.Category.Get(u => u.Id == id);

            if (categoryfromdb == null)
            {
                return NotFound();
            }


            return View( categoryfromdb);
        }

        [HttpPost,ActionName("Delete")]
        public IActionResult DeletePost(int?id)
        {
            if(id == null || id == 0) { return NotFound(); }
            Category? obj = _unitofwork.Category.Get(u => u.Id == id);
            if (obj ==null)
            {
                return NotFound();
            }


            _unitofwork.Category.Remove(obj);
            _unitofwork.Save();
            TempData["success"] = "Category Deleted Succesfully";

            return RedirectToAction("index");
        }
    }
}
