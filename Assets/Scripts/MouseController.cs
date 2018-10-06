using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Update_CurrentFunc = Update_DetectModeStart;

        hexMap = GameObject.FindObjectOfType<HexMap>();
        lineRenderer = transform.GetComponentInChildren<LineRenderer>();
    }

    // Generic bookkeeping variables
    Vector3 lastMousePosition; // From Input.mousePosition
    HexMap hexMap;
    Hex hexUnderMouse;
    Hex hexLastUnderMouse;

    // Camera dragging bookkeeping variables
    Vector3 lastMouseGroundPlanePosition;
    int mouseDragThreshold = 3; // Threshold of mouse movement to start a drag

    // Unit movement
    Unit selectedUnit = null;
    Hex[] hexPath;
    LineRenderer lineRenderer;

    delegate void UpdateFunc();
    UpdateFunc Update_CurrentFunc;

    public LayerMask HexLayerMask;

    private void Update()
    {
        hexUnderMouse = MouseToHex();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelUpdateFunc();
        }

        Update_CurrentFunc();

        Update_ScrollZoom();

        lastMousePosition = Input.mousePosition;
        hexLastUnderMouse = MouseToHex();
    }

    void CancelUpdateFunc()
    {
        Update_CurrentFunc = Update_DetectModeStart;

        // Do cleanup of UI elements associated with modes (eg. unit path lines)
        selectedUnit = null;
    }

    // Only runs if not in a mode
    void Update_DetectModeStart() {
        if (Input.GetMouseButtonDown(0))
        {
            // Left mouse button pressed this frame
            // Doesn't do anything on its own
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("Mouse up click");

            // Are we clicking on a tile with a unit? If so, select it
            Unit[] units = hexUnderMouse.Units();

            // TODO: Implement cycling through multiple units in same tile

            if (units.Length > 0)
            {
                selectedUnit = units[0];
                Update_CurrentFunc = Update_UnitMovement;
            }

        }
        else if (Input.GetMouseButton(0) && 
            Vector3.Distance(Input.mousePosition, lastMousePosition) > mouseDragThreshold)
        {
            // Left button is held down and mouse moved: camera drag
            Update_CurrentFunc = Update_CameraDrag;
            lastMouseGroundPlanePosition = MouseToGroundPlane(Input.mousePosition);
            Update_CurrentFunc();

        }
        else if(selectedUnit != null && Input.GetMouseButtonDown(1))
        {
            // A unit is selected and the mouse button is being held down.
            // We are in unit movement mode -- Show a path from unit to mouse positionl


        }
    }
    
    Hex MouseToHex()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if( Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, HexLayerMask))
        {
            // Something got hit
            //Debug.Log(hitInfo.collider.name);

            // The collider is a child of the game object
            GameObject hexGO = hitInfo.rigidbody.gameObject;

            return hexMap.GetHexFromGameObject(hexGO);
        }
        Debug.Log("Found nothing");
        return null;
    }

    Vector3 MouseToGroundPlane(Vector3 mousePos)
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(mousePos);
        // Where does the mouse ray intersect y = 0 ?
        if (mouseRay.direction.y >= 0)
        {
            Debug.Log("Mouse should not be pointing up");
            return Vector3.zero;
        }
        float rayLength = (mouseRay.origin.y / mouseRay.direction.y);
        return mouseRay.origin - (mouseRay.direction * rayLength);
    }

    void Update_UnitMovement()
    {
        if (Input.GetMouseButtonUp(1) || selectedUnit == null)
        {
            if( selectedUnit != null)
            {
                selectedUnit.SetHexPath(hexPath);
            }

            CancelUpdateFunc();
            return;
        }

        // We have a selected unit
        // Look at hex under mouse
        // Is this a different hex than before? (Or we don't have a path)
        if (hexPath == null || hexUnderMouse != hexLastUnderMouse)
        {
            // Pathfind to that hex
            hexPath = QPath.QPath.FindPath<Hex>(hexMap, selectedUnit, selectedUnit.Hex, hexUnderMouse, Hex.CostEstimate);
            // Draw the path
            DrawPath(hexPath);
        }

    }

    void DrawPath(Hex[] hexPath)
    {
        if (hexPath.Length == 0)
        {
            lineRenderer.enabled = false;
            return;
        }
        lineRenderer.enabled = true;

        Vector3[] positions = new Vector3[hexPath.Length];

        for (int i = 0; i < hexPath.Length; i++)
        {
            GameObject hexGO = hexMap.GetHexGO(hexPath[i]);
            positions[i] = hexGO.transform.position + (Vector3.up * 0.1f);
        }
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    void Update_CameraDrag ()
    {

        if (Input.GetMouseButtonUp(0))
        {
            CancelUpdateFunc();
            return;
        }

        Vector3 hitPos = MouseToGroundPlane(Input.mousePosition);

        Vector3 diff = lastMouseGroundPlanePosition - hitPos;
        Camera.main.transform.Translate(diff, Space.World);

        lastMouseGroundPlanePosition = hitPos = MouseToGroundPlane(Input.mousePosition);

    }

    void Update_ScrollZoom()
    {
        // Zoom to scroll wheel
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        float minHeight = 4f;
        float maxHeight = 20f;

        if (Mathf.Abs(scrollAmount) > 0.01f)
        {
            // Move camera towards hitPos
            Vector3 hitPos = MouseToGroundPlane(Input.mousePosition);
            Vector3 dir = hitPos - Camera.main.transform.position;

            Vector3 p = Camera.main.transform.position;

            if (Mathf.Abs(scrollAmount) > 0 && p.y > minHeight && p.y < maxHeight)
            {
                Camera.main.transform.Translate(dir * scrollAmount, Space.World);
            }
            else if (scrollAmount > 0 && p.y >= maxHeight)
            {
                Camera.main.transform.Translate(dir * scrollAmount, Space.World);
            }
            else if (scrollAmount < 0 && p.y <= minHeight)
            {
                Camera.main.transform.Translate(dir * scrollAmount, Space.World);
            }
        }

        Camera.main.transform.rotation = Quaternion.Euler(
                Mathf.Lerp(30, 90, Camera.main.transform.position.y / (maxHeight / 1.5f)),
                Camera.main.transform.rotation.eulerAngles.y,
                Camera.main.transform.rotation.eulerAngles.z
            );
    }
}
