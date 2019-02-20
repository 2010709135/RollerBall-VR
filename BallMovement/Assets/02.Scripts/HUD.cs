using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {
    private Transform tr;
    private Transform mainCamera;

	// Use this for initialization
	void Start () {
        tr = GetComponent<Transform>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update () {
        tr.LookAt(mainCamera);
	}
}
