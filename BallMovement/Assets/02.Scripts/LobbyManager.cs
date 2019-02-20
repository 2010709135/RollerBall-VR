using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour {
    public GameObject connectManager;
    public GameObject CBM;

	// Use this for initialization
	void Start () {
        connectManager = GameObject.FindGameObjectWithTag("CONNECT");

        if(connectManager.GetComponent<ConnectServer>().getVRMode() == false)
            CBM.GetComponent<Cardboard>().VRModeEnabled = false;
    }
	
	// Update is called once per frame
	void Update () {

    }

    public void printButtonReaction()
    {

    }

    public void EnterMultiGame()
    {
        connectManager.GetComponent<ConnectServer>().MakeAliveScene();
        SceneManager.LoadSceneAsync("BallMovement");
    }

    public void EnterSinglePushGame()
    {
        connectManager.GetComponent<ConnectServer>().MakeAliveScene();
		SceneManager.LoadScene("BallMovement_Single");
    }

    IEnumerator EnterBallMovement()
    {
        AsyncOperation ao = Application.LoadLevelAsync("BallMovement");
        yield return ao;
    }

    public void nonVRMode()
    {
        if (CBM.GetComponent<Cardboard>().VRModeEnabled == true)
            CBM.GetComponent<Cardboard>().VRModeEnabled = false;
        else
            CBM.GetComponent<Cardboard>().VRModeEnabled = true;

        connectManager.GetComponent<ConnectServer>().setVRMode(CBM.GetComponent<Cardboard>().VRModeEnabled);
    }
}
