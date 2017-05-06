using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Jetpack.Core
{
    /// <summary>
    /// A wrapper around a byte buffer that provides the ability to read
    /// values from the buffer.
    /// </summary>
    /// <remarks>
    /// This class is not guaranteed to be concurrency-safe.
    /// </remarks>
    public unsafe class ReadableBuffer : IDisposable
    {
        private int _bufferSize;

        private byte* _bufferPtr;

        private GCHandle _handle;

        private int _currentIndex;

        private bool isDisposed;

        private Decoder _decoder;

        public ReadableBuffer(byte[] buffer)
        {
            _bufferSize = buffer.Length;
            _handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            _bufferPtr = (byte*)_handle.AddrOfPinnedObject().ToPointer();
        }

        public ReadableBuffer(byte* buffer, int size)
        {
            _bufferPtr = buffer;
            _bufferSize = size;
        }

        public bool ReadValue(out bool value)
        {
            var newPos = _currentIndex + 1;
            if (newPos < _bufferSize)
            {
                value = *(bool*)_bufferPtr;
                _bufferPtr += 1;
                _currentIndex += 1;

                return true;
            }

            value = default(bool);
            return false;
        }

        public bool ReadValue(out byte value)
        {
            var newPos = _currentIndex + 1;
            if (newPos < _bufferSize)
            {
                value = *_bufferPtr;
                _bufferPtr += 1;
                _currentIndex += 1;

                return true;
            }

            value = default(byte);
            return false;
        }

        public bool ReadValue(out sbyte value)
        {
            var newPos = _currentIndex + 1;
            if (newPos < _bufferSize)
            {
                value = *(sbyte*)_bufferPtr;
                _bufferPtr += 1;
                _currentIndex += 1;

                return true;
            }

            value = default(sbyte);
            return false;
        }

        public bool ReadValue(out char value)
        {
            var newPos = _currentIndex + 2;
            if (newPos < _bufferSize)
            {
                value = *(char*)_bufferPtr;
                _bufferPtr += 2;
                _currentIndex += 2;

                return true;
            }

            value = default(char);
            return false;
        }

        public bool ReadValue(out decimal value)
        {
            var newPos = _currentIndex + 16;
            if (newPos < _bufferSize)
            {
                value = *(decimal*)_bufferPtr;
                _bufferPtr += 16;
                _currentIndex += 16;

                return true;
            }

            value = default(decimal);
            return false;
        }

        public bool ReadValue(out double value)
        {
            var newPos = _currentIndex + 8;
            if (newPos < _bufferSize)
            {
                value = *(double*)_bufferPtr;
                _bufferPtr += 8;
                _currentIndex += 8;

                return true;
            }

            value = default(double);
            return false;
        }

        public bool ReadValue(out float value)
        {
            var newPos = _currentIndex + 4;
            if (newPos < _bufferSize)
            {
                value = *(float*)_bufferPtr;
                _bufferPtr += 4;
                _currentIndex += 4;

                return true;
            }

            value = default(float);
            return false;
        }

        public bool ReadValue(out int value)
        {
            var newPos = _currentIndex + 4;
            if (newPos < _bufferSize)
            {
                value = *(int*)_bufferPtr;
                _bufferPtr += 4;
                _currentIndex += 4;

                return true;
            }

            value = default(int);
            return false;
        }

        public bool ReadValue(out uint value)
        {
            var newPos = _currentIndex + 4;
            if (newPos < _bufferSize)
            {
                value = *(uint*)_bufferPtr;
                _bufferPtr += 4;
                _currentIndex += 4;

                return true;
            }

            value = default(uint);
            return false;
        }

        public bool ReadValue(out long value)
        {
            var newPos = _currentIndex + 8;
            if (newPos < _bufferSize)
            {
                value = *(long*)_bufferPtr;
                _bufferPtr += 8;
                _currentIndex += 8;

                return true;
            }

            value = default(long);
            return false;
        }

        public bool ReadValue(out ulong value)
        {
            var newPos = _currentIndex + 8;
            if (newPos < _bufferSize)
            {
                value = *(ulong*)_bufferPtr;
                _bufferPtr += 8;
                _currentIndex += 8;

                return true;
            }

            value = default(long);
            return false;
        }

        public bool ReadValue(out short value)
        {
            var newPos = _currentIndex + 2;
            if (newPos < _bufferSize)
            {
                value = *(short*)_bufferPtr;
                _bufferPtr += 2;
                _currentIndex += 2;

                return true;
            }

            value = default(short);
            return false;
        }

        public bool ReadValue(out ushort value)
        {
            var newPos = _currentIndex + 2;
            if (newPos < _bufferSize)
            {
                value = *(ushort*)_bufferPtr;
                _bufferPtr += 2;
                _currentIndex += 2;

                return true;
            }

            value = default(ushort);
            return false;
        }

        public (bool IsRead, int BytesRead, int CharsRead) ReadValue(out string value, int length)
        {
            if (_decoder == null)
            {
                _decoder = Encoding.UTF8.GetDecoder();
            }

            value = new string(' ', length);
            fixed(char* charPtr = value)
            {
                _decoder.Convert(_bufferPtr, (_bufferSize - 1) - _currentIndex, charPtr, value.Length, false, out var bytesUsed, out var charsUsed, out var completed);

                if (charsUsed == value.Length)
                {
                    _decoder.Reset();
                }

                _bufferPtr += bytesUsed;
                _currentIndex += bytesUsed;

                return (charsUsed == value.Length, bytesUsed, charsUsed);
            }
        }

        public (bool IsWritten, int BytesRead, int CharsRead) ReadValue(ref string value, int startIndex, bool reset)
        {
            fixed (char* charPtrOrigin = value)
            {
                char* charPtr = charPtrOrigin + startIndex;

                if (_decoder == null)
                {
                    _decoder = Encoding.UTF8.GetDecoder();
                }

                _decoder.Convert(_bufferPtr, (_bufferSize - 1) - _currentIndex, charPtr, value.Length - startIndex, false, out var bytesUsed, out var charsUsed, out var completed);

                if (charsUsed == (value.Length - startIndex))
                {
                    _decoder.Reset();
                }

                _bufferPtr += bytesUsed;
                _currentIndex += bytesUsed;

                return (charsUsed == (value.Length - startIndex), bytesUsed, charsUsed);
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
            _bufferSize = buffer.Length;
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

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (_handle != null)
                {
                    _handle.Free();
                }
                isDisposed = true;
            }
        }

        ~ReadableBuffer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
