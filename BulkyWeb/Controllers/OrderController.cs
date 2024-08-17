using Bulky.DataAcess.Repository.IRepository;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace BulkyWeb.Controllers
    {
    public class OrderController : Controller
        {
        private readonly IUnitOfWork _unitofwork;
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitofwork = unitOfWork;
            
        }


        public IActionResult Index()
            {
            List<OrderHeader> objorderheader = _unitofwork.OrderHeader.GetAll(includeproperties: "ApplicationUser").ToList();
            return View(objorderheader);
            }

        public IActionResult Details(int id ) {
            OrderVM orderVM = new()
                {
                OrderHeader = _unitofwork.OrderHeader.Get(u => u.Id == id, includeproperties: "ApplicationUser"),
                OrderDetail = _unitofwork.OrderDetail.GetAll(u => u.OrderHeaderId == id, includeproperties: "Product"),
                };
            return View(orderVM);

            }
        }
    }
