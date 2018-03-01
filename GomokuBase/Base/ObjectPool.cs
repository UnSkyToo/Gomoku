using System;
using System.Collections.Generic;

namespace GomokuBase.Base
{
    public class ObjectPool<T> where T : new()
    {
        public event Action<T> OnSpawn;
        public event Action<T> OnRecycle; 

        private Queue<T> Objects_;

        public ObjectPool()
        {
            Objects_ = new Queue<T>();
        }

        public T Spawn()
        {
            var Obj = Objects_.Count > 0 ? Objects_.Dequeue() : new T();
            OnSpawn?.Invoke(Obj);
            return Obj;
        }

        public void Recycle(T Obj)
        {
            OnRecycle?.Invoke(Obj);
            Objects_.Enqueue(Obj);
        }
    }
}