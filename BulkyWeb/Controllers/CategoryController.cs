using BulkyWeb.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoryController( ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            List<Category> CategoryList = _db.Categories.ToList();
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
            }

            if (ModelState.IsValid)
            {

                _db.Categories.Add(obj);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
            
        }
        [HttpGet]

        public IActionResult Edit( int? id)
        {
            if (id == null || id==0) 
            {
                return NotFound();
            }
            Category? categoryfromdb = _db.Categories.Find(id);
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

                _db.Categories.Update(obj);
                _db.SaveChanges();
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
            Category? categoryfromdb = _db.Categories.Find(id);
          
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
            Category? obj = _db.Categories.Find(id);
            if (obj ==null)
            {
                return NotFound();
            }

            
                _db.Categories.Remove(obj);
                _db.SaveChanges();

            return RedirectToAction("index");
        }
    }
}
