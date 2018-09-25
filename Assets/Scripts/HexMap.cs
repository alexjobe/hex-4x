using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap : MonoBehaviour {

    public GameObject hexPrefab;
    public Material[] hexMaterials;

    public int numRows = 20;
    public int numColumns = 40;

    // Use this for initialization
    void Start () {
        GenerateMap();
	}

    public void GenerateMap()
    {
        for(int column = 0; column < numColumns; column++)
        {
            for (int row = 0; row < numRows; row++)
            {
                Hex h = new Hex(column, row);

                Vector3 pos = h.PositionFromCamera(
                    Camera.main.transform.position,
                    numRows,
                    numColumns
                );

                GameObject hexGO = (GameObject)Instantiate(
                    hexPrefab, 
                    pos,
                    Quaternion.identity,
                    this.transform
                );

                hexGO.GetComponent<HexComponent>().hex = h;
                hexGO.GetComponent<HexComponent>().hexMap = this;

                MeshRenderer mr = hexGO.GetComponentInChildren<MeshRenderer>();
                mr.material = hexMaterials[Random.Range(0, hexMaterials.Length)];
            }
        }

        //StaticBatchingUtility.Combine(this.gameObject);
    }
}
