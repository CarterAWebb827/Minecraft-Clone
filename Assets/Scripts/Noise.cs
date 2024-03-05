using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {
    private static PerlinNoise perlin = new PerlinNoise(World.seed);
    
    public static float Get2DPerlin(Vector2 pos, float offset, float scale) {
        float x = (pos.x + 0.1f) / VoxelData.chunkWidth * scale + offset;
        float y = (pos.y + 0.1f) / VoxelData.chunkWidth * scale + offset;

        // Use the Noise method of the PerlinNoise instance
        // Use a constant value for the z parameter
        return perlin.Noise(x, y, 0f);

        //return Mathf.PerlinNoise((pos.x + 0.1f) / VoxelData.chunkWidth * scale + offset, (pos.y + 0.1f) / VoxelData.chunkWidth * scale + offset);
    }

    public static bool Get3DPerlin(Vector3 pos, float offset, float scale, float threshold) {
        /*
        // Create an instance of PerlinNoise with a seed value
        PerlinNoise perlin = new PerlinNoise(12345);

        float x = (pos.x + 0.1f) / VoxelData.chunkWidth * scale + offset;
        float y = (pos.y + 0.1f) / VoxelData.chunkHeight * scale + offset;
        float z = (pos.z + 0.1f) / VoxelData.chunkWidth * scale + offset;

        // Use the Noise method of the PerlinNoise instance instead of Mathf.PerlinNoise
        return perlin.Noise(x, y, z) > threshold;
        */

        
        float x = (pos.x + offset + 0.1f) * scale;
        float y = (pos.y + offset + 0.1f) * scale;
        float z = (pos.z + offset + 0.1f) * scale;

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        if ((AB + BC + AC + BA + CB + CA) / 6f > threshold) {
            return true;
        } else {
            return false;
        }
        
    }
}
