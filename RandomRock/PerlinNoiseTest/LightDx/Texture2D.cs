﻿using LightDx.Natives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightDx
{
    public sealed class Texture2D : IDisposable
    {
        private readonly LightDevice _device;

        private bool _holdPointer;
        private IntPtr _texture, _view;
        internal IntPtr ViewPtr => _view;
        internal IntPtr TexturePtr => _texture;
        public int Width { get; private set; }
        public int Height { get; private set; }

        private bool _disposed;

        internal Texture2D(LightDevice device, IntPtr tex, IntPtr view, int w, int h, bool holdPtr = true)
        {
            _device = device;
            device.AddComponent(this);

            _holdPointer = holdPtr;

            _texture = tex;
            _view = view;

            Width = w;
            Height = h;
        }

        ~Texture2D()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (_holdPointer)
            {
                NativeHelper.Dispose(ref _texture);
                NativeHelper.Dispose(ref _view);
            }
            else
            {
                _texture = _view = IntPtr.Zero;
            }

            if (disposing)
            {
                _device.RemoveComponent(this);
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        internal void UpdatePointer(int w, int h, IntPtr tex, IntPtr view)
        {
            if (_holdPointer)
            {
                throw new InvalidOperationException();
            }
            Width = w;
            Height = h;
            _texture = tex;
            _view = view;
        }

        public unsafe void UpdateWithRawPointer(IntPtr data, int byteStride, int height)
        {
            if (height < Height)
            {
                throw new OutOfMemoryException();
            }

            SubresourceData d;
            DeviceContext.Map(_device.ContextPtr, TexturePtr, 0, 4, 0, &d).Check();
            var copyStride = Math.Min(byteStride, d.SysMemPitch);

            for (int i = 0; i < height; ++i)
            {
                System.Buffer.MemoryCopy((byte*)data.ToPointer() + byteStride * i,
                    (byte*)d.pSysMem + d.SysMemPitch * i, d.SysMemPitch, copyStride);
            }

            DeviceContext.Unmap(_device.ContextPtr, TexturePtr, 0);
        }
    }
}
