using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "BiomeAttributes", menuName = "Prototype Shooty Jumpy Blocks/Biome Attribute")]
public class BiomeAttributes : ScriptableObject {
    public string biomeName;

    public int highestTerrainHeight;
    public int lowestTerrainHeight;

    public float terrainScale;

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