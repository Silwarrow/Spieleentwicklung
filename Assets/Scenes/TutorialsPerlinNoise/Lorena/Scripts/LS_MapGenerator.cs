using UnityEngine;

public class LS_MapGenerator : MonoBehaviour
{
   public int mapWidth;
   public int mapHeight;
   public float noiseScale;

   public bool autoUpdate;
   public void GenerateMap()
    {
        float[,] noiseMap = LS_Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale); //vergiss nicht class namen zu ändern! Aufruf vom noise genertor

        LS_MapDisplay display = FindAnyObjectByType<LS_MapDisplay>();
        display.DrawNoiseMap (noiseMap);
    }
}
