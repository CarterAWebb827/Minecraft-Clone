using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure {
    public static Queue<VoxelMod> MakeTree (Vector3 position, int minTrunkHeight, int maxTrunkHeight) {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();

        int trunkHeight = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 750, 3f));
        
        if (trunkHeight < minTrunkHeight) {
            trunkHeight = minTrunkHeight;
        }

        for (int i = 1; i < trunkHeight; i++) {
            queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), 9)); // 9 is the ID for the oak log
        }

        // Cube of leaves on top of the trunk
        for (int x = -3; x < 4; x++) {
            for (int y = 0; y < 7; y++) {
                for (int z = -3; z < 4; z++) {
                    if (x != 0 || y > 3 || z != 0) {
                        queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + trunkHeight + y, position.z + z), 12)); // 12 is the ID for the leaves
                    }
                }
            }
        }

        return queue;
    }
}
