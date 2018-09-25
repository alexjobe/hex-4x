using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotion : MonoBehaviour {

    Vector3 oldPosition;
    HexComponent[] hexes;

    // Use this for initialization
    void Start () {
        oldPosition = this.transform.position;
	}
	
	// Update is called once per frame
	void Update () {

        // TODO: Click to drag camera
        //       WASD
        //       Zoom in and out


        CheckIfCameraMoved();
	}

    public void PanToHex(Hex hex)
    {
        // TODO: Move camera to hex
    }

    private void CheckIfCameraMoved()
    {
        if(oldPosition != this.transform.position)
        {
            oldPosition = this.transform.position;

            // TODO: Create dictionary in HexMap
            if (hexes == null)
                hexes = GameObject.FindObjectsOfType<HexComponent>();

            foreach(HexComponent hex in hexes)
            {
                hex.UpdatePosition();
            }
        }
    }
}
