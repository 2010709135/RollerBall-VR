using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BallNetControl : MonoBehaviour
{
    string ID;
    public Transform tr;
    public GameObject net_ball;
    public Text userID;
    private Transform mainCamera;

    // Use this for initialization
    void Start()
    {
        tr = GetComponent<Transform>();

        string tempId = this.name;
        // tempId.Length - 5 is index from '_' of '_HUD'
        tempId = tempId.Remove(tempId.Length - 4);

        userID.text = tempId;

        Debug.Log(tempId);

        GameObject[] ob = GameObject.FindGameObjectsWithTag("NET_BALL");

        foreach (GameObject tempOB in ob)
        {
            if (tempOB.name == tempId)
                net_ball = tempOB;
        }

        tr.localPosition = net_ball.GetComponent<Transform>().localPosition;

        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        tr.localPosition = net_ball.GetComponent<Transform>().localPosition + new Vector3(0, 1, 0);

        tr.LookAt(mainCamera);
    }
}