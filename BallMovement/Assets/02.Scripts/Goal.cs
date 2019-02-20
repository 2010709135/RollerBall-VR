using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Goal : MonoBehaviour {
    Transform tr;
    public GameObject exitManager;
	GameObject Winner;
	public bool reach = false;
	bool gameOver = false;
	public bool endOfGame = false;
	// Use this for initialization
	void Start () {
        tr = GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update () {
		if (reach == false) {
			if (gameOver == true) {
				float y = Winner.GetComponent<Transform> ().localPosition.y;
				Winner.GetComponent<Transform> ().localPosition = new Vector3 (0, y, 0);
				Winner.GetComponent<Transform> ().localPosition += new Vector3 (0, (float)0.05, 0);
			}
		} else {
			Winner.GetComponent<Transform> ().localPosition = new Vector3 (0, 30, 0);
		}
	}

	void OnTriggerEnter(Collider other)
	{
		string msg;

		if (other.GetComponent<Collider>().tag == "Player" || other.GetComponent<Collider>().tag == "NET_BALL")
		{
			GameObject ob = GameObject.FindGameObjectWithTag("ENDMSG");
			ob.GetComponent<Canvas>().enabled = true;

			Text text = ob.GetComponentInChildren<Text>();

			Winner = other.GetComponent<Collider>().gameObject;

			if (other.GetComponent<Collider>().name == "RollerBall")
			{
				msg = "You Win";
				text.color = Color.blue;
			}
			else {
				msg = other.GetComponent<Collider>().name;
				msg += " Win";
				text.color = Color.red;
			}

			text.text = msg;

			other.GetComponent<Rigidbody> ().useGravity = false;
			gameOver = true;
			gameObject.GetComponent<CapsuleCollider> ().enabled = false;
			Winner.GetComponent<Transform> ().localPosition = new Vector3 (0, 0, 0);
		}
	}

    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        exitManager.GetComponent<ExitManager>().ReturnToLobby();
        // Code to execute after the delay
    }

}
