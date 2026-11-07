using UnityEngine;

public class LS_MapDisplay : MonoBehaviour
{
    public Renderer textureRender;

    public void DrawNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0); //for first dimension
        int height = noiseMap.GetLength(1); //second dimension

        Texture2D texture = new Texture2D(width, height);

        //fast method -> man erstell ein array von den werten und gibt sie als array and texture anstatt jeder wert einzelnt
        Color[] colorMap = new Color[width * height]; //unser color array ist 1dimensionnal aber unsere karte 2 dimensionao
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]); //mapping der farbe

            }
        }
        texture.SetPixels(colorMap);
        texture.Apply();

        //diese funktion erlaubt uns auch die updates an unserer map auch im preview zu sehen statt nur in game
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(width, 1, height);
    }
}
