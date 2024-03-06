using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour {
    public World world;
    private TMPro.TextMeshProUGUI text;

    private float frameRate;
    private float timer;

    private int halfWorldSizeInVoxels;
    private int halfWorldSizeInChunks;

    private void Start() {
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<TMPro.TextMeshProUGUI>();

        halfWorldSizeInVoxels = VoxelData.worldSizeInVoxels / 2;
        halfWorldSizeInChunks = VoxelData.worldSizeInChunks / 2;
    }

    private void Update() {
        string debugText = "Player Position (X / Y / Z ): " + (Mathf.FloorToInt(world.player.position.x) - halfWorldSizeInVoxels) + " / " + Mathf.FloorToInt(world.player.position.y) + " / " + (Mathf.FloorToInt(world.player.position.z) - halfWorldSizeInVoxels)+ "\n";
        
        if (timer > 1f) {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        timer += Time.unscaledDeltaTime;
        debugText += "FPS: " + frameRate;

        debugText += "\nChunk Coord: " + (world.playerChunkCoord.x - halfWorldSizeInChunks) + ", " + (world.playerChunkCoord.z - halfWorldSizeInChunks) + "\n";
        //debugText += "Biome: " + world.biomes.GetBiome(world.playerChunkCoord).biomeName + "\n";
        //debugText += "Active Chunks: " + world.activeChunks.Count + "\n";
        debugText += "Seed: " + World.seed + "\n";

        text.text = debugText;
    }
}
