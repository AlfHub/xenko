﻿#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.VirtualReality
{
    internal class FoveHmd : VRDevice
    {
        private Texture nonSrgbFrame;
        private readonly Matrix referenceMatrix = Matrix.RotationZ(MathUtil.Pi);
        private Matrix referenceMatrixInv;

        private const float HalfIpd = 0.06f;
        private const float EyeHeight = 0.08f;
        private const float EyeForward = -0.04f;

        internal FoveHmd(IServiceRegistry registry) : base(registry)
        {
            referenceMatrixInv = Matrix.RotationZ(MathUtil.Pi);
            referenceMatrixInv.Invert();
        }

        public override void Initialize(GraphicsDevice device, bool depthStencilResource = false, bool requireMirror = false)
        {
            RenderFrame = Texture.New2D(device, RenderFrameSize.Width, RenderFrameSize.Height, PixelFormat.R8G8B8A8_UNorm_SRgb, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
            nonSrgbFrame = Texture.New2D(device, RenderFrameSize.Width, RenderFrameSize.Height, PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
            if (requireMirror)
            {
                MirrorTexture = RenderFrame; //assign the surface we submit as mirror if needed
            }

//            var compositor = (SceneGraphicsCompositorLayers)Game.SceneSystem.SceneInstance.Scene.Settings.GraphicsCompositor;
//            compositor.Master.Add(new SceneDelegateRenderer((x, y) =>
//            {
//                x.CommandList.Copy(RenderFrameProvider.RenderFrame.RenderTargets[0], nonSrgbFrame);
//                //send to hmd
//                var bounds0 = new Vector4(0.0f, 1.0f, 0.5f, 0.0f);
//                var bounds1 = new Vector4(0.5f, 1.0f, 1.0f, 0.0f);
//                if (!Fove.Submit(nonSrgbFrame.NativeResource.NativePointer, ref bounds0, 0) ||
//                    !Fove.Submit(nonSrgbFrame.NativeResource.NativePointer, ref bounds1, 1))
//                {
//                    //failed...
//                }
//
//                Fove.Commit();
//            }));

            base.Initialize(device, requireMirror);
        }

        public override void ReadEyeParameters(Eyes eye, float near, float far, ref Vector3 cameraPosition, ref Matrix cameraRotation, out Matrix view, out Matrix projection)
        {
            throw new System.NotImplementedException();
        }

        public override void Commit(CommandList commandList)
        {
            throw new System.NotImplementedException();
        }

//        public override void Draw(GameTime gameTime)
//        {
//            var properties = new Fove.FrameProperties
//            {
//                Near = LeftCameraComponent.NearClipPlane,
//                Far = LeftCameraComponent.FarClipPlane
//            };
//            Fove.PrepareRender(ref properties);
//
//            properties.ProjLeft.Transpose();
//            properties.ProjRight.Transpose();
//
//            Vector3 scale, camPos;
//            Quaternion camRot;
//
//            //have to make sure it's updated now
//            CameraRootEntity.Transform.UpdateWorldMatrix();
//            CameraRootEntity.Transform.WorldMatrix.Decompose(out scale, out camRot, out camPos);
//
//            LeftCameraComponent.ProjectionMatrix = properties.ProjLeft;
//
//            var pos = camPos + (new Vector3(-HalfIpd * 0.5f, EyeHeight, EyeForward) * ViewScaling);
//            var posV = pos + Vector3.Transform(properties.Pos * ViewScaling, camRot);
//            var rotV = referenceMatrix * Matrix.RotationQuaternion(properties.Rot) * referenceMatrixInv * Matrix.Scaling(ViewScaling) * Matrix.RotationQuaternion(camRot);
//            var finalUp = Vector3.TransformCoordinate(new Vector3(0, 1, 0), rotV);
//            var finalForward = Vector3.TransformCoordinate(new Vector3(0, 0, -1), rotV);
//            var view = Matrix.LookAtRH(posV, posV + finalForward, finalUp);
//            LeftCameraComponent.ViewMatrix = view;
//
//            RightCameraComponent.ProjectionMatrix = properties.ProjRight;
//
//            pos = camPos + (new Vector3(HalfIpd * 0.5f, EyeHeight, EyeForward) * ViewScaling);
//            posV = pos + Vector3.Transform(properties.Pos * ViewScaling, camRot);
//            rotV = referenceMatrix * Matrix.RotationQuaternion(properties.Rot) * referenceMatrixInv * Matrix.Scaling(ViewScaling) * Matrix.RotationQuaternion(camRot);
//            finalUp = Vector3.TransformCoordinate(new Vector3(0, 1, 0), rotV);
//            finalForward = Vector3.TransformCoordinate(new Vector3(0, 0, -1), rotV);
//            view = Matrix.LookAtRH(posV, posV + finalForward, finalUp);
//            RightCameraComponent.ViewMatrix = view;
//
//            base.Draw(gameTime);
//        }

        public override Size2 OptimalRenderFrameSize => new Size2(2560, 1440);

        public override Texture RenderFrame { get; protected set; }
        public override Texture RenderFrameDepthStencil { get; protected set; }

        public override Texture MirrorTexture { get; protected set; }

        public override float RenderFrameScaling { get; set; } = 1.2f;

        public override DeviceState State => DeviceState.Valid;
        public override Vector3 HeadPosition { get; }


        public override Quaternion HeadRotation { get; }

        public override Vector3 HeadLinearVelocity { get; }

        public override Vector3 HeadAngularVelocity { get; }
//to improve and verify with Fove api.

        public override bool CanInitialize => Fove.Startup() && Fove.IsHardwareReady();
    }
}

#endif