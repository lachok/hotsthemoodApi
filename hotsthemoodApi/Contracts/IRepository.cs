using System.Collections.Generic;

namespace hotsthemoodApi.Contracts
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T Get(string id);
        T Add(T item);
        bool Remove(string id);
        bool Update(string id, T item);
    }
}