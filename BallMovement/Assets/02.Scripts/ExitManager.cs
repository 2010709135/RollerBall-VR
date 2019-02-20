using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Vehicles.Ball;

public class ExitManager : MonoBehaviour {
    public GameObject connectManager;
    public GameObject ball;
    public GameObject CBM;

	// Use this for initialization
	void Start () {
        connectManager = GameObject.FindGameObjectWithTag("CONNECT");
        //CBM.GetComponent<Cardboard>().VRModeEnabled = connectManager.GetComponent<ConnectServer>().getVRMode();
        //CBM.GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ReturnToLobby()
    {
        //ball.GetComponent <Ball> ().sendCloseMessage();
        //connectManager.GetComponent<ConnectServer>().MakeAliveScene();
        SceneManager.LoadScene("BallLobby");
    }
}
