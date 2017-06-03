using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Jetpack.Core
{
    /// <summary>
    /// A lock-free object pool.
    /// </summary>
    public class ConcurrentObjectPool<T>
    {
        /// <summary>
        /// The pool of items.
        /// </summary>
        private T[] _pool;

        /// <summary>
        /// The pool's current item slot.
        /// </summary>
        private int _currentSlot;

        /// <summary>
        /// A factory that creates instances of the items.
        /// </summary>
        private readonly Func<T> _factory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="itemLimit"></param>
        public ConcurrentObjectPool(Func<T> factory, int itemLimit)
        {
            _pool = new T[itemLimit];

            for(var i = 0; i < _pool.Length; i++)
            {
                _pool[i] = factory();
            }

            _currentSlot = _pool.Length - 1;
            _factory = factory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public T Rent()
        {
            var currentSlot = _currentSlot;
            if(currentSlot == 0)
            {
                return _factory();
            }
            else
            {
                var snapshot = Interlocked.CompareExchange(ref _currentSlot, currentSlot - 1, currentSlot);
                if(snapshot != currentSlot)
                {
                    do
                    {
                        currentSlot = _currentSlot;
                        if (currentSlot == 0)
                        {
                            return _factory();
                        }

                        Thread.Sleep(0);

                    } while (currentSlot != Interlocked.CompareExchange(ref _currentSlot, currentSlot - 1, currentSlot));

                    return _pool[currentSlot - 1];
                }

                return _pool[currentSlot - 1];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        public void Return(T writer)
        {
            var currentSlot = _currentSlot;
            if(currentSlot == _pool.Length - 1)
            {
                return;
            }
            else
            {
                var snapshot = Interlocked.CompareExchange(ref _currentSlot, currentSlot + 1, currentSlot);
                if(snapshot != currentSlot)
                {
                    do
                    {
                        currentSlot = _currentSlot;
                        if (currentSlot == _pool.Length - 1)
                        {
                            return;
                        }

                    } while (currentSlot != Interlocked.CompareExchange(ref _currentSlot, currentSlot + 1, currentSlot));

                    _pool[currentSlot + 1] = writer;
                }

                _pool[currentSlot + 1] = writer;
            }
        }
    }
}
