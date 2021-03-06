using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {
    public ChunkCoordinate coord;

    GameObject chunkObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3> ();
    List<int> triangles = new List<int> ();
    List<Vector2> uvs = new List<Vector2> ();

    public byte [,,] voxelMap = new byte [VoxelData.ChunkSize, VoxelData.ChunkHeight, VoxelData.ChunkSize];

    World world;

    private bool _isActive;
    public bool isVoxelMapPopulated = false;

    public Chunk (ChunkCoordinate _coord, World _world, bool generateOnLoad)  {
        coord = _coord;
        world = _world;
        IsActive = true;

        if (generateOnLoad) {
            Init();
        }
    }

    public void Init () {
        chunkObject = new GameObject ();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
 
        meshRenderer.material = world.material;
        chunkObject.transform.SetParent (world.transform);
        chunkObject.transform.position = new Vector3 (coord.x * VoxelData.ChunkSize, 0f, coord.z * VoxelData.ChunkSize);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.z;

        PopulateVoxelMap ();
        UpdateChunk ();        
    }

    void PopulateVoxelMap () {
        for(int y = 0; y < VoxelData.ChunkHeight; y++) {
            for(int x = 0; x < VoxelData.ChunkSize; x++) {
                for(int z = 0; z < VoxelData.ChunkSize; z++) {
                    voxelMap [x, y, z] = world.GetVoxel(new Vector3 (x, y, z) + position);
                }
            }
        }
        isVoxelMapPopulated = true;
    }

    void UpdateChunk () {
        ClearMeshData ();

        for(int y = 0; y < VoxelData.ChunkHeight; y++) {
            for(int x = 0; x < VoxelData.ChunkSize; x++) {
                for(int z = 0; z < VoxelData.ChunkSize; z++) {
                    if (world.blocktypes [voxelMap [x, y, z]].isSolid)
                        UpdateMeshData (new Vector3 (x, y, z));
                }
            }
        }

        CreateMesh ();
    }

    void ClearMeshData () {
        vertexIndex = 0;
        vertices.Clear ();
        triangles.Clear ();
        uvs.Clear ();
    }

    public bool IsActive  {
        get { return _isActive; }
        set { 
            _isActive = value;
            if (chunkObject != null)
                chunkObject.SetActive (value);
        }
    }

    public Vector3 position {
        get { return chunkObject.transform.position; }
    }

    bool IsVoxelInChunk (int x, int y, int z) {
        if (x < 0 || x > VoxelData.ChunkSize - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkSize - 1)
            return false;
        else
            return true;
    }

    public void EditVoxel (Vector3 pos, byte newBlockId) {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt (chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt (chunkObject.transform.position.z);

        voxelMap [xCheck, yCheck, zCheck] = newBlockId;

        UpdateSurroundingVoxels (xCheck, yCheck, zCheck);

        UpdateChunk ();
    }

    void UpdateSurroundingVoxels (int x, int y, int z) {
        Vector3 thisVoxel = new Vector3 (x, y, z);

        for (int p = 0; p < 6; p++) {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks [p];

            if (!IsVoxelInChunk ((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z)) {
                world.GetChunkFromVector3 (currentVoxel + position).UpdateChunk ();
            }
        }
    }

    bool CheckVoxel (Vector3 pos) {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk (x, y, z))
            return world.CheckForVoxel (pos + position);
            
        return world.blocktypes [voxelMap [x, y, z]].isSolid;
    }

    public byte GetVoxelFromGlobalVector3 (Vector3 pos) {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt (chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt (chunkObject.transform.position.z);

        return voxelMap [xCheck, yCheck, zCheck];
    }

    void UpdateMeshData (Vector3 position) {
        for (int p = 0; p < 6; p++) {
            if (!CheckVoxel (position + VoxelData.faceChecks [p])) {
                for (int i = 0; i < 4; i++) {
                    int triangleIndex = VoxelData.voxelTriangles [p, i];
                    vertices.Add (VoxelData.voxelVertices [triangleIndex] + position);
                }
                byte blockid = voxelMap [(int)position.x, (int)position.y, (int)position.z];

                AddTexture (world.blocktypes [blockid].GetTextureId (p));
                triangles.Add (vertexIndex);
                triangles.Add (vertexIndex + 1);
                triangles.Add (vertexIndex + 2);
                triangles.Add (vertexIndex + 2);
                triangles.Add (vertexIndex + 1);
                triangles.Add (vertexIndex + 3);
                vertexIndex += 4;
            }
        }
    }

    void CreateMesh () {
        Mesh mesh = new Mesh {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            uv = uvs.ToArray()
        };

        mesh.RecalculateNormals ();
        meshFilter.mesh = mesh;
    }

    void AddTexture (int textureId) {
        float y = textureId / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureId - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add (new Vector2 (x, y));
        uvs.Add (new Vector2 (x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add (new Vector2 (x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add (new Vector2 (x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }
}

public class ChunkCoordinate {
    public int x;
    public int z;

    public ChunkCoordinate () {
        x = 0;
        z = 0;
    }

    public ChunkCoordinate (int _x, int _z) {
        x = _x;
        z = _z;
    }

    public ChunkCoordinate (Vector3 pos) {
        int xCheck = Mathf.FloorToInt (pos.x);
        int zCheck = Mathf.FloorToInt (pos.z);

        x = xCheck / VoxelData.ChunkSize;
        z = zCheck / VoxelData.ChunkSize;
    }

    public bool Equals (ChunkCoordinate other) {
        if (other == null) {
            return false;
        } else if (other.x == x && other.z == z) {
            return true;
        } else
            return false;
    }
}
