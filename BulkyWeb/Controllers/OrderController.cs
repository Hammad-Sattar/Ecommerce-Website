using Bulky.DataAcess.Repository;
using Bulky.DataAcess.Repository.IRepository;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Security.Claims;

namespace BulkyWeb.Controllers
    {
    [Authorize]
    public class OrderController : Controller
        {
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        private readonly IUnitOfWork _unitofwork;
        public OrderController(IUnitOfWork unitOfWork)
            {
            _unitofwork = unitOfWork;

            }


        public IActionResult Index()
            {
            List<OrderHeader> objorderheader;
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Admin))
                {
                objorderheader = _unitofwork.OrderHeader.GetAll(includeproperties: "ApplicationUser").ToList();

                }
            else
                {

                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userid = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objorderheader = _unitofwork.OrderHeader.GetAll(u => u.ApplicationUserId == userid, 
                    includeproperties: "ApplicationUser").ToList();


                }

            return View(objorderheader);
            }

        public IActionResult Details(int id)
            {
            OrderVM = new()
                {
                OrderHeader = _unitofwork.OrderHeader.Get(u => u.Id == id, includeproperties: "ApplicationUser"),
                OrderDetail = _unitofwork.OrderDetail.GetAll(u => u.OrderHeaderId == id, includeproperties: "Product"),
                };
            return View(OrderVM);

            }


        [ActionName("Details")]
        [HttpPost]
        public IActionResult DetailsOnPayNow() 
            {
            OrderVM.OrderHeader = _unitofwork.OrderHeader.
                 Get(u => u.Id == OrderVM.OrderHeader.Id, includeproperties: "ApplicationUser");
            OrderVM.OrderDetail = _unitofwork.OrderDetail.
                GetAll(u => u.OrderHeaderId == OrderVM.OrderHeader.Id, includeproperties: "Product");

            //stripe logic
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
                {
                SuccessUrl = domain + $"Order/PaymentConfirmation?orderheaderId={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"Order/Details?id={OrderVM.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                };

            foreach (var item in OrderVM.OrderDetail)
                {
                var sessionLineItem = new SessionLineItemOptions
                    {
                    PriceData = new SessionLineItemPriceDataOptions
                        {
                        UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                            Name = item.Product.Title
                            }
                        },
                    Quantity = item.Count
                    };
                options.LineItems.Add(sessionLineItem);
                }


            var service = new SessionService();
            Session session = service.Create(options);
            _unitofwork.OrderHeader.UpdateStripePaymentId(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitofwork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);


            }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetails()
            {
            var orderheaderfromdb = _unitofwork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            var appuser = _unitofwork.ApplicationUser.Get(u => u.Id == orderheaderfromdb.ApplicationUserId);
            orderheaderfromdb.ApplicationUser = appuser;
            if (orderheaderfromdb != null)
                {
                orderheaderfromdb.Name = OrderVM.OrderHeader.Name;
                orderheaderfromdb.ApplicationUser.PhoneNumber = orderheaderfromdb.ApplicationUser.PhoneNumber;
                orderheaderfromdb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
                orderheaderfromdb.City = OrderVM.OrderHeader.City;
                orderheaderfromdb.State = OrderVM.OrderHeader.State;
                orderheaderfromdb.PostalCode = OrderVM.OrderHeader.PostalCode;

                if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
                    {
                    orderheaderfromdb.Carrier = OrderVM.OrderHeader.Carrier;

                    }

                if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
                    {
                    orderheaderfromdb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;

                    }
                }
            _unitofwork.OrderHeader.Update(orderheaderfromdb);
            _unitofwork.Save();
            TempData["success"] = "Order Details Update Succesfully";
            return RedirectToAction(nameof(Details), new { id = orderheaderfromdb.Id });


            }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
            {
            _unitofwork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitofwork.Save();
            TempData["success"] = "Processing....... ";
            return RedirectToAction(nameof(Details), new { id = OrderVM.OrderHeader.Id });

            }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
            {

            var orderheader = _unitofwork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            if (orderheader != null)
                {

                orderheader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
                orderheader.Carrier = OrderVM.OrderHeader.Carrier;
                orderheader.OrderStatus = SD.StatusShipped;
                orderheader.ShippingDate = DateTime.Now;

                if (orderheader.PaymentStatus == SD.PaymentStatusDelayedPayment)
                    {
                    orderheader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
                    }
                _unitofwork.OrderHeader.Update(orderheader);
                _unitofwork.Save();
                TempData["success"] = "Order Shipped Succesfully ";



                }


            return RedirectToAction(nameof(Details), new { id = OrderVM.OrderHeader.Id });

            }

        [HttpPost]
        [Authorize]
        public IActionResult CancelOrder()
            {

            var orderheader = _unitofwork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            if (orderheader != null)
                {
                if (orderheader.PaymentStatus == SD.StatusApproved)
                    {
                    var options = new RefundCreateOptions
                        {

                        Reason = RefundReasons.RequestedByCustomer,
                        PaymentIntent = orderheader.PaymentIntentId
                        };
                    var service = new RefundService();
                    Refund refund = service.Create(options);
                    _unitofwork.OrderHeader.UpdateStatus(orderheader.Id, SD.StatusCancelled, SD.StatusRefunded);

                    }
                else {
                    _unitofwork.OrderHeader.UpdateStatus(orderheader.Id, SD.StatusCancelled, SD.StatusCancelled);
                    }

                _unitofwork.OrderHeader.Update(orderheader);
                _unitofwork.Save();
                TempData["success"] = "Order Cancelled Succesfully ";


                }

            return RedirectToAction(nameof(Details), new { id = OrderVM.OrderHeader.Id });

            }

        
              public IActionResult PaymentConfirmation(int orderheaderId)
            {

            OrderHeader orderHeader = _unitofwork.OrderHeader.Get(u => u.Id == orderheaderId);
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
                {
                //this is an order by Company

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                    {
                    _unitofwork.OrderHeader.UpdateStripePaymentId(orderheaderId, session.Id, session.PaymentIntentId);
                    _unitofwork.OrderHeader.UpdateStatus(orderheaderId, OrderVM.OrderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitofwork.Save();
                    }
                // HttpContext.Session.Clear();

                }

            //  _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Bulky Book",
            //    $"<p>New Order Created - {orderHeader.Id}</p>");

            
            return View(orderheaderId);
            }



        }
    }


