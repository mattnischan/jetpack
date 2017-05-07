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
    public unsafe class WritableBuffer : IDisposable
    {
        private int _bufferSize;

        private byte* _bufferPtr;

        private GCHandle _handle;

        private int _currentIndex;

        private bool isDisposed;

        private Encoder _encoder;

        public WritableBuffer(byte[] buffer)
        {
            _bufferSize = buffer.Length;
            _handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            _bufferPtr = (byte*)_handle.AddrOfPinnedObject().ToPointer();
        }

        public WritableBuffer(byte* buffer, int size)
        {
            _bufferPtr = buffer;
            _bufferSize = size;
        }

        public bool WriteValue(bool value)
        {
            var newPos = _currentIndex + 1;
            if (newPos <= _bufferSize)
            {
                *(bool*)_bufferPtr = value;
                _bufferPtr += 1;
                _currentIndex += 1;

                return true;
            }

            return false;
        }

        public bool WriteValue(byte value)
        {
            var newPos = _currentIndex + 1;
            if(newPos <= _bufferSize)
            {
                *_bufferPtr = value;
                _bufferPtr += 1;
                _currentIndex += 1;

                return true;
            }

            return false;
        }

        public bool WriteValue(sbyte value)
        {
            var newPos = _currentIndex + 1;
            if (newPos <= _bufferSize)
            {
                *(sbyte*)_bufferPtr = value;
                _bufferPtr += 1;
                _currentIndex += 1;

                return true;
            }

            return false;
        }

        public bool WriteValue(char value)
        {
            var newPos = _currentIndex + 2;
            if (newPos <= _bufferSize)
            {
                *(char*)_bufferPtr = value;
                _bufferPtr += 2;
                _currentIndex += 2;

                return true;
            }

            return false;
        }

        public bool WriteValue(decimal value)
        {
            var newPos = _currentIndex + 16;
            if (newPos <= _bufferSize)
            {
                *(decimal*)_bufferPtr = value;
                _bufferPtr += 16;
                _currentIndex += 16;

                return true;
            }

            return false;
        }

        public bool WriteValue(double value)
        {
            var newPos = _currentIndex + 8;
            if (newPos <= _bufferSize)
            {
                *(double*)_bufferPtr = value;
                _bufferPtr += 8;
                _currentIndex += 8;

                return true;
            }

            return false;
        }

        public bool WriteValue(float value)
        {
            var newPos = _currentIndex + 4;
            if (newPos <= _bufferSize)
            {
                *(float*)_bufferPtr = value;
                _bufferPtr += 4;
                _currentIndex += 4;

                return true;
            }

            return false;
        }

        public bool WriteValue(int value)
        {
            var newPos = _currentIndex + 4;
            if (newPos <= _bufferSize)
            {
                *(int*)_bufferPtr = value;
                _bufferPtr += 4;
                _currentIndex += 4;

                return true;
            }

            return false;
        }

        public bool WriteValue(uint value)
        {
            var newPos = _currentIndex + 4;
            if (newPos <= _bufferSize)
            {
                *(uint*)_bufferPtr = value;
                _bufferPtr += 4;
                _currentIndex += 4;

                return true;
            }

            return false;
        }

        public bool WriteValue(long value)
        {
            var newPos = _currentIndex + 8;
            if (newPos <= _bufferSize)
            {
                *(long*)_bufferPtr = value;
                _bufferPtr += 8;
                _currentIndex += 8;

                return true;
            }

            return false;
        }

        public bool WriteValue(ulong value)
        {
            var newPos = _currentIndex + 8;
            if (newPos <= _bufferSize)
            {
                *(ulong*)_bufferPtr = value;
                _bufferPtr += 8;
                _currentIndex += 8;

                return true;
            }

            return false;
        }

        public bool WriteValue(short value)
        {
            var newPos = _currentIndex + 2;
            if (newPos <= _bufferSize)
            {
                *(short*)_bufferPtr = value;
                _bufferPtr += 2;
                _currentIndex += 2;

                return true;
            }

            return false;
        }

        public bool WriteValue(ushort value)
        {
            var newPos = _currentIndex + 2;
            if (newPos <= _bufferSize)
            {
                *(ushort*)_bufferPtr = value;
                _bufferPtr += 2;
                _currentIndex += 2;

                return true;
            }

            return false;
        }

        public bool WriteValue(DateTime value)
        {
            var newPos = _currentIndex + 8;
            if (newPos <= _bufferSize)
            {
                *(long*)_bufferPtr = value.Ticks;
                _bufferPtr += 8;
                _currentIndex += 8;

                return true;
            }

            return false;
        }

        public bool WriteValue(Guid value)
        {
            var newPos = _currentIndex + 16;
            if (newPos <= _bufferSize)
            {
                *(Guid*)_bufferPtr = value;
                _bufferPtr += 16;
                _currentIndex += 16;

                return true;
            }

            return false;
        }

        public bool  WriteValue(string value, out int charsWritten)
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

        public bool WriteValue(string value, int startIndex, out int charsWritten)
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

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (_handle != default(GCHandle))
                {
                    _handle.Free();
                }
                isDisposed = true;
            }
        }

         ~WritableBuffer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
