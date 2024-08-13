using Bulky.DataAcess.Repository.IRepository;
using Bulky.Models.Models;
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
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingcart.ApplicationUserId = userId;
            ShoppingCart cartfromdb = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId
            && u.ProductId == shoppingcart.ProductId
            );

            if (cartfromdb != null)
                {
                //Update the Cart 
                cartfromdb.Count += shoppingcart.Count;
                _unitOfWork.ShoppingCart.Update(cartfromdb);



                }
            else
                {
                shoppingcart.Id = 0;

                _unitOfWork.ShoppingCart.Add(shoppingcart);

                //Add the cart
                }
            TempData["Success"] = "Item Added To Cart";

            _unitOfWork.Save();

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
