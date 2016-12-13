﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if defined(WINDOWS_DESKTOP) || !defined(__clang__)

#include "../../../../deps/NativePath/NativePath.h"
#include "../../../../deps/NativePath/NativeDynamicLinking.h"
#include "../../../../deps/Fove/IFVRCompositor.h"
#include "../../../../deps/Fove/IFVRHeadset.h"
#include "../../SiliconStudio.Xenko.Native/XenkoNative.h"

extern "C" 
{
	void* __libFove = NULL;

	static Fove::IFVRHeadset* sHeadSet = NULL;
	static Fove::IFVRCompositor* sCompositor = NULL;

	typedef Fove::IFVRHeadset* (*GetFVRHeadsetPtr)();
	typedef Fove::IFVRCompositor* (*CGetFVRCompositorPtr)();

	DLL_EXPORT_API npBool xnFoveStartup()
	{
		__libFove = LoadDynamicLibrary("FoveClient");
		if (!__libFove) __libFove = LoadDynamicLibrary("x86\\FoveClient");
		if (!__libFove) __libFove = LoadDynamicLibrary("x64\\FoveClient");
		if (!__libFove) __libFove = LoadDynamicLibrary("x64/FoveClient");
		if (!__libFove) __libFove = LoadDynamicLibrary("x64/FoveClient");
		if (!__libFove)
		{
			return false;
		}

		GetFVRHeadsetPtr get_fvr_headset_ptr = (GetFVRHeadsetPtr)GetSymbolAddress(__libFove, "CGetFVRHeadset");
		if (!get_fvr_headset_ptr) return false;
		sHeadSet = get_fvr_headset_ptr();

		CGetFVRCompositorPtr get_fvr_compositor_ptr = (CGetFVRCompositorPtr)GetSymbolAddress(__libFove, "CGetFVRCompositor");
		if (!get_fvr_compositor_ptr) return false;
		sCompositor = get_fvr_compositor_ptr();

		return sHeadSet != NULL && sHeadSet->Initialise();
	}

	DLL_EXPORT_API void xnFoveShutdown()
	{
		if (sCompositor) sCompositor->Shutdown();
		if (sHeadSet) sHeadSet->Destroy();
		if(__libFove)
		{
			FreeDynamicLibrary(__libFove);
			__libFove = NULL;
		}
	}

	DLL_EXPORT_API npBool xnFoveSubmit(void* texture, float* bounds, int eyeIndex)
	{
		if (!sCompositor) return false;

		Fove::SFVR_TextureBounds fbounds;
		fbounds.left = bounds[0];
		fbounds.bottom = bounds[1];
		fbounds.right = bounds[2];
		fbounds.top = bounds[3];

		return sCompositor->Submit(texture, 
			Fove::EFVR_GraphicsAPI::DirectX, 
				eyeIndex == 0 ? Fove::EFVR_Eye::Left : Fove::EFVR_Eye::Right, 
				fbounds)
		== Fove::EFVR_CompositorError::None;
	}

	DLL_EXPORT_API void xnFoveCommit()
	{
		if (sCompositor)
		{
			sCompositor->WaitForRenderPose();
			//sCompositor->SignalFrameComplete();
		}
	}

	DLL_EXPORT_API npBool xnFoveGetLeftEyePoint(float* point)
	{
		if(sHeadSet)
		{
			auto eyePoint = sHeadSet->GetGazePoint();
			point[0] = eyePoint.coord.x;
			point[1] = eyePoint.coord.y;

			return eyePoint.error == Fove::EFVR_ErrorCode::None;
		}

		return false;
	}

#pragma pack(push, 4)
	struct FrameProperties
	{
		//Camera properties
		float Near;
		float Far;
		float ProjLeft[16];
		float ProjRight[16];
		float Pos[3];
		float Rot[4];
	};
#pragma pack(pop)

	DLL_EXPORT_API void xnFovePrepareRender(FrameProperties* properties)
	{
		auto matrixL = sHeadSet->GetProjectionMatrixRH(Fove::EFVR_Eye::Left, properties->Near, properties->Far);
		auto matrixR = sHeadSet->GetProjectionMatrixRH(Fove::EFVR_Eye::Right, properties->Near, properties->Far);
		memcpy(properties->ProjLeft, &matrixL, sizeof(float) * 16);
		memcpy(properties->ProjRight, &matrixR, sizeof(float) * 16);

		auto pose = sHeadSet->GetHMDPose();
		memcpy(properties->Pos, &pose.position, sizeof(float) * 3);
		memcpy(properties->Rot, &pose.orientation, sizeof(float) * 4);
		properties->Pos[2] = -properties->Pos[2]; //flip Z
	}

	DLL_EXPORT_API void xnFoveRecenter()
	{
		if(sHeadSet)
		{
			sHeadSet->TareOrientationSensor();
			sHeadSet->TarePositionSensors();
		}
	}
}

#else

extern "C"
{
	DLL_EXPORT_API npBool xnFoveStartup()
	{
		return false;
	}

	DLL_EXPORT_API void xnFoveShutdown()
	{
	}

	DLL_EXPORT_API npBool xnFoveSubmit(void* texture, float* bounds, int eyeIndex)
	{
		return false;
	}

	DLL_EXPORT_API void xnFoveCommit()
	{
	}

	DLL_EXPORT_API void xnFovePrepareRender(void* properties)
	{
	}

	DLL_EXPORT_API npBool xnFoveGetLeftEyePoint(float* point)
	{
		return false;
	}

	DLL_EXPORT_API void xnFoveRecenter()
	{
	}
}

#endif