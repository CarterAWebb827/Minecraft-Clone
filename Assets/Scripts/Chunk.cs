using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    public void Start() {
        int vertexIndex = 0;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int i = 0; i < 6; i++) {
            for (int j = 0; j < 6; j ++) {
                int triangleIndex = VoxelData.voxelTris[i, j];
                vertices.Add(VoxelData.voxelVerts[triangleIndex]);
                triangles.Add(vertexIndex);

                uvs.Add(VoxelData.voxelUvs[j]);

                vertexIndex++;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals(); // This is important for lighting to work correctly

        meshFilter.mesh = mesh;
    }
}
