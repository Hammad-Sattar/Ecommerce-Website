using Bulky.DataAcess.Repository.IRepository;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyWeb.Controllers
    {
    [Authorize]
    public class CartController : Controller
        {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController( IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            
        }

        public IActionResult Index()
            {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM = new()
                {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeproperties: "Product")

                };
            foreach (var cart in ShoppingCartVM.ShoppingCartList) { 
                
                cart.Price=GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderTotal += (cart.Price * cart.Count);
                }

            return View(ShoppingCartVM);
            }
        public IActionResult Summary() { 
            
            return View();
            }

        public IActionResult Plus(int cartid) { 
            
            var cartfromdb=_unitOfWork.ShoppingCart.Get(u=>u.Id==cartid);
            cartfromdb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartfromdb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
            
            }

        public IActionResult Minus(int cartid)
            {

            var cartfromdb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartid);
            if (cartfromdb.Count <= 1) { 
                
                //remove from cart
                _unitOfWork.ShoppingCart.Remove(cartfromdb);
                }
            else
                {
                cartfromdb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartfromdb);

                }
           
          
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));

            }

        public IActionResult Remove(int cartid)
            {

            var cartfromdb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartid);
           
            _unitOfWork.ShoppingCart.Remove(cartfromdb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));

            }
        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart) {
            if (shoppingCart.Count >= 50) {

                return shoppingCart.Product.Price;
                
                }
            else
            {
                if (shoppingCart.Product.Price <= 100)
                    {
                    return shoppingCart.Product.Price50;
                    }
                else {

                    return shoppingCart.Product.Price100;
                    
                    }
            }

        }
        }
    }
