using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathAnchor : MonoBehaviour {

	private WorldGenerator worldGenerator;
	private PathManager pathManager;

	//Path Data
    private Vector3Int parentTileIndex;
	public Vector3Int cellIndex;

	//UI Controls
	[SerializeField] private GameObject frameUp;
	[SerializeField] private GameObject frameDown;
	[SerializeField] private GameObject frameLeft;
	[SerializeField] private GameObject frameRight;

	private void Awake() {
		worldGenerator = WorldGenerator.worldGenerator;
		pathManager = PathManager.pathManager;
	}

	private void Update() {
	}

	private void OnTriggerEnter(Collider other) {
		if (other.CompareTag("TerrianTile")) {
			pathManager.RegisterToBlockedPath(cellIndex);
            Destroy(gameObject);
        }
	}

	public void EnterCellIndex(Transform parentTransform) {
		parentTileIndex = parentTransform.gameObject.GetComponent<TileAnchor>().cellIndex;
		Vector3 localPosition = gameObject.transform.position - Vector3.Scale(parentTileIndex, worldGenerator.gridScale);
		transform.rotation = Quaternion.Euler(new Vector3(0, -parentTransform.rotation.eulerAngles.y, 0));
		int offsetX = Mathf.RoundToInt(localPosition.x / 0.67f);
		int offsetZ = Mathf.RoundToInt(localPosition.z / 0.67f);
		cellIndex = new Vector3Int(parentTileIndex.x * 3, parentTileIndex.y, parentTileIndex.z * 3) + new Vector3Int(offsetX, 0, offsetZ) + new Vector3Int(1, 0, 1);
	}
	
	public void SelectAll() {
		frameUp.SetActive(true);
		frameDown.SetActive(true);
		frameLeft.SetActive(true);
		frameRight.SetActive(true);
	}

	public void DeselectAll() {
		frameUp.SetActive(false);
		frameDown.SetActive(false);
		frameLeft.SetActive(false);
		frameRight.SetActive(false);
	}

	public void SelectUp() {
		frameUp.SetActive(true);
	}

	public void SelectDown() {
		frameDown.SetActive(true);
	}
	
	public void SelectLeft() {
		frameLeft.SetActive(true);
	}

	public void SelectRight() {
		frameRight.SetActive(true);
	}
}