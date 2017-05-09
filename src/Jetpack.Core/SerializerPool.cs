using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Jetpack.Core
{
    public class SerializerPool
    {
        private WritableBuffer[] _pool;

        private SemaphoreSlim _signal;

        private int _currentSlot;

        public SerializerPool(Func<WritableBuffer> factory)
        {
            _pool = new WritableBuffer[Environment.ProcessorCount * 2];
            _signal = new SemaphoreSlim(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2);

            for(var i = 0; i < _pool.Length; i++)
            {
                _pool[i] = factory();
            }
        }

        public WritableBuffer Rent()
        {
            _signal.Wait();
            var slot = Interlocked.Increment(ref _currentSlot);
            return _pool[slot];
        }

        public void Return(WritableBuffer writer)
        {
            var slot = Interlocked.Decrement(ref _currentSlot);
            _pool[slot] = writer;
            _signal.Release();
        }
    }
}
