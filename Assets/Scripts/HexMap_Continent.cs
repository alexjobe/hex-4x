using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap_Continent : HexMap {

    override public void GenerateMap()
    {
        // First call the base version to make all the hexes
        base.GenerateMap();


        // Elevate area
        int numContinents = 2;
        int continentSpacing = numColumns / numContinents;

        for(int c = 0; c < numContinents; c++)
        {
            int numSplats = Random.Range(4, 8);
            for (int i = 0; i < numSplats; i++)
            {
                int range = Random.Range(5, 8);
                int y = Random.Range(range, numRows - range);
                int x = Random.Range(0, 10) - y / 2 + (c*continentSpacing);

                ElevateArea(x, y, range);
            }
        }

        // Add Perlin noise
        float noiseResolution = 0.1f;
        Vector2 noiseOffset = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
        float noiseScale = 2f;
        for (int column = 0; column < numColumns; column++)
        {
            for (int row = 0; row < numRows; row++)
            {
                Hex h = GetHexAt(column, row);
                float n = Mathf.PerlinNoise(
                    ((float)column / Mathf.Max(numColumns, numRows) / noiseResolution) + noiseOffset.x, 
                    ((float)row / Mathf.Max(numColumns, numRows) / noiseResolution) + noiseOffset.y)
                    - 0.5f;
                h.elevation += n * noiseScale;
            }
        }

        // Update hex visuals to match the data
        UpdateHexVisuals();
        
    }

    void ElevateArea(int q, int r, int range, float centerHeight = 0.5f)
    {
        Hex centerHex = GetHexAt(q, r);

        Hex[] areaHexes = GetHexesWithinRangeOf(centerHex, range);

        foreach(Hex h in areaHexes)
        {
            if (h.elevation < 0)
                h.elevation = 0;
            h.elevation += centerHeight * Mathf.Lerp(1f, 0.25f, Hex.Distance(centerHex, h) / range);
        }
    }
}
