using UnityEngine;

public static class LS_Noise
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        if (scale <= 0){  //damit kein division durch 0 error falls 0 eingegeben wird
            scale = 0.0001f;
        }

        for (int y=0; y<mapHeight; y++)   //looo through the map
        {
            for (int x=0; x<mapWidth; x++)
            {
                float sampleX = x / scale;  //das unser yoff wie coding train
                float sampleY = y / scale;  // das unser xoff

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseMap[x, y] = perlinValue;
            }
        }
        return noiseMap;
    }


}
