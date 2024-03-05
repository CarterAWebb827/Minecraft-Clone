using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    int vertexIndex;
    List<Vector3> vertices;
    List<int> triangles;
    List<Vector2> uvs;

    bool[,,] voxelMap;

    public void Start() {
        vertexIndex = 0;
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();

        voxelMap = new bool[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];

        PopulateVoxelMap();

        CreateMeshData();

        CreateMesh();
    }

    private void PopulateVoxelMap() {
        for (int y = 0; y < VoxelData.chunkHeight; y++) {
            for (int x = 0; x < VoxelData.chunkWidth; x++) {
                for (int z = 0; z < VoxelData.chunkWidth; z++) {
                    voxelMap[x, y, z] = true;
                }
            }
        }
    }

    private void CreateMeshData() {
        for (int y = 0; y < VoxelData.chunkHeight; y++) {
            for (int x = 0; x < VoxelData.chunkWidth; x++) {
                for (int z = 0; z < VoxelData.chunkWidth; z++) {
                    AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    private bool CheckVoxel(Vector3 pos) {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (x < 0 || x > VoxelData.chunkWidth - 1 || y < 0 || y > VoxelData.chunkHeight - 1 || z < 0 || z > VoxelData.chunkWidth - 1) {
            return false;
        }

        return voxelMap[x, y, z];
    }

    private void AddVoxelDataToChunk( Vector3 position ) {
        for (int i = 0; i < 6; i++) {
            if (!CheckVoxel(position + VoxelData.faceChecks[i])) {
                vertices.Add(position + VoxelData.voxelVerts[VoxelData.voxelTris[i, 0]]);
                vertices.Add(position + VoxelData.voxelVerts[VoxelData.voxelTris[i, 1]]);
                vertices.Add(position + VoxelData.voxelVerts[VoxelData.voxelTris[i, 2]]);
                vertices.Add(position + VoxelData.voxelVerts[VoxelData.voxelTris[i, 3]]);

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);

                uvs.Add(VoxelData.voxelUvs[0]);
                uvs.Add(VoxelData.voxelUvs[1]);
                uvs.Add(VoxelData.voxelUvs[2]);
                uvs.Add(VoxelData.voxelUvs[3]);

                vertexIndex += 4;
            }
        }
    }

    private void CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals(); // This is important for lighting to work correctly

        meshFilter.mesh = mesh;
    }
}
