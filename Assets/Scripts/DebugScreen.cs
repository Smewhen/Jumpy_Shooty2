using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour {
    World world;
    Text onScreenText;

    float frameRate;
    float timer;

    int halfWorldSizeInVoxels;
    int halfWorldSizeInChunks;

    void Start () {
        world = GameObject.Find ("World").GetComponent<World>();
        onScreenText = GetComponent<Text> ();

        halfWorldSizeInChunks = VoxelData.WorldSizeInChunk / 2;
        halfWorldSizeInVoxels = VoxelData.WorldSizeInVoxels / 2;
    }

    void Update () {
        string debugText = "Prototype Shooty Jumpy Blocks mk1";
        debugText += "\n";
        debugText += frameRate + " fps";
        debugText += "\n\n";
        debugText += "X,Y,Z: ";
        debugText += (Mathf.FloorToInt (world.player.transform.position.x) - halfWorldSizeInVoxels) + " / ";
        debugText += Mathf.FloorToInt (world.player.transform.position.y) + " / ";
        debugText += (Mathf.FloorToInt (world.player.transform.position.z) - halfWorldSizeInVoxels);
        debugText += "\n";
        debugText += "Chunk " + (world.playerChunkCoord.x - halfWorldSizeInChunks) + " / " + (world.playerChunkCoord.z - halfWorldSizeInChunks);

        onScreenText.text = debugText;

        if (timer > 1f) {
            frameRate = (int) (1f / Time.unscaledDeltaTime);
            timer = 0;
        } else 
            timer += Time.deltaTime;
    }
}
