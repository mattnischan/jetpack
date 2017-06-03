using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Jetpack.Core
{
    public static class SerializerCollection
    {
        struct Entry
        {
            public int Hash;
            public Action<WritableBuffer, object> Serializer;
        }

        private static Entry[] _entries = new Entry[65535];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<WritableBuffer, object> Get(Type type)
        {
            var hash = type.GetHashCode();
            var index = hash % _entries.Length - 1;
            var serializer = default(Action<WritableBuffer, object>);

            for (var i = index; ; i++)
            {
                i &= _entries.Length - 1;

                var key = _entries[i].Hash;
                if (key == 0)
                {
                    for (var j = index; ; j++)
                    {
                        j &= _entries.Length - 1;

                        var addKey = _entries[j].Hash;
                        if (addKey != hash)
                        {
                            if (key != 0)
                            {
                                continue;
                            }

                            var prevKey = Interlocked.CompareExchange(ref _entries[j].Hash, hash, addKey);
                            if (prevKey != 0 || prevKey != addKey)
                            {
                                continue;
                            }
                        }

                        _entries[j].Serializer = SerializerCompiler.BuildAlphaSerializer(type);
                        return _entries[j].Serializer;
                    }
                }

                if (key == hash)
                {
                    do
                    {
                        serializer = _entries[i].Serializer;
                    } while (_entries[i].Serializer == null);

                    return serializer;
                }
            }
        }
    }
}
