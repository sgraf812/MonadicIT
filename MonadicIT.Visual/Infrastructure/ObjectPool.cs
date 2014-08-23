using System.Collections.Generic;
using System.Linq;

namespace MonadicIT.Visual.Infrastructure
{
    public class ObjectPool<T> where T : class, new()
    {
        private readonly ISet<T> _pool = new HashSet<T>();

        public T Allocate()
        {
            if (_pool.Count == 0) return new T();
            T obj = _pool.First();
            _pool.Remove(obj);
            return obj;
        }

        public void Free(T obj)
        {
            // _pool.Add(obj);
        }
    }
}