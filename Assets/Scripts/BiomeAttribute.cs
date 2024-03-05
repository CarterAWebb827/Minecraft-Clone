using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "ScriptableObjects/Biome Attributes")]
public class BiomeAttribute : ScriptableObject {
    public string biomeName;
    public int solidGroundHeight;
    public int terrainHeight;
    public float terrainScale;

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