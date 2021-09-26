using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {
    public int seed;
    public BiomeAttributes biome;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public BlockType[] blocktypes;

    Chunk [,] chunks = new Chunk [VoxelData.WorldSizeInChunk, VoxelData.WorldSizeInChunk];

    List<ChunkCoordinate> activeChunks = new List<ChunkCoordinate> ();
    public ChunkCoordinate playerChunkCoord;
    ChunkCoordinate playerLastChunkCoord;

    List<ChunkCoordinate> chunksToCreate = new List<ChunkCoordinate> ();
    private bool isCreatingChunks;

    public GameObject debugScreen;

    private void Start () {
        Random.InitState (seed);

        spawnPosition = new Vector3 ((VoxelData.WorldSizeInChunk * VoxelData.ChunkSize) / 2f, VoxelData.ChunkHeight - 50f, (VoxelData.WorldSizeInChunk * VoxelData.ChunkSize) / 2f);
        GenerateWorld ();
        playerLastChunkCoord = GetChunkCoordinateFromVector3 (player.position);
    }

    private void Update () {
        playerChunkCoord = GetChunkCoordinateFromVector3 (player.position);

        if (!playerChunkCoord.Equals (playerLastChunkCoord)) {
            CheckViewDistance ();
        }

        if (chunksToCreate.Count > 0 && !isCreatingChunks)
            StartCoroutine ("CreateChunks");

        if (Input.GetKeyDown (KeyCode.F3))
            debugScreen.SetActive (!debugScreen.activeSelf);
    }

    void GenerateWorld () {
        for (int x = (VoxelData.WorldSizeInChunk / 2) - VoxelData.viewDistance; x < (VoxelData.WorldSizeInChunk / 2) + VoxelData.viewDistance; x++) {
            for (int z = (VoxelData.WorldSizeInChunk / 2) - VoxelData.viewDistance; z < (VoxelData.WorldSizeInChunk / 2) + VoxelData.viewDistance; z++) {
                chunks [x, z] = new Chunk (new ChunkCoordinate (x, z), this, true);
                activeChunks.Add (new ChunkCoordinate (x, z));
            }
        }

        player.position = spawnPosition;
    }

    IEnumerator CreateChunks () {
        isCreatingChunks = true;

        while (chunksToCreate.Count > 0) {
            chunks [chunksToCreate [0].x, chunksToCreate [0].z].Init();
            chunksToCreate.RemoveAt (0);

            yield return null;
        }

        isCreatingChunks = false;
    }

    ChunkCoordinate GetChunkCoordinateFromVector3 (Vector3 pos) {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkSize);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkSize);

        return new ChunkCoordinate (x, z);
    }

    public Chunk GetChunkFromVector3 (Vector3 pos) {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkSize);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkSize);

        return chunks [x, z];
    }

    void CheckViewDistance () {
        ChunkCoordinate coord = GetChunkCoordinateFromVector3 (player.position);
        playerLastChunkCoord = playerChunkCoord;

        List<ChunkCoordinate> previouslyActiveChunks = new List<ChunkCoordinate> (activeChunks);

        for (int x = coord.x - VoxelData.viewDistance; x < coord.x + VoxelData.viewDistance; x++) {
            for (int z = coord.z - VoxelData.viewDistance; z < coord.z + VoxelData.viewDistance; z++) {
                if (IsChunkInWorld (new ChunkCoordinate (x, z))) {
                    if (chunks [x, z] == null) {
                        chunks [x,z] = new Chunk (new ChunkCoordinate (x,z), this, false);
                        chunksToCreate.Add (new ChunkCoordinate (x, z));
                    } else if (!chunks [x, z].IsActive) {
                        chunks [x, z].IsActive = true;
                    }
                    activeChunks.Add (new ChunkCoordinate (x, z));
                }

                for (int i = 0; i < previouslyActiveChunks.Count; i++) {
                    if (previouslyActiveChunks[i].Equals (new ChunkCoordinate (x, z))) {
                        previouslyActiveChunks.RemoveAt (i);                    
                    }
                }
            }
        }
        foreach (ChunkCoordinate c in previouslyActiveChunks) {
            chunks [c.x, c.z].IsActive = false;
        }
    }

    public bool CheckForVoxel (Vector3 pos) {
        ChunkCoordinate thisChunk = new ChunkCoordinate (pos);

        if (!IsChunkInWorld (thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight) 
            return false;

        if (chunks [thisChunk.x, thisChunk.z] != null && chunks [thisChunk.x, thisChunk.z].isVoxelMapPopulated)
            return blocktypes [chunks [thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3 (pos)].isSolid;

        return blocktypes [GetVoxel (pos)].isSolid;
    }

    public byte GetVoxel (Vector3 pos) {
        int yPos = Mathf.FloorToInt (pos.y);

        /* If block is outside world, return air (0) */
        if (!IsVoxelInWorld (pos))
            return 0;
        /* If block is bottom of world, return bedrock (1)*/
        if (yPos == 0)
            return 1;

        int terrainHeight = Mathf.FloorToInt (biome.highestTerrainHeight * Noise.Get2dPerlin (new Vector2 (pos.x, pos.z), 0, biome.terrainScale)) + biome.lowestTerrainHeight;
        byte voxelValue = 0; 

        if (yPos == terrainHeight)
            voxelValue = 3;
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
            voxelValue = 5;
        else if (yPos > terrainHeight)
            return 0;
        else
            voxelValue = 2;

        if (voxelValue == 2) {
            foreach (Lode lode in biome.lodes) {
                if (yPos > lode.minHeight && yPos < lode.maxHeight) {
                    if (Noise.Get3dPerlin (pos, lode.offset, lode.scale, lode.threshold))
                        voxelValue = lode.blockID;
                }
            }
        }
        return voxelValue;
    }   

    bool IsChunkInWorld (ChunkCoordinate coord) {
        if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunk - 1 && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunk - 1)
            return true;
        else
            return false;
    }

    bool IsVoxelInWorld (Vector3 position) {
        if (position.x >= 0 && position.x < VoxelData.WorldSizeInVoxels && position.y >= 0 && position.y < VoxelData.ChunkHeight && position.z >= 0 && position.z < VoxelData.WorldSizeInVoxels )
            return true;
        else
            return false;
    }

}

[System.Serializable]
public class BlockType {
    public string blockName;
    public bool isSolid;

    [Header ("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    // Back, Front, Top, Bottom, Left, Right
    public int GetTextureId (int faceIndex) {
        switch (faceIndex) {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log ("Error in GetTextureId; invalid face index");
                return 0;
        }
    }
}