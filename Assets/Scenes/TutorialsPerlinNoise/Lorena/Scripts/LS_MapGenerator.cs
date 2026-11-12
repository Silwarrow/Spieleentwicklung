using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

public class LS_MapGenerator : MonoBehaviour
{

    public enum DrawMode
    {
        NoiseMap,
        ColorMap,
        Mesh
    }

   public DrawMode drawMode;

   public const int mapChunkSize = 241;
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

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<LS.MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<LS.MeshData>>();


    //Recherchire was threading ist und die funktionen

    public void RequestMapData(Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(callback);
        };
        new Thread(threadStart).Start();
    }

    void MapDataThread(Action<MapData> callback) {
        MapData mapData = GenerateMapData();
        //die lock schaut das wenn ein thread diese queue erreicht wärend die queue läuft kann kein anderer threat kann es auch greifen.Es muss erst warten
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }


    public void RequestMeshData(MapData mapData, Action<LS.MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, callback);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, Action<LS.MeshData> callback)
    {
        LS.MeshData meshData = LS_MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<LS.MeshData>(callback, meshData));
        }
    }



    private void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo <MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<LS.MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

    }




    MapData GenerateMapData()
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
        return new MapData(noiseMap, colorMap);
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData();

        LS_MapDisplay display = FindAnyObjectByType<LS_MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(LS_TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(LS_TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(LS_MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), LS_TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
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

    struct MapThreadInfo<T> //generic damit es Mapdata und meshdata übernehmen kan
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo (Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }

    }



}
//End of MapGenerator Ckass

//name space weil sonst clashed mit anderer struct von basti
namespace LS
{
    [System.Serializable]

    //hier nicht readonly weil sonst sieht man nicht in inspektor
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color color;
    }
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }

}
