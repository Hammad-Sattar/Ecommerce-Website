using Bulky.DataAcess.Repository.IRepository;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Collections.Generic;

namespace BulkyWeb.Controllers
    {
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
        {
        private readonly IUnitOfWork _unitofwork;

        public CompanyController(IUnitOfWork db)
            {
            _unitofwork = db;

            }
        public IActionResult Index()
            {
            List<Company> CompanyList = _unitofwork.Company.GetAll().ToList();

            return View(CompanyList);
            }
        public IActionResult Upsert(int? id)
            {
            if (id == null || id == 0)
                {
                // Create a new product
                return View(new Company());
                }
            else
                {
                // Update an existing product
                Company company = _unitofwork.Company.Get(u => u.Id == id);
                return View(company);
                }

            }
        [HttpPost]
        public IActionResult Upsert(Company obj)
            {

            if (ModelState.IsValid)
                {


                if (obj.Id == 0)
                    {
                    _unitofwork.Company.Add(obj);
                    _unitofwork.Save();
                    TempData["success"] = "Company Created Succesfully";
                    }
                else
                    {
                    _unitofwork.Company.Update(obj);
                    _unitofwork.Save();
                    TempData["success"] = "Company Update Succesfully";
                    }

                //_unitofwork.Save();
                //TempData["success"] = "Company Created Succesfully";
                return RedirectToAction("Index");
                }
            else
                {
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
            Company? companyfromdb = _unitofwork.Company.Get(u => u.Id == id);

            if (companyfromdb == null)
                {
                return NotFound();
                }


            return View(companyfromdb);
            }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
            {
            if (id == null || id == 0) { return NotFound(); }
            Company? obj = _unitofwork.Company.Get(u => u.Id == id);
            if (obj == null)
                {
                return NotFound();
                }
            _unitofwork.Company.Remove(obj);
            _unitofwork.Save();
            TempData["success"] = "Company Deleted Succesfully";

            return RedirectToAction("index");
            }
        #region Api Calls
        public IActionResult GetAll()
            {
            List<Company> CompanyList = _unitofwork.Company.GetAll().ToList();
            return Json(new { data = CompanyList });

            }
        #endregion

        }
    }
