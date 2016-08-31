﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11 || SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D12

using SharpDX;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
using SharpDX.Direct3D11;
#elif SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D12
using SharpDX.Direct3D12;
#endif

namespace SiliconStudio.Xenko.Graphics
{

    public static class SharpDXInterop
    {
        /// <summary>
        /// Gets the native device (DX11/DX12)
        /// </summary>
        /// <param name="device">The Xenko GraphicsDevice</param>
        /// <returns></returns>
        public static object GetNativeDevice(GraphicsDevice device)
        {
            return GetNativeDeviceImpl(device);
        }

        /// <summary>
        /// Gets the native command queue (DX12 only)
        /// </summary>
        /// <param name="device">The Xenko GraphicsDevice</param>
        /// <returns></returns>
        public static object GetNativeCommandQueue(GraphicsDevice device)
        {
            return GetNativeCommandQueueImpl(device);
        }

        /// <summary>
        /// Gets the DX11 native resource handle
        /// </summary>
        /// <param name="resource">The Xenko GraphicsResourceBase</param>
        /// <returns></returns>
        public static object GetNativeResource(GraphicsResource resource)
        {
            return GetNativeResourceImpl(resource);
        }

        /// <summary>
        /// Creates a Texture from a DirectX11 native texture
        /// This method internally will call AddReference on the dxTexture2D texture.
        /// </summary>
        /// <param name="device">The GraphicsDevice in use</param>
        /// <param name="dxTexture2D">The DX11 texture</param>
        /// <param name="takeOwnership">If false AddRef will be called on the texture, if true will not, effectively taking ownership</param>
        /// <returns></returns>
        public static Texture CreateTextureFromNative(GraphicsDevice device, object dxTexture2D, bool takeOwnership)
        {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
            return CreateTextureFromNativeImpl(device, (Texture2D)dxTexture2D, takeOwnership);
#elif SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D12
            return CreateTextureFromNativeImpl(device, (Resource)dxTexture2D, takeOwnership);
#endif
        }

#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
        /// <summary>
        /// Gets the DX11 native device
        /// </summary>
        /// <param name="device">The Xenko GraphicsDevice</param>
        /// <returns></returns>
        private static Device GetNativeDeviceImpl(GraphicsDevice device)
        {
            return device.NativeDevice;
        }

        private static object GetNativeCommandQueueImpl(GraphicsDevice device)
        {
            return null;
        }

        /// <summary>
        /// Gets the DX11 native resource handle
        /// </summary>
        /// <param name="resource">The Xenko GraphicsResourceBase</param>
        /// <returns></returns>
        private static Resource GetNativeResourceImpl(GraphicsResource resource)
        {
            return resource.NativeResource;
        }

        /// <summary>
        /// Creates a Texture from a DirectX11 native texture
        /// This method internally will call AddReference on the dxTexture2D texture.
        /// </summary>
        /// <param name="device">The GraphicsDevice in use</param>
        /// <param name="dxTexture2D">The DX11 texture</param>
        /// <param name="takeOwnership">If false AddRef will be called on the texture, if true will not, effectively taking ownership</param>
        /// <returns></returns>
        private static Texture CreateTextureFromNativeImpl(GraphicsDevice device, Texture2D dxTexture2D, bool takeOwnership)
        {
            var tex = new Texture(device);

            if (takeOwnership)
            {
                var unknown = dxTexture2D as IUnknown;
                unknown.AddReference();
            }

            tex.InitializeFrom(dxTexture2D, false);

            return tex;
        }

#elif SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D12
        /// <summary>
        /// Gets the DX11 native device
        /// </summary>
        /// <param name="device">The Xenko GraphicsDevice</param>
        /// <returns></returns>
        private static Device GetNativeDeviceImpl(GraphicsDevice device)
        {
            return device.NativeDevice;
        }

        private static CommandQueue GetNativeCommandQueueImpl(GraphicsDevice device)
        {
            return device.NativeCommandQueue;
        }

        /// <summary>
        /// Gets the DX11 native resource handle
        /// </summary>
        /// <param name="resource">The Xenko GraphicsResourceBase</param>
        /// <returns></returns>
        private static Resource GetNativeResourceImpl(GraphicsResource resource)
        {
            return resource.NativeResource;
        }

        /// <summary>
        /// Creates a Texture from a DirectX11 native texture
        /// This method internally will call AddReference on the dxTexture2D texture.
        /// </summary>
        /// <param name="device">The GraphicsDevice in use</param>
        /// <param name="dxTexture2D">The DX11 texture</param>
        /// <param name="takeOwnership">If false AddRef will be called on the texture, if true will not, effectively taking ownership</param>
        /// <returns></returns>
        private static Texture CreateTextureFromNativeImpl(GraphicsDevice device, Resource dxTexture2D, bool takeOwnership)
        {
            var tex = new Texture(device);

            if (takeOwnership)
            {
                var unknown = dxTexture2D as IUnknown;
                unknown.AddReference();
            }

            tex.InitializeFrom(dxTexture2D, false);

            return tex;
        }
#endif
    }

}

#endif
