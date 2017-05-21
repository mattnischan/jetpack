using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Jetpack.Core
{
    public class SerializerPool
    {
        private WritableBuffer[] _pool;

        private int _currentSlot;

        private readonly object _syncroot = new object();

        private readonly Func<WritableBuffer> _factory;

        public SerializerPool(Func<WritableBuffer> factory)
        {
            _pool = new WritableBuffer[Environment.ProcessorCount * 2];

            for(var i = 0; i < _pool.Length; i++)
            {
                _pool[i] = factory();
            }

            _currentSlot = _pool.Length - 1;
            _factory = factory;
        }

        public WritableBuffer Rent()
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

        public void Return(WritableBuffer writer)
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
