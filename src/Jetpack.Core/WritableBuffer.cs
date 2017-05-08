using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Jetpack.Core
{
    /// <summary>
    /// A wrapper around a byte buffer that provides the ability to write
    /// values to the buffer.
    /// </summary>
    /// <remarks>
    /// This class is not guaranteed to be concurrency-safe.
    /// </remarks>
    public unsafe struct WritableBuffer : IDisposable
    {
        private int _bufferSize;

        private byte* _bufferPtr;

        private byte[] _buffer;

        private GCHandle _handle;

        private int _currentIndex;

        private bool isDisposed;

        private Encoder _encoder;

        private Func<byte[]> _allocate;
        private Action<byte[]> _flush;

        public WritableBuffer(byte[] buffer, Func<byte[]> allocate, Action<byte[]> flush)
        {
            _buffer = buffer;
            _bufferSize = buffer.Length;

            _handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            _bufferPtr = (byte*)_handle.AddrOfPinnedObject().ToPointer();

            _allocate = allocate;
            _flush = flush;

            _encoder = null;
            isDisposed = false;
            _currentIndex = 0;
        }

        private void Allocate()
        {
            var newBuffer = _allocate();
            Array.Copy(_buffer, _currentIndex, newBuffer, 0, _bufferSize - _currentIndex);

            var oldBuffer = _buffer;
            Reset(newBuffer);
            _flush(_buffer);
        }

        public void WriteValue(bool value)
        {
            while (true)
            {
                var newPos = _currentIndex + 1;
                if (newPos <= _bufferSize)
                {
                    *(bool*)_bufferPtr = value;
                    _bufferPtr += 1;
                    _currentIndex += 1;
                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(byte value)
        {
            while (true)
            {
                var newPos = _currentIndex + 1;
                if (newPos <= _bufferSize)
                {
                    *_bufferPtr = value;
                    _bufferPtr += 1;
                    _currentIndex += 1;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(sbyte value)
        {
            while (true)
            {
                var newPos = _currentIndex + 1;
                if (newPos <= _bufferSize)
                {
                    *(sbyte*)_bufferPtr = value;
                    _bufferPtr += 1;
                    _currentIndex += 1;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(char value)
        {
            while (true)
            {
                var newPos = _currentIndex + 2;
                if (newPos <= _bufferSize)
                {
                    *(char*)_bufferPtr = value;
                    _bufferPtr += 2;
                    _currentIndex += 2;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(decimal value)
        {
            while (true)
            {
                var newPos = _currentIndex + 16;
                if (newPos <= _bufferSize)
                {
                    *(decimal*)_bufferPtr = value;
                    _bufferPtr += 16;
                    _currentIndex += 16;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(double value)
        {
            while (true)
            {
                var newPos = _currentIndex + 8;
                if (newPos <= _bufferSize)
                {
                    *(double*)_bufferPtr = value;
                    _bufferPtr += 8;
                    _currentIndex += 8;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(float value)
        {
            while (true)
            {
                var newPos = _currentIndex + 4;
                if (newPos <= _bufferSize)
                {
                    *(float*)_bufferPtr = value;
                    _bufferPtr += 4;
                    _currentIndex += 4;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(int value)
        {
            while (true)
            {
                var newPos = _currentIndex + 4;
                if (newPos <= _bufferSize)
                {
                    *(int*)_bufferPtr = value;
                    _bufferPtr += 4;
                    _currentIndex += 4;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(uint value)
        {
            while (true)
            {
                var newPos = _currentIndex + 4;
                if (newPos <= _bufferSize)
                {
                    *(uint*)_bufferPtr = value;
                    _bufferPtr += 4;
                    _currentIndex += 4;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(long value)
        {
            while (true)
            {
                var newPos = _currentIndex + 8;
                if (newPos <= _bufferSize)
                {
                    *(long*)_bufferPtr = value;
                    _bufferPtr += 8;
                    _currentIndex += 8;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(ulong value)
        {
            while (true)
            {
                var newPos = _currentIndex + 8;
                if (newPos <= _bufferSize)
                {
                    *(ulong*)_bufferPtr = value;
                    _bufferPtr += 8;
                    _currentIndex += 8;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(short value)
        {
            while (true)
            {
                var newPos = _currentIndex + 2;
                if (newPos <= _bufferSize)
                {
                    *(short*)_bufferPtr = value;
                    _bufferPtr += 2;
                    _currentIndex += 2;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(ushort value)
        {
            while (true)
            {
                var newPos = _currentIndex + 2;
                if (newPos <= _bufferSize)
                {
                    *(ushort*)_bufferPtr = value;
                    _bufferPtr += 2;
                    _currentIndex += 2;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(DateTime value)
        {
            while (true)
            {
                var newPos = _currentIndex + 8;
                if (newPos <= _bufferSize)
                {
                    *(long*)_bufferPtr = value.Ticks;
                    _bufferPtr += 8;
                    _currentIndex += 8;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(Guid value)
        {
            while (true)
            {
                var newPos = _currentIndex + 16;
                if (newPos <= _bufferSize)
                {
                    *(Guid*)_bufferPtr = value;
                    _bufferPtr += 16;
                    _currentIndex += 16;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(string value)
        {
            int index = 0;
            if(!WriteString(value, out var charsWritten))
            {
                var completed = false;
                do
                {
                    Allocate();
                    completed = WriteString(value, index, out charsWritten);
                    index += charsWritten;
                } while (!completed);
            }
        }

        private bool WriteString(string value, out int charsWritten)
        {
            fixed(char* charPtr = value)
            {
                if(_encoder == null)
                {
                    _encoder = Encoding.UTF8.GetEncoder();
                }

                _encoder.Convert(charPtr, value.Length, _bufferPtr, (_bufferSize - 1) - _currentIndex, false, out var charsUsed, out var bytesUsed, out var completed);

                if(charsUsed == value.Length)
                {
                    _encoder.Reset();
                }

                _bufferPtr += bytesUsed;
                _currentIndex += bytesUsed;
                charsWritten = charsUsed;

                return completed;
            }
        }

        private bool WriteString(string value, int startIndex, out int charsWritten)
        {
            fixed (char* charPtrOrigin = value)
            {
                char* charPtr = charPtrOrigin + startIndex;

                if (_encoder == null)
                {
                    _encoder = Encoding.UTF8.GetEncoder();
                }

                _encoder.Convert(charPtr, value.Length - startIndex, _bufferPtr, (_bufferSize - 1) - _currentIndex, false, out var charsUsed, out var bytesUsed, out var completed);

                if (charsUsed == (value.Length - startIndex))
                {
                    _encoder.Reset();
                }

                _bufferPtr += bytesUsed;
                _currentIndex += bytesUsed;
                charsWritten = charsUsed;

                return completed;
            }
        }

        public void Reset(byte[] buffer)
        {
            if (_handle != default(GCHandle))
            {
                _handle.Free();
            }

            _handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            _bufferPtr = (byte*)_handle.AddrOfPinnedObject().ToPointer();
            _currentIndex = 0;
        }

        public void Reset(byte* buffer)
        {
            if (_handle != default(GCHandle))
            {
                _handle.Free();
            }

            _handle = default(GCHandle);
            _bufferPtr = buffer;
            _currentIndex = 0;
        }

        public void Reset()
        {
            _bufferPtr -= _currentIndex;
            _currentIndex = 0;
        }

        public void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (_handle != default(GCHandle))
                {
                    _handle.Free();
                }
                _flush(_buffer);
                isDisposed = true;
            }
        }


        public void Dispose()
        {
            Dispose(true);
        }
    }
}
