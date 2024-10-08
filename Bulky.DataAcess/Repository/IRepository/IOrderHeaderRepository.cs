﻿using Bulky.Models.Models;
using BulkyWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAcess.Repository.IRepository
{
    public interface IOrderHeaderRepository: IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);
        void UpdateStatus(int id, string orderstatus, string? paymentstatus = null);
        void UpdateStripePaymentId(int id, string sessiond,string paymentid);
        
    }
}
