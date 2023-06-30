using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainGenerator : MonoBehaviour
{
    public int width = 256;
    public int height = 256;
    public int depth = 20;

    public float scale = 20f;

    public float offX = 10f;
    public float offY = 10f;
    
        
    private void Start()
    {
        offX = Random.Range(0f, 9999f); 
        offY = Random.Range(0f, 9999f);
        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenTerrain(terrain.terrainData);
        
    }
    

    TerrainData GenTerrain(TerrainData tData)
    {
        tData.heightmapResolution = width + 1;

        tData.size = new Vector3(width, depth, height);
        
        tData.SetHeights(0,0,GenHeights());

        return tData;
    }

    float[,] GenHeights ()
    {
        float[,] heights = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }

        return heights;
    }

    float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / width * scale + offX;
        float yCoord = (float)y / height * scale + offY;

        return Mathf.PerlinNoise(xCoord, yCoord);

    }
    
}
