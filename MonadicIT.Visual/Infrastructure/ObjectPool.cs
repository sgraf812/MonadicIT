using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;

namespace MonadicIT.Visual.Infrastructure
{
    public class ObjectPool<T> where T:class, new()
    {
        private readonly ISet<T> _pool = new HashSet<T>();

        public T Allocate()
        {
            if (_pool.Count == 0) return new T();
            var obj = _pool.First();
            _pool.Remove(obj);
            return obj;
        }

        public void Free(T obj)
        {
           // _pool.Add(obj);
        }
    }
}