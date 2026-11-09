using UnityEngine;

public static class LS_TextureGenerator {

    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        //fixing blurryness with filterMode
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        int width = heightMap.GetLength(0); //for first dimension
        int height = heightMap.GetLength(1); //second dimension

        Texture2D texture = new Texture2D(width, height);

        //fast method -> man erstell ein array von den werten und gibt sie als array and texture anstatt jeder wert einzelnt
        Color[] colorMap = new Color[width * height]; //unser color array ist 1dimensionnal aber unsere karte 2 dimensionao
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]); //mapping der farbe

            }
        }
        return TextureFromColorMap(colorMap,width, height);
    }


}
