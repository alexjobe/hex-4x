using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitView : MonoBehaviour {

    Vector3 newPosition;

    Vector3 currentVelocity;
    float smoothTime = 0.5f;

    private void Start()
    {
        newPosition = this.transform.position;
    }

    public void OnUnitMoved(Hex oldHex, Hex newHex)
    {
        // This GameObject is meant to be a child of the hex we are standing
        // in. This ensures that we are in the correct place in the hierarchy.
        // The correct position when not moving is to be at 0,0 local position
        // relative to the parent
        this.transform.position = oldHex.PositionFromCamera();
        newPosition = newHex.PositionFromCamera();
        currentVelocity = Vector3.zero;

        // Unit should only move one tile at a time. If distance > 2,
        // we've reached the edge, so just teleport
        if(Vector3.Distance(this.transform.position, newPosition) > 2)
        {
            this.transform.position = newPosition;
        }
        else
        {
            // TODO: We need a better signalling system and/or animation queueing
            GameObject.FindObjectOfType<HexMap>().AnimationIsPlaying = true;
        }
    }

    private void Update()
    {
        this.transform.position = Vector3.SmoothDamp(this.transform.position, 
            newPosition, ref currentVelocity, smoothTime);

        // TODO: Determine the best way to handle the end of our animation
        if(Vector3.Distance(this.transform.position, newPosition) < 0.1f)
        {
            GameObject.FindObjectOfType<HexMap>().AnimationIsPlaying = false; 
        }
    }
}
