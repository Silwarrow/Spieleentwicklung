using UnityEngine;

public class LS_MapGenerator : MonoBehaviour
{
   public int mapWidth;
   public int mapHeight;
   public float noiseScale;

    public int octaves;
    [Range(0f, 1f)] public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

   public bool autoUpdate;
   public void GenerateMap()
    {
        float[,] noiseMap = LS_Noise.GenerateNoiseMap(mapWidth, mapHeight, seed,  noiseScale, octaves, persistance,lacunarity, offset); //vergiss nicht class namen zu ändern! Aufruf vom noise genertor

        LS_MapDisplay display = FindAnyObjectByType<LS_MapDisplay>();
        display.DrawNoiseMap (noiseMap);
    }

    private void OnValidate() //clamping für den inspector, wird aufgerufen immer wenn sich ein value ändert
    {
        if (mapWidth < 1)
        {
            mapWidth = 1;
        }
        if (mapHeight < 1)
        {
            mapHeight = 1;
        }
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
    }
}
