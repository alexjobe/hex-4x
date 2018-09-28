using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMouseController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    bool isDraggingCamera = false;
    Vector3 lastMousePosition;
	
	// Update is called once per frame
	void Update () {

        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Where does the mouse ray intersect y = 0 ?
        if (mouseRay.direction.y >= 0)
        {
            Debug.Log("Mouse should not be pointing up");
            return;
        }
        float rayLength = (mouseRay.origin.y / mouseRay.direction.y);
        Vector3 hitPos = mouseRay.origin - (mouseRay.direction * rayLength);

        if (Input.GetMouseButtonDown(0))
        {
            isDraggingCamera = true;
            lastMousePosition = hitPos;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDraggingCamera = false;
        }

        if (isDraggingCamera)
        {
            Vector3 diff = lastMousePosition - hitPos;
            Camera.main.transform.Translate(diff, Space.World);
            mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (mouseRay.direction.y >= 0)
            {
                Debug.Log("Mouse should not be pointing up");
                return;
            }
            rayLength = (mouseRay.origin.y / mouseRay.direction.y);
            lastMousePosition = mouseRay.origin - (mouseRay.direction * rayLength);
        }

        // Zoom to scroll wheel
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        if(Mathf.Abs(scrollAmount) > 0.01f)
        {
            // Move camera towards hitPos
            Vector3 dir = hitPos - Camera.main.transform.position;

            Vector3 p = Camera.main.transform.position;
            Debug.Log("Amount: " + scrollAmount);

            if(Mathf.Abs(scrollAmount) > 0 && p.y > 2 && p.y < 20)
            {
                Camera.main.transform.Translate(dir * scrollAmount, Space.World);
            }
            else if(scrollAmount > 0 && p.y >= 20)
            {
                Camera.main.transform.Translate(dir * scrollAmount, Space.World);
            }
            else if (scrollAmount < 0 && p.y <= 2)
            {
                Camera.main.transform.Translate(dir * scrollAmount, Space.World);
            }
        }
	}
}
