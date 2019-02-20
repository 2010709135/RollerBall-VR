using UnityEngine;
using System.Collections;

namespace UnityStandardAssets.Vehicles.Ball
{
    public class MapAlloc : MonoBehaviour
    {

        public int[] mapSeq;
        GameObject[] mapSrc;
        public Ball ball;
        public Ball_Single ball_Single;

        // Use this for initialization
        void Start()
        {
            mapSeq = new int[8];
            mapSrc = new GameObject[8];

            for (int i = 0; i < 8; i++)
            {
                mapSeq[i] = ball.GetComponent<Ball>().mapSeq[i];
            }

            for (int i = 0; i < 8; i++)
            {
                string mapName = "rr" + mapSeq[i].ToString();
                mapSrc[i] = (GameObject)Instantiate(Resources.Load(mapName));
            }

            mapSrc[0].GetComponent<Transform>().localPosition = new Vector3(-30, 0, -30);
            mapSrc[1].GetComponent<Transform>().localPosition = new Vector3(-30, 0, 0);
            mapSrc[2].GetComponent<Transform>().localPosition = new Vector3(-30, 0, 30);
            mapSrc[3].GetComponent<Transform>().localPosition = new Vector3(0, 0, -30);
            mapSrc[4].GetComponent<Transform>().localPosition = new Vector3(0, 0, 30);
            mapSrc[5].GetComponent<Transform>().localPosition = new Vector3(30, 0, -30);
            mapSrc[6].GetComponent<Transform>().localPosition = new Vector3(30, 0, 0);
            mapSrc[7].GetComponent<Transform>().localPosition = new Vector3(30, 0, 30);



        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}