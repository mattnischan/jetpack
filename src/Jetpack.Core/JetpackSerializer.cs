using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading;

namespace Jetpack.Core
{
    public static class JetpackSerializer
    {
        public static readonly ConcurrentObjectPool<WritableBuffer> _serializerPool =
            new ConcurrentObjectPool<WritableBuffer>(() => new WritableBuffer(ArrayPool<byte>.Shared.Rent(4096),
                () => ArrayPool<byte>.Shared.Rent(4096),
                buffer => ArrayPool<byte>.Shared.Return(buffer)), 16);

        private static object _syncroot = new object();

        private static SpinLock _lock = new SpinLock();

        public static void Serialize<T>(Stream stream, T value)
        {
            var serializer = SerializerCollection.Get(typeof(T));

            var writer = _serializerPool.Rent();
            serializer(writer, value);

            stream.Write(writer.Buffer, 0, writer.CurrentIndex);
            writer.Reset();
            _serializerPool.Return(writer);
        }
    }
}
