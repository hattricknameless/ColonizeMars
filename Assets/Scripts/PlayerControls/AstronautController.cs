using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstronautController : MonoBehaviour {

    //References
    private PathManager pathManager;
	private AudioManager audioManager;
	private Spacecraft spacecraft;

    public Vector3Int currentCellIndex;
	private NaturalResource targetResource;
	[SerializeField] private float moveSpeed;
	[SerializeField] private float rotateSpeed;
	public int currentStorage;
	public int maxStorage;
	public int miningRate;

	private void Awake() {
		pathManager = PathManager.pathManager;
		audioManager = AudioManager.audioManager;
		spacecraft = FindObjectOfType<Spacecraft>();
	}

	private void Start() {
		StartCoroutine(WorkLoop());
	}

	private IEnumerator WorkLoop() {
    yield return new WaitForSeconds(1); // Initial delay
    while (true) {
        // Gathering resources
        while (currentStorage < maxStorage) {
            // Find the nearest resource
            Vector3Int resourceCellIndex = pathManager.FindNearestResource(currentCellIndex);
            if (resourceCellIndex == new Vector3Int(-1, -1, -1)) {
                Debug.LogWarning("No available resources found!");
                yield return new WaitForSeconds(1); // Wait before retrying
                continue;
            }

            Debug.Log($"Found nearest resource at {resourceCellIndex}");
            yield return MoveToDestination(resourceCellIndex);

            // Check if the resource is still valid
            if (targetResource == null || targetResource.depositValue <= 0) {
                Debug.Log("Resource depleted or invalid, finding the next resource...");
                continue; // Find the next resource
            }

			audioManager.PlayMiningSound();

            // Collect resources until storage is full or resource is depleted
            while (currentStorage < maxStorage && targetResource != null && targetResource.depositValue > 0) {
                yield return null;
            }

			audioManager.StopMiningSound();
        }

        // Returning to deposit resources
        while (currentStorage >= maxStorage) {
            Debug.Log($"Max storage reached, returning to spaceship at {spacecraft.cellIndex}");
            yield return MoveToDestination(spacecraft.cellIndex);

            // Deposit resources
            currentStorage = 0;
            Debug.Log("Finished depositing resources, returning to work.");
        }
    }
}

	private IEnumerator MoveToDestination(Vector3Int destination) {
		List<Vector3Int> pathList = pathManager.PathfindingTo(currentCellIndex, destination);
		string print = "";
		foreach (Vector3Int node in pathList) print += node + ",";

		audioManager.PlayWalkingSound();

		while (pathList.Count > 0) {
			//Move to next node
			Vector3 targetPosition = pathManager.pathGrid[pathList[0]].transform.position;
			
			// Rotate to face the target position (only on the Y-axis)
			Vector3 direction = (targetPosition - transform.position).normalized;
			Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
			while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f) {
				if (Quaternion.Angle(transform.rotation, targetRotation) < 2f) {
					transform.rotation = targetRotation;
					break;
				}
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
				yield return null;
			}
			
			while (Vector3.Distance(transform.position, targetPosition) > 0.05f) {
				transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
				
				yield return null;
			}
			//Update currentCellIndex
			currentCellIndex = pathList[0];

			//Remove node when arrive
			pathList.RemoveAt(0);
			yield return new WaitForSeconds(0.1f);
		}

		audioManager.StopWalkingSound();
	}

	private void OnTriggerEnter(Collider other) {
		if (other.TryGetComponent(out NaturalResource resource)) {
            resource.RegisterToResource(this);
			targetResource = resource;
        }
	}

	private void OnTriggerExit(Collider other) {
		if (other.TryGetComponent(out NaturalResource resource)) {
			resource.UnregisterToResource(this);
			targetResource = null;
		}
	}
}
