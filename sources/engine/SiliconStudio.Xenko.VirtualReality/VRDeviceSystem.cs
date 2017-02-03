﻿using System;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.VirtualReality
{
    public class VRDeviceSystem : GameSystemBase
    {
        public VRDeviceSystem(IServiceRegistry registry) : base(registry)
        {
            registry.AddService(typeof(VRDeviceSystem), this);
            EnabledChanged += OnEnabledChanged;

            DrawOrder = -100;
            UpdateOrder = -100;
        }

        public VRApi[] PreferredApis;

        public VRDevice Device { get; private set; }

        public bool DepthStencilAsResource;

        public bool RequireMirror;

        public MSAALevel MSAALevel = MSAALevel.None;

        private void OnEnabledChanged(object sender, EventArgs eventArgs)
        {
            if (Enabled && Device == null)
            {
                if (PreferredApis == null)
                {
                    return;
                }

                foreach (var hmdApi in PreferredApis)
                {
                    switch (hmdApi)
                    {
                        case VRApi.Oculus:
                        {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
                            Device = new OculusOvrHmd();
                                
#endif
                        }
                            break;
                        case VRApi.OpenVR:
                        {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
                            Device = new OpenVRHmd();
#endif
                        }
                            break;
                        case VRApi.Fove:
                        {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
                            Device = new FoveHmd();
#endif
                        }
                            break;
                        case VRApi.Google:
                        {
#if SILICONSTUDIO_PLATFORM_IOS || SILICONSTUDIO_PLATFORM_ANDROID
                                VRDevice = new GoogleVrHmd();
#endif
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (Device != null)
                    {
                        Device.Game = Game;

                        if (Device != null && !Device.CanInitialize)
                        {
                            Device.Dispose();
                            Device = null;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                var deviceManager = (GraphicsDeviceManager)Services.GetService(typeof(IGraphicsDeviceManager));
                Device?.Enable(GraphicsDevice, deviceManager, DepthStencilAsResource, RequireMirror, MSAALevel);
            }
        }

        public override void Update(GameTime gameTime)
        {
            Device?.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Device?.Draw(gameTime);
        }
    }
}