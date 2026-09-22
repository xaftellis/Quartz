using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace Quartz.Controls
{
    // The small DirectComposition/D3D11 bridge used by the button renderer. No
    // HWNDs or input are moved between threads. DWM runs committed animations.
    // ABI order follows Microsoft's windows-rs 0.58 DirectComposition bindings;
    // native C++ overloads have animation overloads BEFORE scalar overloads.
    internal static class DirectComposition
    {
        [ThreadStatic] private static DeviceContext _current;

        internal static DeviceContext Acquire()
        {
            if (_current == null) _current = new DeviceContext();
            _current.Users++;
            return _current;
        }

        internal static void Release(DeviceContext context)
        {
            if (--context.Users != 0) return;
            if (ReferenceEquals(_current, context)) _current = null;
            context.Dispose();
        }

        internal static void Release(object value)
        {
            if (value == null || !Marshal.IsComObject(value)) return;
            try { Marshal.ReleaseComObject(value); }
            catch (InvalidComObjectException) { } // An invalidated wrapper has no remaining reference to release.
        }

        // A failed RCW QueryInterface arrives as InvalidCastException, rather
        // than COMException. Treat only this COM HRESULT as an interface failure.
        internal static bool IsInterfaceFailure(Exception error) => error is InvalidComObjectException ||
            (error is InvalidCastException && error.HResult == unchecked((int)0x80004002));

        internal sealed class DeviceContext : IDisposable
        {
            internal IDevice Device;
            internal int Users;
            private IntPtr _d3d, _immediate;
            private UpdateSubresource _upload;
            private readonly Dictionary<long, ISurface> _textures = new Dictionary<long, ISurface>();

            internal DeviceContext()
            {
                try
                {
                    int level;
                    int hr = D3D11CreateDevice(IntPtr.Zero, 1, IntPtr.Zero, 0x20, IntPtr.Zero,
                        0, 7, out _d3d, out level, out _immediate);
                    if (hr < 0)
                    {
                        if (_immediate != IntPtr.Zero) Marshal.Release(_immediate);
                        if (_d3d != IntPtr.Zero) Marshal.Release(_d3d);
                        _immediate = _d3d = IntPtr.Zero;
                        Marshal.ThrowExceptionForHR(D3D11CreateDevice(IntPtr.Zero, 5, IntPtr.Zero,
                            0x20, IntPtr.Zero, 0, 7, out _d3d, out level, out _immediate));
                    }
                    Guid iid = new Guid("54ec77fa-1377-44e6-8c32-88fd5f44c84c"); // IDXGIDevice
                    IntPtr dxgi;
                    Marshal.ThrowExceptionForHR(Marshal.QueryInterface(_d3d, ref iid, out dxgi));
                    try
                    {
                        iid = typeof(IDevice).GUID;
                        Marshal.ThrowExceptionForHR(DCompositionCreateDevice(dxgi, ref iid, out Device));
                    }
                    finally { Marshal.Release(dxgi); }
                    // ID3D11DeviceContext::UpdateSubresource, including IUnknown
                    // and ID3D11DeviceChild's four preceding methods (d3d11.h).
                    IntPtr method = Marshal.ReadIntPtr(Marshal.ReadIntPtr(_immediate), 48 * IntPtr.Size);
                    _upload = (UpdateSubresource)Marshal.GetDelegateForFunctionPointer(method, typeof(UpdateSubresource));
                }
                catch { Dispose(); throw; }
            }

            internal ISurface CreateSurface(SKBitmap bitmap)
            {
                ISurface surface;
                Device.CreateSurface((uint)bitmap.Width, (uint)bitmap.Height, 87, 1, out surface);
                try { Upload(surface, bitmap); return surface; }
                catch { Release(surface); throw; }
            }

            internal void Upload(ISurface surface, SKBitmap bitmap)
            {
                Guid iid = new Guid("cafcb56c-6ac3-4889-bf47-9e23bbd260ec"); // IDXGISurface
                IntPtr resource;
                NativePoint offset;
                surface.BeginDraw(IntPtr.Zero, ref iid, out resource, out offset);
                try
                {
                    iid = new Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c"); // ID3D11Texture2D
                    IntPtr texture;
                    Marshal.ThrowExceptionForHR(Marshal.QueryInterface(resource, ref iid, out texture));
                    try
                    {
                        var box = new Box { Left = (uint)offset.X, Top = (uint)offset.Y,
                            Right = (uint)(offset.X + bitmap.Width), Bottom = (uint)(offset.Y + bitmap.Height), Back = 1 };
                        _upload(_immediate, texture, 0, ref box, bitmap.GetPixels(), (uint)bitmap.RowBytes, 0);
                    }
                    finally { Marshal.Release(texture); }
                }
                finally { Marshal.Release(resource); surface.EndDraw(); }
            }

            internal ISurface Texture(Color color, bool circle)
            {
                long key = ((long)(uint)color.ToArgb() << 1) | (circle ? 1L : 0L);
                ISurface surface;
                if (_textures.TryGetValue(key, out surface)) return surface;
                // Visuals retain their own COM reference, so old cached entries
                // can be released without disturbing an already displayed button.
                if (_textures.Count >= 32)
                {
                    foreach (var item in _textures.Values) Release(item);
                    _textures.Clear();
                }
                int size = circle ? 256 : 1;
                using (var pixels = new SKBitmap(size, size, SKColorType.Bgra8888, SKAlphaType.Premul))
                using (var canvas = new SKCanvas(pixels))
                using (var paint = new SKPaint { IsAntialias = true, Color = new SKColor(color.R, color.G, color.B, color.A) })
                {
                    canvas.Clear(SKColors.Transparent);
                    if (circle) canvas.DrawCircle(128, 128, 127, paint);
                    else canvas.Clear(paint.Color);
                    canvas.Flush();
                    surface = CreateSurface(pixels);
                }
                _textures.Add(key, surface);
                return surface;
            }

            public void Dispose()
            {
                foreach (var surface in _textures.Values) Release(surface);
                _textures.Clear();
                Release(Device); Device = null;
                if (_immediate != IntPtr.Zero) Marshal.Release(_immediate);
                if (_d3d != IntPtr.Zero) Marshal.Release(_d3d);
                _immediate = _d3d = IntPtr.Zero;
            }
        }

        [StructLayout(LayoutKind.Sequential)] internal struct NativePoint { internal int X, Y; }
        [StructLayout(LayoutKind.Sequential)] internal struct Box { internal uint Left, Top, Front, Right, Bottom, Back; }
        [StructLayout(LayoutKind.Sequential)] internal struct Matrix { internal float M11, M12, M21, M22, X, Y; }
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void UpdateSubresource(IntPtr context, IntPtr resource, uint subresource,
            ref Box box, IntPtr data, uint rowPitch, uint depthPitch);

        [DllImport("d3d11.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int D3D11CreateDevice(IntPtr adapter, int driverType, IntPtr software,
            uint flags, IntPtr levels, uint levelCount, uint sdkVersion, out IntPtr device,
            out int featureLevel, out IntPtr immediateContext);
        [DllImport("dcomp.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int DCompositionCreateDevice(IntPtr dxgi, ref Guid iid, out IDevice device);

        [ComImport, Guid("C37EA93A-E7AA-450D-B16F-9746CB0407F3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IDevice
        {
            void Commit();
            void WaitForCommitCompletion();
            void GetFrameStatistics(IntPtr statistics);
            void CreateTargetForHwnd(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool topmost, out ITarget target);
            void CreateVisual(out IVisual visual);
            void CreateSurface(uint width, uint height, int format, int alpha, out ISurface surface);
            void CreateVirtualSurface(uint width, uint height, int format, int alpha, out IntPtr surface);
            void CreateSurfaceFromHandle(IntPtr handle, out IntPtr surface);
            void CreateSurfaceFromHwnd(IntPtr hwnd, out IntPtr surface);
            void CreateTranslateTransform(out IntPtr transform);
            void CreateScaleTransform(out IScale transform);
            void CreateRotateTransform(out IntPtr transform);
            void CreateSkewTransform(out IntPtr transform);
            void CreateMatrixTransform(out IntPtr transform);
            void CreateTransformGroup(IntPtr transforms, uint count, out IntPtr transform);
            void CreateTranslateTransform3D(out IntPtr transform);
            void CreateScaleTransform3D(out IntPtr transform);
            void CreateRotateTransform3D(out IntPtr transform);
            void CreateMatrixTransform3D(out IntPtr transform);
            void CreateTransform3DGroup(IntPtr transforms, uint count, out IntPtr transform);
            void CreateEffectGroup(out IEffect effect);
            void CreateRectangleClip(out IClip clip);
            void CreateAnimation(out IAnimation animation);
            void CheckDeviceState([MarshalAs(UnmanagedType.Bool)] out bool valid);
        }

        [ComImport, Guid("eacdd04c-117e-4e17-88f4-d1b12b0e3d89"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ITarget { void SetRoot(IVisual visual); }

        [ComImport, Guid("4d93059d-097b-4651-9a60-f0f25116e2f3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IVisual
        {
            void AnimateOffsetX(IAnimation animation); void SetOffsetX(float value);
            void AnimateOffsetY(IAnimation animation); void SetOffsetY(float value);
            void SetTransform([MarshalAs(UnmanagedType.IUnknown)] object transform); void SetMatrix(ref Matrix matrix);
            void SetTransformParent(IVisual visual);
            void SetEffect(IEffect effect);
            void SetBitmapInterpolationMode(int mode);
            void SetBorderMode(int mode);
            void SetClip(IClip clip); void SetRectangleClip(IntPtr rect);
            void SetContent([MarshalAs(UnmanagedType.IUnknown)] object content);
            void AddVisual(IVisual visual, [MarshalAs(UnmanagedType.Bool)] bool above, IVisual reference);
            void RemoveVisual(IVisual visual); void RemoveAllVisuals();
            void SetCompositeMode(int mode);
        }

        [ComImport, Guid("71FDE914-40EF-45ef-BD51-68B037C339F9"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IScale
        {
            void AnimateScaleX(IAnimation animation); void SetScaleX(float value);
            void AnimateScaleY(IAnimation animation); void SetScaleY(float value);
            void AnimateCenterX(IAnimation animation); void SetCenterX(float value);
            void AnimateCenterY(IAnimation animation); void SetCenterY(float value);
        }

        [ComImport, Guid("A7929A74-E6B2-4bd6-8B95-4040119CA34D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IEffect
        {
            void AnimateOpacity(IAnimation animation); void SetOpacity(float value);
            void SetTransform3D(IntPtr transform);
        }

        [ComImport, Guid("9842AD7D-D9CF-4908-AED7-48B51DA5E7C2"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IClip
        {
            void AnimateLeft(IAnimation animation); void SetLeft(float value);
            void AnimateTop(IAnimation animation); void SetTop(float value);
            void AnimateRight(IAnimation animation); void SetRight(float value);
            void AnimateBottom(IAnimation animation); void SetBottom(float value);
            void AnimateTopLeftRadiusX(IAnimation animation); void SetTopLeftRadiusX(float value);
            void AnimateTopLeftRadiusY(IAnimation animation); void SetTopLeftRadiusY(float value);
            void AnimateTopRightRadiusX(IAnimation animation); void SetTopRightRadiusX(float value);
            void AnimateTopRightRadiusY(IAnimation animation); void SetTopRightRadiusY(float value);
            void AnimateBottomLeftRadiusX(IAnimation animation); void SetBottomLeftRadiusX(float value);
            void AnimateBottomLeftRadiusY(IAnimation animation); void SetBottomLeftRadiusY(float value);
            void AnimateBottomRightRadiusX(IAnimation animation); void SetBottomRightRadiusX(float value);
            void AnimateBottomRightRadiusY(IAnimation animation); void SetBottomRightRadiusY(float value);
        }

        [ComImport, Guid("cbfd91d9-51b2-45e4-b3de-d19ccfb863c5"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAnimation
        {
            void Reset(); void SetAbsoluteBeginTime(long time);
            void AddCubic(double begin, float constant, float linear, float quadratic, float cubic);
            void AddSinusoidal(double begin, float bias, float amplitude, float frequency, float phase);
            void AddRepeat(double begin, double duration); void End(double end, float value);
        }

        [ComImport, Guid("BB8A4953-2C99-4F5A-96F5-4819027FA3AC"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ISurface
        {
            void BeginDraw(IntPtr updateRect, ref Guid iid, out IntPtr updateObject, out NativePoint offset);
            void EndDraw(); void SuspendDraw(); void ResumeDraw();
            void Scroll(IntPtr scrollRect, IntPtr clipRect, int x, int y);
        }
    }
}
