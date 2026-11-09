using UnityEngine;
using System.Collections;
using UnityEditor.AssetImporters;

public class LS_MapGenerator : MonoBehaviour
{

    public enum DrawMode
    {
        NoiseMap,
        ColorMap,
        Mesh
    }

   public DrawMode drawMode;

   const int mapChunkSize = 241;
    [Range(0,6)]
    public int levelOfDetail;
   public float noiseScale;

    public int octaves;
    [Range(0f, 1f)] public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve; //lässt uns die gravity anpassen wie stark welche level effected werden

   public bool autoUpdate;

    public LS.TerrainType[] regions;

    public void GenerateMap()
    {
        float[,] noiseMap = LS_Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset); //vergiss nicht class namen zu ändern! Aufruf vom noise genertor

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];



        //looping to apply color
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                //checking which region this height falls under
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }

        }


        LS_MapDisplay display = FindAnyObjectByType<LS_MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(LS_TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(LS_TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(LS_MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), LS_TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        }
    }

    private void OnValidate() //clamping für den inspector, wird aufgerufen immer wenn sich ein value ändert
    {
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


//name space weil sonst clashed mit anderer struct von basti
namespace LS
{
    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color color;
    }
}
