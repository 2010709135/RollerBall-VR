using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {
	public Transform tr;
	public Vector3 dest = new Vector3(-30,10,30);
	public Transform destTr;

	// Use this for initialization
	void Start () {
		tr = gameObject.GetComponent<Transform> ();
	}
	
	// Update is called once per frame
	void Update () {
		tr.position = Vector3.Lerp (tr.position, destTr.position, 0.01f); 
	}
}
