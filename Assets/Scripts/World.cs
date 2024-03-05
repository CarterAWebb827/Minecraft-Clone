using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {
    public static int seed;
    public BiomeAttribute biomes;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public BlockType[] blockTypes;

    Chunk[,] chunks;

    List<ChunkCoord> activeChunks;
    ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    private void Start() {
        chunks = new Chunk[VoxelData.worldSizeInChunks, VoxelData.worldSizeInChunks];
        activeChunks = new List<ChunkCoord>();

        Random.InitState(seed);

        spawnPosition = new Vector3((VoxelData.worldSizeInChunks * VoxelData.chunkWidth) / 2f, VoxelData.chunkHeight + 2f, (VoxelData.worldSizeInChunks * VoxelData.chunkWidth) / 2f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }

    private void Update() {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);

        // Only update the chunks if the player has moved from the chunk they were previously on.
        if (!playerChunkCoord.Equals(playerLastChunkCoord)) {
            CheckViewDistance();
        }
    }

    void GenerateWorld() {
        for (int x = (VoxelData.worldSizeInChunks / 2) - VoxelData.viewDistanceInChunks; x < (VoxelData.worldSizeInChunks / 2) + VoxelData.viewDistanceInChunks; x++) {
            for (int z = (VoxelData.worldSizeInChunks / 2) - VoxelData.viewDistanceInChunks; z < (VoxelData.worldSizeInChunks / 2) + VoxelData.viewDistanceInChunks; z++) {
                CreateNewChunk(x, z);
            }
        }

        player.position = spawnPosition;
    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos) {
        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.chunkWidth);
        return new ChunkCoord(x, z);
    }

    void CheckViewDistance() {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        // Loop through all chunks currently within view distance of the player.
        for (int x = coord.x - VoxelData.viewDistanceInChunks; x < coord.x + VoxelData.viewDistanceInChunks; x++) {
            for (int z = coord.z - VoxelData.viewDistanceInChunks; z < coord.z + VoxelData.viewDistanceInChunks; z++) {
                // If the current chunk is in the world...
                if (IsChunkInWorld(new ChunkCoord(x, z))) {
                    // Check if it active, if not, activate it.
                    if (chunks[x, z] == null) {
                        CreateNewChunk(x, z);
                    } else if (!chunks[x, z].isActive) {
                        chunks[x, z].isActive = true;
                        activeChunks.Add(new ChunkCoord(x, z));
                    }
                }

                // Check through previously active chunks to see if this chunk is there. If it is, remove it from the list.
                for (int i = 0; i < previouslyActiveChunks.Count; i++) {
                    if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z))) {
                        previouslyActiveChunks.RemoveAt(i);
                    }
                }
            }
        }

        // Any chunks left in the previousActiveChunks list are no longer in the player's view distance, so loop through and disable them.
        foreach (ChunkCoord c in previouslyActiveChunks) {
            chunks[c.x, c.z].isActive = false;
        }
    }

    public byte GetVoxel(Vector3 pos) {
        int yPos = Mathf.FloorToInt(pos.y);

        /* ===== IMMUTABLE PASS ===== */
        // If outside world, return air.
        if (!IsVoxelInWorld(pos)) {
            return 0;
        }
        // If bottom block of chunk, return bedrock.
        if (yPos == 0) {
            return 1;
        }

        /* ===== BASIC TERRAIN PASS ===== */
        int terrainHeight = Mathf.FloorToInt(biomes.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biomes.terrainScale)) + biomes.solidGroundHeight;
        byte voxelValue = 0;

        if (yPos == terrainHeight) {
            voxelValue = 3; // Grass
        } else if (yPos < terrainHeight && yPos > terrainHeight - 4) {
            voxelValue = 5; // Dirt
        } else if (yPos > terrainHeight) {
            return 0; // Air
        } else {
            voxelValue = 2; // Stone
        }

        /* ===== SECOND PASS ===== */
        if (voxelValue == 2) {
            foreach (Lode lode in biomes.lodes) {
                if (yPos > lode.minHeight && yPos < lode.maxHeight) {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold)) {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }

        return voxelValue;
    }

    void CreateNewChunk(int x, int z) {
        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
        activeChunks.Add(new ChunkCoord(x, z));
    }

    bool IsChunkInWorld(ChunkCoord coord) {
        if (coord.x > 0 && coord.x < VoxelData.worldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.worldSizeInChunks - 1) {
            return true;
        } else {
            return false;
        }
    }

    bool IsVoxelInWorld(Vector3 pos) {
        if (pos.x >= 0 && pos.x < VoxelData.worldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.chunkHeight && pos.z >= 0 && pos.z < VoxelData.worldSizeInVoxels) {
            return true;
        } else {
            return false;
        }
    }
}

[System.Serializable]
public class BlockType {
    public string blockName;
    public bool isSolid;

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    // Back, Front, Top, Bottom, Left, Right
    public int GetTextureID(int faceIndex) {
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
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;
        }
    }
}