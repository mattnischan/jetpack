using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jetpack.Core
{
    public static class JetpackSerializer
    {
        public static readonly SerializerPool _serializerPool =
            new SerializerPool(() => new WritableBuffer(ArrayPool<byte>.Shared.Rent(4096),
                () => ArrayPool<byte>.Shared.Rent(4096),
                buffer => ArrayPool<byte>.Shared.Return(buffer)));

        private static class Serializer<T>
        {
            public static Action<WritableBuffer, T> WriteObject = SerializerCompiler.BuildAlphaSerializer<T>();
        }

        public static void Serialize<T>(Stream stream, T value)
        {
            var writer = _serializerPool.Rent();
            Serializer<T>.WriteObject(writer, value);

            stream.Write(writer.Buffer, 0, writer.CurrentIndex);
            _serializerPool.Return(writer);
        }
    }
}
