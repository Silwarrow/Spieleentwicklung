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
        Mesh,
        FalloffMap
    }
   public DrawMode drawMode;

    public LS_Noise.NormalizeMode normalizeMode;



   public const int mapChunkSize = 239;
    [Range(0,6)]
    public int editorLevelOfDetail;
   public float noiseScale;

    public int octaves;
    [Range(0f, 1f)] public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public bool useFalloff;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve; //lässt uns die gravity anpassen wie stark welche level effected werden

   public bool autoUpdate;

    public TerrainType[] regions;

    float[,] falloffMap;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();


    private void Awake()
    {
        falloffMap = LS_FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }

    //Recherchire was threading ist und die funktionen

    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(centre, callback);
        };
        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre, Action<MapData> callback) {
        MapData mapData = GenerateMapData(centre);
        //die lock schaut das wenn ein thread diese queue erreicht wärend die queue läuft kann kein anderer threat kann es auch greifen.Es muss erst warten
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }


    public void RequestMeshData(MapData mapData,int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData,lod, callback);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = LS_MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
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
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

    }




    MapData GenerateMapData(Vector2 centre)
    {
        float[,] noiseMap = LS_Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed, noiseScale, octaves, persistance, lacunarity, centre + offset, normalizeMode); //vergiss nicht class namen zu ändern! Aufruf vom noise genertor //+ 2 ist für die bordersize

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];



        //looping to apply color
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                if (useFalloff)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                }

                float currentHeight = noiseMap[x, y];
                //checking which region this height falls under
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight >= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                    } else
                    {
                        break;
                    }
                }
            }

        }
        return new MapData(noiseMap, colorMap);
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);

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
            display.DrawMesh(LS_MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorLevelOfDetail), LS_TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            display.DrawTexture(LS_TextureGenerator.TextureFromHeightMap(LS_FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
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
        falloffMap = LS_FalloffGenerator.GenerateFalloffMap(mapChunkSize);
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
    [System.Serializable]

    //hier nicht readonly weil sonst sieht man nicht in inspektor
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color color;
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
