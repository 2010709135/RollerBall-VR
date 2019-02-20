using UnityEngine;
using System.Collections;

public class ItemGenerator : MonoBehaviour {
	Transform tr;
	bool reach = false;
	bool moving = false;

	Vector3 start;
	Vector3 dest;

	 float moveSpeed = 1f;

	float delta = 0;

	int x_dif, y_dif, z_dif;

	// Use this for initialization
	void Start () {

		tr = gameObject.GetComponent<Transform> ();

		float x = Random.Range (-50, 50);
		float y = Random.Range (10, 20);
		float z = Random.Range (-50, 50);

		dest = new Vector3 (x, y, z);

		moving = true;
	}
	
	// Update is called once per frame
	void Update () {
		
		delta += Time.deltaTime;

		//Debug.Log (delta);
		if (delta > 15) {
			delta = 0;
			float x = Random.Range (-50, 50);
			float y = Random.Range (10, 20);
			float z = Random.Range (-50, 50);

			dest = new Vector3 (x, y, z);

			moving = true;


		} 

		tr.position = Vector3.Lerp (tr.position, dest, 0.001f );
		//tr.Translate(dest);
	}
}
