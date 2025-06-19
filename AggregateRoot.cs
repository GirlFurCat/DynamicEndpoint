using DynamicEndpoint.EFCore;
using DynamicEndpoint.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using static System.Reflection.Metadata.BlobBuilder;

namespace DynamicEndpoint
{
    public class AggregateRoot<T>(dbContext _Db) : IAggregateRoot<T> where T : class
    {
        protected dbContext Db => _Db ?? throw new ArgumentNullException(nameof(_Db), "DbContext cannot be null");

        protected Guid AggregateId { get; } = Guid.NewGuid();

        protected readonly List<T> _Entitys = new();

        public IReadOnlyCollection<T> Entitys => _Entitys.AsReadOnly();

        public async Task<List<T>> ToListAsync(Expression<Func<T, bool>>? where = null) => await Db.Set<T>().Where(where is not null, where!).ToListAsync();

        public IEnumerable<T> AsEnumerable(Expression<Func<T, bool>>? where = null) => Db.Set<T>().Where(where is not null, where!).AsEnumerable();

        public IQueryable<T> AsQueryable(Expression<Func<T, bool>>? where = null) => Db.Set<T>().Where(where is not null, where!);

        public void Update()
        {
            Db.Set<T>().UpdateRange(_Entitys);
            _Entitys.Clear();
        }

        public void Delete() {
            Db.Set<T>().RemoveRange(_Entitys);
            _Entitys.Clear();
        }

        public async Task Insert()
        {
            await Db.Set<T>().AddRangeAsync(_Entitys);
            _Entitys.Clear();
        }

        public async Task<bool> SaveChangesAsync() => await Db.SaveChangesAsync() > 0;

        public bool SaveChanges() => Db.SaveChanges() > 0;
    }
}
