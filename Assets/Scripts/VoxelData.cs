using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData {

    public static readonly int ChunkSize = 16;
    public static readonly int ChunkHeight = 128;
    public static readonly int WorldSizeInChunk = 100;

    public static int WorldSizeInVoxels {
        get { return WorldSizeInChunk * ChunkSize; }
    }

    public static readonly int viewDistance = 5;

    public static readonly int TextureAtlasSizeInBlocks = 4;
    public static float NormalizedBlockTextureSize {
        
        get { return 1f / (float)TextureAtlasSizeInBlocks; }
    }

    public static readonly Vector3[] voxelVertices = 
    {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f)
    };

    public static readonly Vector3[] faceChecks = 
    {
        new Vector3(0.0f, 0.0f, -1.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, -1.0f, 0.0f),
        new Vector3(-1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f)
    };

    public static readonly int[,] voxelTriangles = 
    {
        // Back, Front, Top, Bottom, Left, Right
        // 0 1 2 2 1 3
         {0, 3, 1, 2}, // Back Face
         {5, 6, 4, 7}, // Front Face
         {3, 7, 2, 6}, // Top Face
         {1, 5, 0, 4}, // Bottom Face
         {4, 7, 0, 3}, // Left Face
         {1, 2, 5, 6}  // Right Face
    };

    public static readonly Vector2[] voxelUvs =
    {
        new Vector2 (0.0f, 0.0f),
        new Vector2 (0.0f, 1.0f),
        new Vector2 (1.0f, 0.0f),
        new Vector2 (1.0f, 1.0f)
    };
}
