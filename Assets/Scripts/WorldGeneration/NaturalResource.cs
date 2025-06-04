using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NaturalResource : MonoBehaviour {

    private PathManager pathManager;
    [SerializeField] private List<AstronautController> astronauts;
    public Vector3Int cellIndex;
    public int depositValue;
    [SerializeField] private int maxDepositValue;
    [SerializeField] private float miningInterval;
    private Coroutine miningSequence;
    
    private const int MAXASTRONAUTS = 3;

	private void Awake() {
		depositValue = maxDepositValue;
	}

	private void Start() {
		pathManager = PathManager.pathManager;
	}

	private void Update() {
		if (miningSequence == null && astronauts.Count != 0) {
            miningSequence = StartCoroutine(MiningSequence());
        }
	}

	//Structure inspired by ChatGPT
	public void RegisterToResource(AstronautController astronaut) {
        Debug.Log("Astronaut enter work site");
        astronauts.Add(astronaut);
        if (astronauts.Count == 0) {
            miningSequence = StartCoroutine(MiningSequence());
        }
    }

    public void UnregisterToResource(AstronautController astronaut) {
        Debug.Log("Astronaut left the work site");
        astronauts.Remove(astronaut);
        if (astronauts.Count == 0) {
            StopCoroutine(MiningSequence());
        }
    }

    public int Mine(int amount) {
        int minedAmount = Mathf.Min(amount, depositValue);
        depositValue -= minedAmount;
        if (depositValue <= 0) {
            Debug.Log("Resource depeleted");
        }
        return minedAmount;
    }

    private IEnumerator MiningSequence() {
        Debug.Log($"Start Mining at {cellIndex}");
        while (depositValue > 0) {
            foreach (AstronautController astronaut in astronauts) {
                astronaut.currentStorage += Mine(Mathf.Min(astronaut.miningRate, astronaut.maxStorage - astronaut.currentStorage));
                //Check if this mine action depleted the deposit
                if (depositValue <= 0) {
                    ResourceDepleted();
                }
            }
            yield return new WaitForSeconds(miningInterval);
        }
        ResourceDepleted();
        yield break;
    }

    private void ResourceDepleted() {
        pathManager.resourceGrid.Remove(cellIndex);
        transform.GetChild(0).gameObject.SetActive(false);
        Debug.Log($"Resource at {cellIndex} depleted");
    }
}