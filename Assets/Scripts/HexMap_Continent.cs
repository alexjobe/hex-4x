using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap_Continent : HexMap {

    override public void GenerateMap()
    {
        // First call the base version to make all the hexes
        base.GenerateMap();

        // Elevate area
        ElevateArea(53, 14, 4);

        // Update hex visuals to match the data
        UpdateHexVisuals();
        
    }

    void ElevateArea(int q, int r, int range)
    {
        Hex centerHex = GetHexAt(q, r);

        Hex[] areaHexes = GetHexesWithinRangeOf(centerHex, range);

        foreach(Hex h in areaHexes)
        {
            h.elevation = 0.5f;
        }
    }
}
