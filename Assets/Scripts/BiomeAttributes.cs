using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "BiomeAttributes", menuName = "Prototype Shooty Jumpy Blocks/Biome Attribute")]
public class BiomeAttributes : ScriptableObject {
    public string biomeName;

    public int highestTerrainHeight;
    public int lowestTerrainHeight;

    public float terrainScale;

    [Header ("Trees")]
    public float treeZoneScale = 1.3f;

    [Range (0.1f, 1f)]
    public float treeZoneThreshold = 0.6f;
    public float treePlacementScale = 15f;
    public float treePlacementThreshold = 0.8f;

    public int maxTreeHeight = 12;
    public int minTreeHeight = 5;

    public Lode[] lodes;
}

[System.Serializable]
public class Lode {
    public string nodeName;
    public byte blockID;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float offset;
}