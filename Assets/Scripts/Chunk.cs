using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {
    public ChunkCoord coord;

    private GameObject chunkObject;
	private MeshRenderer meshRenderer;
	private MeshFilter meshFilter;

	private int vertexIndex;
	private List<Vector3> vertices;
	private List<int> triangles;
	private List<Vector2> uvs;

	public byte[,,] voxelMap;

    private World world;

    private bool _isActive;

    public bool isVoxelMapPopulated = false;

	public Chunk(ChunkCoord _cord, World _world, bool generateOnLoad) {
        coord = _cord;
        world = _world;
        isActive = true;
        
        if(generateOnLoad) {
            Init();
        }
    }

    public void Init () {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.chunkWidth, 0f, coord.z * VoxelData.chunkWidth);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.z;

        vertexIndex = 0;
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();

        voxelMap = new byte[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];

        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();
    }

	private void PopulateVoxelMap() {
		for(int y = 0; y < VoxelData.chunkHeight; y++) {
			for(int x = 0; x < VoxelData.chunkWidth; x++) {
				for(int z = 0; z < VoxelData.chunkWidth; z++) {
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + position);
				}
			}
		}

        isVoxelMapPopulated = true;
	}

	private void CreateMeshData() {
		for(int y = 0; y < VoxelData.chunkHeight; y++) {
			for(int x = 0; x < VoxelData.chunkWidth; x++) {
				for(int z = 0; z < VoxelData.chunkWidth; z++) {
                    if(world.blockTypes[voxelMap[x, y, z]].isSolid) {
                        AddVoxelDataToChunk(new Vector3(x, y, z));
                    }
				}
			}
		}
	}

    public bool isActive {
        get { return _isActive; }
        set { 
            _isActive = value;
            if(chunkObject != null) {
                chunkObject.SetActive(value);
            }
        }
    }

    public Vector3 position {
        get { return chunkObject.transform.position; }
    }

    private bool IsVoxelInChunk(int x, int y, int z) {
        if(x < 0 || x > VoxelData.chunkWidth - 1 || y < 0 || y > VoxelData.chunkHeight - 1 || z < 0 || z > VoxelData.chunkWidth - 1) {
            return false;
        }

        return true;
    }

	private bool CheckVoxel(Vector3 pos) {
		int x = Mathf.FloorToInt(pos.x);
		int y = Mathf.FloorToInt(pos.y);
		int z = Mathf.FloorToInt(pos.z);

		if(!IsVoxelInChunk(x, y, z)) {
			return world.CheckForVoxel(pos + position);
        }

		return world.blockTypes[voxelMap [x, y, z]].isSolid;
	}

    public byte GetVoxelFromGlobalVector3(Vector3 pos) {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(position.x);
        zCheck -= Mathf.FloorToInt(position.z);

        return voxelMap[xCheck, yCheck, zCheck];
    }

	private void AddVoxelDataToChunk(Vector3 pos) {
		for(int p = 0; p < 6; p++) { 
			if(!CheckVoxel(pos + VoxelData.faceChecks[p])) {
                byte blockID = voxelMap[(int)pos.x,(int)pos.y,(int)pos.z];

				vertices.Add(pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 0]]);
				vertices.Add(pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 1]]);
				vertices.Add(pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 2]]);
				vertices.Add(pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 3]]);

                AddTexture(world.blockTypes[blockID].GetTextureID(p));

				triangles.Add(vertexIndex);
				triangles.Add(vertexIndex + 1);
				triangles.Add(vertexIndex + 2);
				triangles.Add(vertexIndex + 2);
				triangles.Add(vertexIndex + 1);
				triangles.Add(vertexIndex + 3);
				vertexIndex += 4;
			}
		}
	}

	private void CreateMesh() {
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();

		mesh.RecalculateNormals();

		meshFilter.mesh = mesh;
	}

    private void AddTexture(int textureID) {
        float y = textureID / VoxelData.textureAtlasSizeInBlocks;
        float x = textureID -(y * VoxelData.textureAtlasSizeInBlocks);

        x *= VoxelData.normalizedBlockTextureSize;
        y *= VoxelData.normalizedBlockTextureSize;

        y = 1f - y - VoxelData.normalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.normalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.normalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.normalizedBlockTextureSize, y + VoxelData.normalizedBlockTextureSize));
    }
}

public class ChunkCoord {
    public int x;
    public int z;

    public ChunkCoord() {
        x = 0;
        z = 0;
    }

    public ChunkCoord(Vector3 pos) {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VoxelData.chunkWidth;
        z = zCheck / VoxelData.chunkWidth;
    }

    public ChunkCoord(int _x, int _z) {
        x = _x;
        z = _z;
    }

    public bool Equals(ChunkCoord other) {
        if(other == null) {
            return false;
        } else if(other.x == x && other.z == z) {
            return true;
        } else {
            return false;
        }
    }
}