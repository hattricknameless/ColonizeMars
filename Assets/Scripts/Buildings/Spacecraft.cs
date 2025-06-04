using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spacecraft : Building {

	private GameObject astronautPrefab;
	[SerializeField] private float summonInterval;

	//Component References
	private ParticleSystem[] smokes;
	private GameObject smokeParticles;

	[SerializeField] private float landingHeight;
	[SerializeField] private float landingDuration;
	[SerializeField] private int astronautCount;

	private new void Awake() {
		base.Awake();
		smokes = GetComponentsInChildren<ParticleSystem>();
		smokeParticles = smokes[0].transform.parent.gameObject;

		astronautPrefab = Resources.Load("SystemPrefabs/Astronaut") as GameObject;
	}

	private void Start() {
		//Clear the smokes before starting to land
		foreach (ParticleSystem smoke in smokes) {
			smoke.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		smokeParticles.SetActive(false);
	}

	public override void PlaceBuilding() {
		previewVisual.SetActive(false);

		StartCoroutine(LandingAnim());
	}

	private IEnumerator LandingAnim() {
		Vector3 startPosition = transform.position + Vector3.up * landingHeight;
		Vector3 endPosition = transform.position;

		//Initial state of the model
		transform.position = startPosition;
		visual.SetActive(true);
		foreach (ParticleSystem smoke in smokes) {
			smoke.Play();
		}
		smokeParticles.SetActive(true);

		//Landing Lerp
		float transitionHeight = landingHeight * 0.1f;
		float elapsedTime = 0f;
		float playLandingTime = 1f;
		bool playLandingFlag = true;

		//Static Landing
		float staticSpeed = landingHeight / landingDuration;
		while (transform.position.y > endPosition.y) {
			elapsedTime += Time.deltaTime;
			transform.position -= Vector3.up * staticSpeed * Time.deltaTime;
			if (Mathf.Abs(elapsedTime - playLandingTime) <= 0.1 && playLandingFlag) {
				audioManager.PlayLanding();
				playLandingFlag = false;
			}
			yield return null;
		}

		//Snap to end position
		transform.position = endPosition;
		yield return StartCoroutine(SmokeFade());

		//Astronaut summons after SmokeFade done
		yield return OnLanding();
		Debug.Log("Landing Sequence Complete");
		yield break;
	}

	private IEnumerator SmokeFade() {
		yield return new WaitForSeconds(3);
		foreach (ParticleSystem smoke in smokes) {
			smoke.Stop(false, ParticleSystemStopBehavior.StopEmitting);
		}

		while (smokes[0].IsAlive()) {
			yield return null;
		}

		smokes[0].transform.parent.gameObject.SetActive(false);
		yield return null;
	}

	private IEnumerator OnLanding() {
		for (int i = 0; i < astronautCount; i++) {
			AstronautController astronaut = Instantiate(astronautPrefab, transform.position, Quaternion.identity).GetComponent<AstronautController>();
			astronaut.currentCellIndex = cellIndex;
			yield return new WaitForSeconds(summonInterval);
		}
	}
}
