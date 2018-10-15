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

    public GameObject UnitSelectionPanel;
    public LayerMask HexLayerMask;

    // Generic bookkeeping variables
    Vector3 lastMousePosition; // From Input.mousePosition
    HexMap hexMap;
    Hex hexUnderMouse;
    Hex hexLastUnderMouse;

    // Camera dragging bookkeeping variables
    Vector3 lastMouseGroundPlanePosition;
    int mouseDragThreshold = 3; // Threshold of mouse movement to start a drag

    // Unit movement
    Unit __selectedUnit = null;
    public Unit SelectedUnit {
        get { return __selectedUnit; }
        set {
            __selectedUnit = value;
            UnitSelectionPanel.SetActive( __selectedUnit != null );
        }
    }





    Hex[] hexPath;
    LineRenderer lineRenderer;

    delegate void UpdateFunc();
    UpdateFunc Update_CurrentFunc;

    private void Update()
    {
        hexUnderMouse = MouseToHex();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SelectedUnit = null;
            CancelUpdateFunc();
        }

        Update_CurrentFunc();

        Update_ScrollZoom();

        lastMousePosition = Input.mousePosition;
        hexLastUnderMouse = MouseToHex();

        if(SelectedUnit != null)
        {
            DrawPath((hexPath != null) ? hexPath : SelectedUnit.GetHexPath());
        }
        else
        {
            DrawPath(null); // Clear path display
        }
    }

    void DrawPath(Hex[] hexPath)
    {
        if (hexPath == null || hexPath.Length == 0)
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

    void CancelUpdateFunc()
    {
        Update_CurrentFunc = Update_DetectModeStart;

        // Do cleanup of UI elements associated with modes (eg. unit path lines)
        hexPath = null;
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
                SelectedUnit = units[0];

                // Selecting a unit does NOT change mouse mode
                //Update_CurrentFunc = Update_UnitMovement;
            }
        }
        else if( SelectedUnit != null && Input.GetMouseButtonDown(1))
        {
            // We have a selected unit, and we've pushed down the right mouse button,
            // so enter unit movement mode
            Update_CurrentFunc = Update_UnitMovement;

        }
        else if (Input.GetMouseButton(0) && 
            Vector3.Distance(Input.mousePosition, lastMousePosition) > mouseDragThreshold)
        {
            // Left button is held down and mouse moved: camera drag
            Update_CurrentFunc = Update_CameraDrag;
            lastMouseGroundPlanePosition = MouseToGroundPlane(Input.mousePosition);
            Update_CurrentFunc();

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
        if (Input.GetMouseButtonUp(1) || SelectedUnit == null)
        {
            if( SelectedUnit != null)
            {
                SelectedUnit.SetHexPath(hexPath);

                // Process unit movement
                StartCoroutine(hexMap.DoUnitMoves(SelectedUnit));
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
            hexPath = QPath.QPath.FindPath<Hex>(hexMap, SelectedUnit, SelectedUnit.Hex, hexUnderMouse, Hex.CostEstimate);
        }

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
