using Bulky.DataAcess.Repository.IRepository;
using Bulky.Models.Models;
using BulkyWeb.Data;
using BulkyWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAcess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

     

        
        public void Update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }

        public void UpdateStatus(int id, string orderstatus, string? paymentstatus = null)
            {
            var orderFromDb=_db.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (orderFromDb != null) { 
                orderFromDb.OrderStatus = orderstatus;
                if (!string.IsNullOrEmpty(paymentstatus)) { 
                    orderFromDb.PaymentStatus = paymentstatus;}
                
                }
            }

        public void UpdateStripePaymentId(int id, string sessiond, string paymentid)
            {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (!string.IsNullOrEmpty(sessiond)) {
                orderFromDb.SessionId = sessiond;

                }
            if (!string.IsNullOrEmpty(paymentid))
                {
                orderFromDb.PaymentIntentId = paymentid;
                orderFromDb.PaymentDate=System.DateTime.Now;

                }
            }
        }
}
