using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {

    public ChunkCoord coord;

    GameObject chunkObject;
	MeshRenderer meshRenderer;
	MeshFilter meshFilter;

	int vertexIndex = 0;
	List<Vector3> vertices = new List<Vector3> ();
	List<int> triangles = new List<int> ();
    List<int> transparentTriangles = new List<int> ();
    Material[] materials = new Material[2];
	List<Vector2> uvs = new List<Vector2> ();

	public byte[,,] voxelMap = new byte[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];

    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();

    World world;

    private bool _isActive;
    public bool isVoxelMapPopulated = false;

    public Chunk (ChunkCoord _coord, World _world, bool generateOnLoad) {
        coord = _coord;
        world = _world;
        isActive = true;

        if (generateOnLoad) {
            Init();
        }
    }

    public void Init () {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        materials[0] = world.material;
        materials[1] = world.transparentMaterial;
        meshRenderer.materials = materials;
        
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.chunkWidth, 0f, coord.z * VoxelData.chunkWidth);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.z;

        PopulateVoxelMap();
        UpdateChunk();
    }

	private void PopulateVoxelMap () {
		for (int y = 0; y < VoxelData.chunkHeight; y++) {
			for (int x = 0; x < VoxelData.chunkWidth; x++) {
				for (int z = 0; z < VoxelData.chunkWidth; z++) {
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + position);
				}
			}
		}

        isVoxelMapPopulated = true;
	}

	public void UpdateChunk () {
        while (modifications.Count > 0) {
            VoxelMod v = modifications.Dequeue();
            Vector3 pos = v.position -= position;

            voxelMap[(int)pos.x, (int)pos.y, (int)pos.z] = v.id;
        }

        ClearMeshData();

		for (int y = 0; y < VoxelData.chunkHeight; y++) {
			for (int x = 0; x < VoxelData.chunkWidth; x++) {
				for (int z = 0; z < VoxelData.chunkWidth; z++) {
                    if (world.blockTypes[voxelMap[x,y,z]].isSolid) {
					    UpdateMeshData (new Vector3(x, y, z));
                    }    
				}
			}
		}

        CreateMesh();
	}

    private void ClearMeshData () {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        uvs.Clear();
    }

    public bool isActive {
        get { return _isActive; }
        set {
            _isActive = value;
            if (chunkObject != null) {
                chunkObject.SetActive(value);
            }    
        }
    }

    public Vector3 position {
        get { return chunkObject.transform.position; }
    }

    private bool IsVoxelInChunk (int x, int y, int z) {

        if (x < 0 || x > VoxelData.chunkWidth - 1 || y < 0 || y > VoxelData.chunkHeight - 1 || z < 0 || z > VoxelData.chunkWidth - 1) {
            return false;
        } else {
            return true;
        }    
    }

    public void EditVoxel (Vector3 pos, byte newID) {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        voxelMap[xCheck, yCheck, zCheck] = newID;

        UpdateSurroundingVoxels(xCheck, yCheck, zCheck);

        UpdateChunk();
    }

    private void UpdateSurroundingVoxels (int x, int y, int z) {
        Vector3 thisVoxel = new Vector3(x, y, z);

        for (int p = 0; p < 6; p++) {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[p];

            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z)) {
                world.GetChunkFromVector3(currentVoxel + position).UpdateChunk();
            }
        }
    }

	private bool CheckVoxel (Vector3 pos) {
		int x = Mathf.FloorToInt (pos.x);
		int y = Mathf.FloorToInt (pos.y);
		int z = Mathf.FloorToInt (pos.z);

        if (!IsVoxelInChunk(x, y, z)) {
            return world.CheckIfVoxelTransparent(pos + position);
        }
        
		return world.blockTypes[voxelMap [x, y, z]].isTransparent;
	}

    public byte GetVoxelFromGlobalVector3 (Vector3 pos) {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        return voxelMap[xCheck, yCheck, zCheck];
    }

	private void UpdateMeshData (Vector3 pos) {
        byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];
        bool isTransparent = world.blockTypes[blockID].isTransparent;

		for (int p = 0; p < 6; p++) { 
			if (CheckVoxel(pos + VoxelData.faceChecks[p])) {
				vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 0]]);
				vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 1]]);
				vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 2]]);
				vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 3]]);

                AddTexture(world.blockTypes[blockID].GetTextureID(p));

                if (!isTransparent) {
                    triangles.Add (vertexIndex);
                    triangles.Add (vertexIndex + 1);
                    triangles.Add (vertexIndex + 2);
                    triangles.Add (vertexIndex + 2);
                    triangles.Add (vertexIndex + 1);
                    triangles.Add (vertexIndex + 3);
                } else {
                    transparentTriangles.Add (vertexIndex);
                    transparentTriangles.Add (vertexIndex + 1);
                    transparentTriangles.Add (vertexIndex + 2);
                    transparentTriangles.Add (vertexIndex + 2);
                    transparentTriangles.Add (vertexIndex + 1);
                    transparentTriangles.Add (vertexIndex + 3);
                }

                vertexIndex += 4;
			}
		}
	}

	private void CreateMesh () {
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);
		mesh.uv = uvs.ToArray();

		mesh.RecalculateNormals();

		meshFilter.mesh = mesh;
	}

    private void AddTexture (int textureID) {
        float y = textureID / VoxelData.textureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.textureAtlasSizeInBlocks);

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

    public ChunkCoord () {
        x = 0;
        z = 0;
    }

    public ChunkCoord (int _x, int _z) {
        x = _x;
        z = _z;
    }

    public ChunkCoord (Vector3 pos) {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VoxelData.chunkWidth;
        z = zCheck / VoxelData.chunkWidth;
    }

    public bool Equals (ChunkCoord other) {
        if (other == null) {
            return false;
        } else if (other.x == x && other.z == z) {
            return true;
        } else {
            return false;
        }    
    }
}