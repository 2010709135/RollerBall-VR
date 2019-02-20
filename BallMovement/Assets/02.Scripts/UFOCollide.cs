using UnityEngine;
using System.Collections;

public class UFOCollide : MonoBehaviour {
	public GameObject exitManager;
	Goal goal;
	public GameObject center;

	// Use this for initialization
	void Start () {
		
	}

	void OnCollisionEnter(Collision other)
	{
		center.GetComponent<Goal> ().endOfGame = true;
		if (other.collider.tag == "Player" || other.collider.tag == "NET_BALL")
		{
			StartCoroutine( ExecuteAfterTime(6));
		}
		//Debug.Log(other.collider.name);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	IEnumerator ExecuteAfterTime(float time)
	{
		yield return new WaitForSeconds(time);

		exitManager.GetComponent<ExitManager>().ReturnToLobby();
		// Code to execute after the delay
	}

}
