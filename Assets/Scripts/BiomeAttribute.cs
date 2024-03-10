using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "ScriptableObjects/Biome Attributes")]
public class BiomeAttribute : ScriptableObject {
    public string biomeName;
    public int solidGroundHeight;
    public int terrainHeight;
    public float terrainScale;

    [Header("Trees")]
    public float treeZoneScale = 1.3f;
    [Range(0.1f, 1f)]
    public float treeZoneThreshold = 0.25f;
    public float treePlacementScale = 11f;
    [Range(0.1f, 1f)]
    public float treePlacementThreshold = 0.45f;

    public int maxTreeHeight = 12;
    public int minTreeHeight = 5;

    public Lode[] lodes;
}

// System.Serializable attribute indicates that this class can be serialized by Unity and saved to a file
[System.Serializable]
// This class is used to store the attributes of a lode (a vein of ore)
public class Lode {
    public string lodeName;
    public byte blockID;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float noiseOffset;
}