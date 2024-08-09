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
    public class ComapnyRepository : Repository<Company>, ICompanyRepository
    {
        private ApplicationDbContext _db;
        public ComapnyRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

     

        
        public void Update(Category obj)
        {
            _db.Update(obj);
        }
    }
}
