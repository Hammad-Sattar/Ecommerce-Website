using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Bulky.DataAcess.Repository.IRepository;
using BulkyWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace Bulky.DataAcess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        private DbSet<T> _dbSet;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this._dbSet = _db.Set<T>();
            //below is equavilent
            //_db.Categories.Add()==dbset.Add()
            _db.Products.Include(u => u.Category).Include(u => u.CategoryId);

        }
        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public T Get(Expression<Func<T, bool>> filter, string? includeproperties = null)
        {
            IQueryable<T> query = _dbSet;
             query=query.Where(filter);
            if (!string.IsNullOrEmpty(includeproperties))
            {
                foreach (var property in includeproperties.
                    Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }
            return query.FirstOrDefault();
        }

        public IEnumerable<T> GetAll(string? includeproperties=null)
        {
            IQueryable<T> query = _dbSet;
            if (!string.IsNullOrEmpty(includeproperties)) 
            {
                foreach (var property in includeproperties.
                    Split(new char[] {','},StringSplitOptions.RemoveEmptyEntries)) 
                {
                    query = query.Include(property);
                }
            }
            return query.ToList(); 
        }

        public void Remove(T entity)
        {
           _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
    }

}
