using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jetpack.Core
{
    public static class JetpackSerializer
    {
        public static readonly ArrayPool<byte> _pool = ArrayPool<byte>.Shared;

        private static class Serializer<T>
        {
            public static Action<WritableBuffer, T> WriteObject = SerializerCompiler.BuildAlphaSerializer<T>();
        }

        public static void Serialize<T>(Stream stream, T value)
        {
            using (var writer = new WritableBuffer(_pool.Rent(1024), () => _pool.Rent(1024), buf => _pool.Return(buf)))
            {
                Serializer<T>.WriteObject(writer, value);
            }
        }
    }
}
