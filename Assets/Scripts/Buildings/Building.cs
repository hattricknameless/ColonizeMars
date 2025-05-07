using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour {
    
    //References
    protected AudioManager audioManager;

    protected GameObject previewVisual;
    protected GameObject visual;
    public int size = 1;
    public Vector3Int cellIndex;

	protected void Awake() {
        audioManager = AudioManager.audioManager;
        
        previewVisual = transform.GetChild(0).gameObject;
        previewVisual.SetActive(true);

        visual = transform.GetChild(1).gameObject;
        visual.SetActive(false);
	}

    public virtual void PlaceBuilding() {
        previewVisual.SetActive(false);
        visual.SetActive(true);
    }
}
