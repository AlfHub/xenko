// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

#pragma once
#include "../../../deps/Recast/include/DetourNavMesh.h"
#include "../../../deps/Recast/include/DetourNavMeshQuery.h"
#include "../../../deps/NativePath/TINYSTL/unordered_set.h"

#pragma pack(4)
struct NavMeshPathfindQuery
{
	Vector3 source;
	Vector3 target;
	Vector3 findNearestPolyExtent;
	int maxPathPoints;
};
struct NavMeshPathfindResult
{
	bool pathFound = false;
	Vector3* pathPoints = nullptr;
	int numPathPoints = 0;
};

struct NavMeshRaycastQuery
{
	Vector3 start;
	Vector3 end;
	Vector3 findNearestPolyExtent;
	int maxPathPoints;
};
struct NavMeshRaycastResult
{
	bool hit = false;
	Vector3 position;
	Vector3 normal;
};

class NavigationMesh
{
private:
	dtNavMesh* m_navMesh = nullptr;
	dtNavMeshQuery* m_navQuery = nullptr;
	tinystl::unordered_set<dtTileRef> m_tileRefs;
public:
	NavigationMesh();
	~NavigationMesh();
	bool Init(float cellTileSize);
	bool LoadTile(uint8_t* navData, int navDataLength);
	bool RemoveTile(Point tileCoordinate);
	void FindPath(NavMeshPathfindQuery query, NavMeshPathfindResult* result);
	void Raycast(NavMeshRaycastQuery query, NavMeshRaycastResult* result);
};
