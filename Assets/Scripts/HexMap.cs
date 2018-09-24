using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap : MonoBehaviour {

    public GameObject hexPrefab;

	// Use this for initialization
	void Start () {
        GenerateMap();
	}

    public void GenerateMap()
    {
        for(int column = 0; column < 10; column++)
        {
            for (int row = 0; row < 10; row++)
            {
                Hex h = new Hex(column, row);

                Instantiate(
                    hexPrefab, 
                    h.Position(),
                    Quaternion.identity,
                    this.transform
                );
            }
        }
    }
}
