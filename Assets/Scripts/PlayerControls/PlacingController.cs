using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlacingController : MonoBehaviour {

    private PathManager pathManager;

    //Selecting pathGrid
    private PathAnchor prevHover;
    private PathAnchor currentHover;
    private enum SelectionMode {
        x1, x2
    }
    [SerializeField] private SelectionMode selectionMode = SelectionMode.x1;

    //Placing Buildings
    private int buildingIndex;
    private List<GameObject> buildingPrefabs;
    private bool isBuilding = false;
    private bool isPlacable = false;
    private GameObject currentBuilding;
    private Building building;
    public enum HoverState {
        x1, x2, none
    }
    public HoverState hoverState = HoverState.none;

    //Placing Input
    private InputAction placingAction;

	private void Awake() {
        buildingPrefabs = Resources.LoadAll<GameObject>("BuildingPrefabs").ToList();

        var inputActionAsset = GetComponent<PlayerInput>().actions;
        placingAction = inputActionAsset["CameraMovement/PlaceBuilding"];
	}

	private void Start() {
        pathManager = PathManager.pathManager;
    }

	private void OnEnable() {
		placingAction.Enable();
        placingAction.performed += PlaceBuilding;
	}

	private void OnDisable() {
		placingAction.Disable();
        placingAction.performed -= PlaceBuilding;
	}

	private void Update() {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //Get if cursor hover on any of the pathAnchors
        if (Physics.Raycast(ray, out RaycastHit hit, 500, LayerMask.GetMask("PathAnchor"))) {
            currentHover = hit.collider.GetComponent<PathAnchor>();

            //If hover from none to pathAnchor
            if (prevHover == null) {
                OnCursorHover(currentHover.cellIndex);
            }
            //If hover to a different pathAnchor
            else if (currentHover != prevHover) {
                pathManager.OnCursorExit();
                OnCursorHover(currentHover.cellIndex);
            }

            //Update hoverState
            if (pathManager.selectedPaths.Count == 1) {
                hoverState = HoverState.x1;
            }
            else if (pathManager.selectedPaths.Count == 4) {
                hoverState = HoverState.x2;
            }
        }
        //If hover to none
        else {
            currentHover = null;
            //If hover from pathAnchor to none
            if (prevHover != null) {
                pathManager.OnCursorExit();
                hoverState = HoverState.none;
            }
        }

        UpdatePreview();
        prevHover = currentHover;
	}

    private void OnCursorHover(Vector3Int cellIndex) {
        switch (selectionMode) {
            case SelectionMode.x1:
                hoverState = HoverState.x1;
                pathManager.OnCursorHoverX1(cellIndex);
                break;
            case SelectionMode.x2:
                hoverState = HoverState.x2;
                pathManager.OnCursorHoverX2(cellIndex);
                break;
        }
    }

    public void SelectBuilding(int index) {
        isBuilding = true;
        isPlacable = false;
        buildingIndex = index;
        currentBuilding = Instantiate(buildingPrefabs[buildingIndex]);
        building = currentBuilding.GetComponent<Building>();
        
        //Change selectionMode with building size
        switch (building.size) {
            case 1:
                selectionMode = SelectionMode.x1;
                break;
            case 2: selectionMode = SelectionMode.x2;
                break;
        }
    }

    private void UpdatePreview() {
        if (!isBuilding) return;
        //If the size and state matches, shows the preview
        if ((hoverState == HoverState.x1 && building.size == 1) || (hoverState == HoverState.x2 && building.size == 2)) {
            currentBuilding.SetActive(true);
            isPlacable = true;

            //Update the visual position of the preview
            Vector3 sum = Vector3.zero;
            foreach (Vector3Int point in pathManager.selectedPaths) {
                sum += pathManager.pathGrid[point].transform.position;
            }

            currentBuilding.transform.position = sum / pathManager.selectedPaths.Count;
        }
        //else disable the preview
        else {
            currentBuilding.SetActive(false);
            isPlacable = false;
        }
    }

    private void PlaceBuilding(InputAction.CallbackContext context) {
        if (!isBuilding || hoverState == HoverState.none) return;
        if (!isPlacable) return;
        building.PlaceBuilding();
        building.cellIndex = currentHover.cellIndex;
        
        //Mark selected path as occupied
        pathManager.PlaceBuilding();

        isBuilding = false;
        isPlacable = false;
    }
}