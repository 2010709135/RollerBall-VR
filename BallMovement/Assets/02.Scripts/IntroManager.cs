using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		StartCoroutine( ExecuteAfterTime(2));
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	IEnumerator ExecuteAfterTime(float time)
	{
		yield return new WaitForSeconds(time);

		SceneManager.LoadScene("Login 1");
		// Code to execute after the delay
	}
}
