using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public static AudioManager audioManager;

    private GameObject windSoundSource;
    private GameObject landingSoundSource;
    private GameObject walkingSoundSource;
    private GameObject miningSoundSource;

    private List<GameObject> walkingStack = new();
    private int walkingCount;
    private List<GameObject> miningStack = new();
    private int miningCount;
    [SerializeField] private int maxAudioStack;

	private void Awake() {
        audioManager = this;

        walkingSoundSource = Resources.Load("AudioClip/WalkingSoundSource") as GameObject;
        miningSoundSource = Resources.Load("AudioClip/MiningSoundSource") as GameObject;
        windSoundSource = Resources.Load("AudioClip/WindSoundSource") as GameObject;
        landingSoundSource = Resources.Load("AudioClip/LandingSoundSource") as GameObject;
	}

	private void Start() {
		StartCoroutine(PlayWindSound());
	}

	public void PlayLanding() {
        GameObject landingSound = Instantiate(landingSoundSource);
        landingSound.transform.SetParent(transform);
        landingSound.GetComponent<AudioSource>().Play();
    }

    public void PlayWalkingSound() {
        walkingCount++;
        if (walkingStack.Count >= maxAudioStack) return;
        GameObject soundSource = Instantiate(walkingSoundSource);
        soundSource.transform.SetParent(transform);
        soundSource.GetComponent<AudioSource>().Play();
        walkingStack.Add(soundSource);
    }

    public void StopWalkingSound() {
        Debug.Log($"Stop walking, walkingCount {walkingCount}, walkingStack {walkingStack.Count}");
        walkingCount--;
        if (walkingCount >= maxAudioStack && walkingStack.Count > 0) return;
        AudioSource audio = walkingStack[0].GetComponent<AudioSource>();
        walkingStack.RemoveAt(0);
        StartCoroutine(AudioFadeOut(audio));
    }

    public void PlayMiningSound() {
        miningCount++;
        if (miningStack.Count >= maxAudioStack) return;
        GameObject soundSource = Instantiate(miningSoundSource);
        soundSource.transform.SetParent(transform);
        soundSource.GetComponent<AudioSource>().Play();
        miningStack.Add(soundSource);
    }

    public void StopMiningSound() {
        Debug.Log($"Stop mining, miningCount {miningCount}, miningStack {miningStack.Count}");
        miningCount--;
        if (miningStack.Count == 0) {
            Debug.LogWarning("Mining Stack empty");
            return;
        }

        if (miningCount >= maxAudioStack && miningStack.Count > 0) return;
        AudioSource audio = miningStack[0].GetComponent<AudioSource>();
        miningStack.RemoveAt(0);
        StartCoroutine(AudioFadeOut(audio));
    }

    private IEnumerator AudioFadeOut(AudioSource audioSource) {
        float startVolume = audioSource.volume;
        float fadeDuration = 1f;

        while (audioSource.volume > 0) {
            audioSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        audioSource.Stop();
        Destroy(audioSource.gameObject);
        yield break;
    }

    private IEnumerator PlayWindSound() {
        GameObject windSound = Instantiate(windSoundSource);
        windSound.transform.SetParent(transform);
        windSound.GetComponent<AudioSource>().Play();

        float minVolume = 0.1f; // Minimum volume
        float maxVolume = 0.7f; // Maximum volume
        float fluctuationInterval = 5f; // Time it takes to go from min to max volume

        windSound.GetComponent<AudioSource>().pitch = Random.Range(0.8f, 1.2f);
        while (true) {
            // Gradually increase volume
            for (float t = 0; t < fluctuationInterval; t += Time.deltaTime) {
                windSound.GetComponent<AudioSource>().volume = Mathf.Lerp(minVolume, maxVolume, t / fluctuationInterval);
                yield return null;
            }
            //Change the pitch a bit different
            windSound.GetComponent<AudioSource>().pitch = Random.Range(0.8f, 1.2f);
            //Wait before switching
            yield return new WaitForSeconds(Random.Range(10, 15));

            // Gradually decrease volume
            for (float t = 0; t < fluctuationInterval; t += Time.deltaTime) {
                windSound.GetComponent<AudioSource>().volume = Mathf.Lerp(maxVolume, minVolume, t / fluctuationInterval);
                yield return null;
            }
            //Change the pitch a bit different
            windSound.GetComponent<AudioSource>().pitch = Random.Range(0.8f, 1.2f);
            //Wait before switching
            yield return new WaitForSeconds(Random.Range(10, 15));
        }
    }
}