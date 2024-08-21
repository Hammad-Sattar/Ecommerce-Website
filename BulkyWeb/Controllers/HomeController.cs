using Bulky.DataAcess.Repository.IRepository;
using Bulky.Models.Models;
using Bulky.Utility;
using BulkyWeb.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Controllers
    {
    public class HomeController : Controller
        {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, ApplicationDbContext applicationDbContext)
            {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _context = applicationDbContext;

            }

        public IActionResult Index()
            {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
                {
                HttpContext.Session.SetInt32(SD.SessionCart,
              _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).Count());
                }
            else { 
                
                 HttpContext.Session.Clear();
                }

            IEnumerable<Product> productlist = _unitOfWork.Product.GetAll(includeproperties: "Category");
            return View(productlist);
            }

        public IActionResult Details(int id)
            {
            ShoppingCart Cart = new ShoppingCart()
                {
                Count = 1,
                Product = _unitOfWork.Product.Get(u => u.Id == id, includeproperties: "Category"),
                ProductId = id
                };
            return View(Cart);

            }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingcart)
            {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                {
                return RedirectToAction("Login", "Account");
                }

            shoppingcart.ApplicationUserId = userId;
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId && u.ProductId == shoppingcart.ProductId);

            if (cartFromDb != null)
                {
                // Update the Cart 
                cartFromDb.Count += shoppingcart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                }
            else
                {
                shoppingcart.Id = 0;
                _unitOfWork.ShoppingCart.Add(shoppingcart);
                }

            _unitOfWork.Save();

            // Update session cart count
            HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());

            TempData["Success"] = "Item Added To Cart";
            return RedirectToAction("Index", "Home");
            }




        public IActionResult Privacy()
            {
            return View();
            }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
