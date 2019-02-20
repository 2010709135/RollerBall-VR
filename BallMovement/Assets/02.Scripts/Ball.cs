using System;
using UnityEngine;
using System.Net.Sockets;  // socket
using System.Net;   // IPAddress
using System.Text;  // Encoding
using System.Runtime.InteropServices; // class to bytes;
using System.Runtime.Serialization.Formatters.Binary; // BinaryFormatter
using System.IO; // MemoryStream
using System.Runtime.Serialization;
using System.Threading;  // for receiving data
using System.Collections.Generic; // linkedlist
using System.Collections;


namespace UnityStandardAssets.Vehicles.Ball
{
    public class Ball : MonoBehaviour
    {
        [SerializeField]
        protected float m_MovePower = 5; // The force added to the ball to move it.
        [SerializeField]
        protected bool m_UseTorque = true; // Whether or not to use torque to move the ball.
        [SerializeField]
        protected float m_MaxAngularVelocity = 25; // The maximum velocity the ball can rotate at.
        [SerializeField]
        protected float m_JumpPower = 2; // The force added to the ball when it jumps.

        protected const float k_GroundRayLength = 1f; // The length of the ray to check if the ball is grounded.
        protected Rigidbody m_Rigidbody;    

        // for communicate with server
        protected Socket socket;
        protected Transform tr;  // my transform component


        // not to use update() too many
        protected Vector3 tempPosition;

        protected byte[] otherNetBuffer;  // other player's position buffer
        protected byte[] recoBuffer;

        protected byte[] startBuffer;
        protected byte[] nameBuffer;
        protected byte[] posBuffer;
        protected byte[] rotBuffer;

        //  short totDataSize;

        public const int MAX_GAMER = 4;  // max player in one room
        public const int BUF_SIZE = 1024;

        public int numOfClient;

        //     string ID = "Jay"; // My ID
        protected string IdNet;      // received data's ID

        protected Header header;


        // string[] othersID;  // others ID
        protected LinkedList<string> othersID;

        protected Thread receiveThread;

        public GameObject tempBall;
        public GameObject tempHUD;
        //public GameObject mapAlloc;

        private MapAlloc map;

        protected Info info;
        protected Info tempInfo;


        public int[] mapSeq;

		GameObject[] wall;
		bool wallDone;

        private void Awake()
        {
			if (gameObject.name == "RollerBall_Single")
				return;

            m_Rigidbody = GetComponent<Rigidbody>();
            // Set the maximum angular velocity.
            GetComponent<Rigidbody>().maxAngularVelocity = m_MaxAngularVelocity;


            tr = GetComponent<Transform>();

            GetComponent<MeshRenderer>().enabled = false;

            GameObject ob = GameObject.FindGameObjectWithTag("CONNECT");
            socket = ob.GetComponent<ConnectServer>().getSocket();

            info = new Info();

            info.name = ob.GetComponent<ConnectServer>().getName();
            info.pos = tr.localPosition;
            info.rot = tr.localRotation;

            

            othersID = new LinkedList<string>();

            // set Packet header
            header = new Header();
            header.start = "Start";
            header.startSize = (short)header.start.Length;
            header.nameSize = (short)info.name.Length;
            header.posSize = (short)Marshal.SizeOf(info.pos);
            header.rotSize = (short)Marshal.SizeOf(info.rot);

            header.close = "Close";
            header.ID = ConnectServer.ID;
            header.single = ConnectServer.SINGLE;
            header.multi = ConnectServer.MULTI;
            header.rank = ConnectServer.RANK;
            header.exit = ConnectServer.EXIT;

            // buffer for sending info
            otherNetBuffer = new byte[BUF_SIZE];

            startBuffer = new byte[header.startSize];
            nameBuffer = new byte[header.nameSize];
            posBuffer = new byte[header.posSize];
            rotBuffer = new byte[header.rotSize];

            socket.Blocking = false;   // make socket non-blocking mode

            Debug.Log("Awake() is done");


            byte[] sizeArr;  // size byte array

            // total data size to send
            // "Start" + menu sel(char) + totalSize(short) + "name"
            header.totalSize = (short)(header.startSize + 1 + 2 + header.nameSize);

            socket.Blocking = true;

            while (true)
            {
                byte[] result = new byte[header.totalSize];

                startBuffer = Encoding.UTF8.GetBytes(header.start);

                // set start string and menu sel size into result buffer
                Buffer.BlockCopy(Encoding.UTF8.GetBytes(header.start), 0, result, 0, header.startSize);
                result[header.startSize] = Convert.ToByte(header.multi);
                // set (short)nameSize and nameBuffer into result buffer
                sizeArr = BitConverter.GetBytes(header.nameSize);

                Buffer.BlockCopy(sizeArr, 0, result, header.startSize + 1, 2);
                Buffer.BlockCopy(nameBuffer, 0, result, header.startSize + 1 + 2, nameBuffer.Length);

                Header tempHeader = new Header();

                socket.Send(result);

                // receive others ID and position
                byte[] tempBuffer = new byte[20];

                int recvSize;

                try {


                    recvSize = socket.Receive(otherNetBuffer);

                    Debug.Log(recvSize);

                    // read start string 
                    recoBuffer = new byte[header.startSize];
                    Buffer.BlockCopy(otherNetBuffer, 0, recoBuffer, 0, header.startSize);

                    tempHeader.start = Encoding.Default.GetString(recoBuffer);
                    Debug.Log(tempHeader.start);
                    if (tempHeader.start == header.start)
                    {
                        // read total data size 
                        recoBuffer = new byte[2];
                        Buffer.BlockCopy(otherNetBuffer, header.startSize, recoBuffer, 0, 2);

                        tempHeader.totalSize = BitConverter.ToInt16(recoBuffer, 0);

                        Debug.Log("recvSize = " + recvSize.ToString());
                        Debug.Log("totalSize = " + tempHeader.totalSize.ToString());
                        // if read data is less than total data, skip below
                        if (recvSize >= tempHeader.totalSize)
                        {
                            recoBuffer = new byte[4];
                            Buffer.BlockCopy(otherNetBuffer, header.startSize + 2, recoBuffer, 0, 4);
                            numOfClient = (int)RawDeserializeEx(recoBuffer, typeof(int)) + 1;
                            
                            mapSeq = new int[8];

                            for (int q = 0; q < 8; q++)
                            {
                                recoBuffer = new byte[4];
                                Buffer.BlockCopy(otherNetBuffer, header.startSize + 2 + 4 + 4 * q, recoBuffer, 0, 4);
                                mapSeq[q] = (int)RawDeserializeEx(recoBuffer, typeof(int));
                            }

                            int com = 0;
                            for (com = 0; com < 8; com++)
                            {
                                if (mapSeq[com] < 0 || 8 < mapSeq[com])
                                    break;
                            }                     

                            // to send ok
                            short totalSize = (short)(header.startSize + 1 + 2);

                            // start + total Data Size + (short)nameSize + name + (short)posSize + pos + (short)rotSize + rot
                            result = new byte[header.totalSize];

                            startBuffer = Encoding.UTF8.GetBytes(header.start);

                            // set start string and menu sel size into result buffer
                            Buffer.BlockCopy(Encoding.UTF8.GetBytes(header.start), 0, result, 0, header.startSize);
                            result[header.startSize] = Convert.ToByte(header.multi);
                            // set (short)nameSize and nameBuffer into result buffer
                            startBuffer = Encoding.UTF8.GetBytes("OK");

                            Buffer.BlockCopy(startBuffer, 0, result, header.startSize + 1, 2);

                            socket.Send(result);

                            break;
                        }
                    }

                }
                catch (SocketException e)
                {
                    if (e.ErrorCode == 10035)
                    {
                     //   Debug.Log("nothing received");
                    }
                }
			
			}

            socket.Blocking = false;

            
            GameObject pos = null;

            switch (numOfClient)
            {
                case 1:
                    pos = GameObject.FindGameObjectWithTag("POS1");
                    break;
                case 2:
                    pos = GameObject.FindGameObjectWithTag("POS2");
                    break;
                case 3:
                    pos = GameObject.FindGameObjectWithTag("POS3");
                    break;
                case 4:
                    pos = GameObject.FindGameObjectWithTag("POS4");
                    break;
            }

            tr.localPosition = pos.GetComponent<Transform>().localPosition;

			wall = GameObject.FindGameObjectsWithTag ("WALL");
			wallDone = false;
        }


        public void Move(Vector3 moveDirection, bool jump)
        {

            // If using torque to rotate the ball...
            if (m_UseTorque)
            {
                // ... add torque around the axis defined by the move direction.
                m_Rigidbody.AddTorque(new Vector3(moveDirection.z, 0, -moveDirection.x) * m_MovePower);
            }
            else
            {
                // Otherwise add force in the move direction.
                m_Rigidbody.AddForce(moveDirection * m_MovePower);
            }

            // If on the ground and jump is pressed...
            if (Physics.Raycast(transform.position, -Vector3.up, k_GroundRayLength) && jump)
            {
                // ... add force in upwards.
                m_Rigidbody.AddForce(Vector3.up * m_JumpPower, ForceMode.Impulse);
            }
        }


        // send my pos to server, and receive others pos info from server
        void Update()
        {
            // when player number is MAX_GAMER, then wall will float,
            // so player can play into maze
			if (wallDone == false &&  numOfClient == MAX_GAMER) {
				for (int i = 0; i < 4; i++) {
					wall[i].GetComponent<Transform> ().localPosition += new Vector3(0,(float)0.05,0);

				}
				if (wall[0].GetComponent<Transform> ().localPosition.y > 10 &&
				   wall[1].GetComponent<Transform> ().localPosition.y > 10 &&
				   wall[2].GetComponent<Transform> ().localPosition.y > 10 &&
				   wall[3].GetComponent<Transform> ().localPosition.y > 10)
					wallDone = true;
			}
				

            // to save header data 
            Header tempHeader = new Header();

            // to recovery data from otherNetBuffer
            tempInfo = new Info();

            int recvSize = 0;

            // save current info. member name is not changable
            info.pos = tr.localPosition;
            info.rot = tr.localRotation;

            // pos and rot size can be changed. so every time we send data
            // we must calculate size of these two info
            header.posSize = (short)Marshal.SizeOf(info.pos);
            header.rotSize = (short)Marshal.SizeOf(info.rot);
            // to save byte stream of pos and rot
            posBuffer = new byte[header.posSize];
            rotBuffer = new byte[header.rotSize];

            byte[] sizeArr;  // size byte array

            // set info into buffers
            startBuffer = Encoding.UTF8.GetBytes(header.start);
            nameBuffer = Encoding.UTF8.GetBytes(info.name);
            posBuffer = RawSerializeEx(info.pos);
            rotBuffer = RawSerializeEx(info.rot);

            // total data size to send
            tempHeader.totalSize = (short)(header.startSize + 1 + 2
            + 2 + header.nameSize
            + 2 + header.posSize
            + 2 + header.rotSize);

            // start + (char)menu selection(multi) + (short)total Data Size + 
            // (short)nameSize + name + (short)posSize + pos + (short)rotSize + rot
            byte[] result = new byte[tempHeader.totalSize];


            // set start string and total data size into result buffer
            sizeArr = BitConverter.GetBytes(tempHeader.totalSize);
            Buffer.BlockCopy(startBuffer, 0, result, 0, header.startSize);
            // send "multi" character , so that server can treat this client
            // as multi player
            result[header.startSize] = Convert.ToByte(header.multi);
            Buffer.BlockCopy(sizeArr, 0, result, header.startSize + 1, 2);

            // set (short)nameSize and nameBuffer into result buffer
            sizeArr = BitConverter.GetBytes(header.nameSize);
            Buffer.BlockCopy(sizeArr, 0
                , result, header.startSize + 1 + 2
                , 2);
            Buffer.BlockCopy(nameBuffer, 0
                , result, header.startSize + 1 + 2 + 2
                , header.nameSize);

            // set (short)posSize and posBuffer into result buffer
            sizeArr = BitConverter.GetBytes(header.posSize);
            Buffer.BlockCopy(sizeArr, 0, result
                , header.startSize + 1 + 2 + 2 + header.nameSize
                , 2);
            Buffer.BlockCopy(posBuffer, 0, result
                , header.startSize + 1 + 2 + 2 + header.nameSize + 2
                , header.posSize);

            // set (short)rotation size and rotBuffer into result buffer
            sizeArr = BitConverter.GetBytes(header.rotSize);
            Buffer.BlockCopy(sizeArr, 0, result
                , header.startSize + 1 + 2 + 2 + header.nameSize + 2 + header.posSize
                , 2);
            Buffer.BlockCopy(rotBuffer, 0, result
                , header.startSize + 1 + 2 + 2 + header.nameSize + 2 + header.posSize + 2
                , header.rotSize);

            // send all data with header
            socket.Send(result);

            // receive other's id first
            try
            {
                // start + (char)menu selection(multi) + (short)total Data Size + 
                // (short)nameSize + name + (short)posSize + pos + (short)rotSize + rot
                recvSize = socket.Receive(otherNetBuffer);

                // read start string 
                recoBuffer = new byte[header.startSize];
                Buffer.BlockCopy(otherNetBuffer, 0, recoBuffer, 0, header.startSize);
                tempHeader.start = Encoding.Default.GetString(recoBuffer);

                char menuSel = (char)otherNetBuffer[header.startSize];

                // skip menu select data
                // if menu select is exit, erase user of that name from Scene
                if (menuSel == header.exit)
                {
                    Debug.Log("Have to remove someone");
                    // read total data size 
                    recoBuffer = new byte[2];
                    Buffer.BlockCopy(otherNetBuffer, header.startSize + 1, recoBuffer, 0, 2);
                    tempHeader.totalSize = BitConverter.ToInt16(recoBuffer, 0);

                    // if read data is less than total data, skip below
                    if (recvSize >= tempHeader.totalSize)
                    {
                        // read name size
                        recoBuffer = new byte[2];
                        Buffer.BlockCopy(otherNetBuffer, header.startSize + 1 + 2, recoBuffer, 0, 2);
                        tempHeader.nameSize = BitConverter.ToInt16(recoBuffer, 0);

                        if (tempHeader.nameSize <= 0)
                            return;

                        // read name
                        recoBuffer = new byte[tempHeader.nameSize];
                        Buffer.BlockCopy(otherNetBuffer, header.startSize + 1 + 2 + 2, recoBuffer, 0, tempHeader.nameSize);
                        tempInfo.name = Encoding.Default.GetString(recoBuffer);

                        RemovePlayerFromHierarchy(tempInfo.name);
                    }
                }
                else if (tempHeader.start == header.start)
                {
                    // read total data size 
                    recoBuffer = new byte[2];
                    Buffer.BlockCopy(otherNetBuffer, header.startSize + 1, recoBuffer, 0, 2);
                    tempHeader.totalSize = BitConverter.ToInt16(recoBuffer, 0);

                    // if read data is less than total data, skip below
                    if (recvSize >= tempHeader.totalSize)
                    {
                        // read name size
                        recoBuffer = new byte[2];
                        Buffer.BlockCopy(otherNetBuffer, header.startSize + 1 + 2, recoBuffer, 0, 2);
                        tempHeader.nameSize = BitConverter.ToInt16(recoBuffer, 0);

                        if (tempHeader.nameSize <= 0)
                            return;

                        // read name
                        recoBuffer = new byte[tempHeader.nameSize];
                        Buffer.BlockCopy(otherNetBuffer, header.startSize + 1 + 2 + 2, recoBuffer, 0, tempHeader.nameSize);
                        tempInfo.name = Encoding.Default.GetString(recoBuffer);
                                                                     
                        // read pos size
                        recoBuffer = new byte[2];
                        Buffer.BlockCopy(otherNetBuffer,
                            header.startSize + 1 + 2 + 2 + tempHeader.nameSize,
                            recoBuffer, 0, 2);
                        tempHeader.posSize = BitConverter.ToInt16(recoBuffer, 0);

                        if (tempHeader.posSize <= 0)
                            return;

                        // read pos
                        recoBuffer = new byte[tempHeader.posSize];
                        Buffer.BlockCopy(otherNetBuffer,
                            header.startSize + 1 + 2 + 2 + tempHeader.nameSize + 2,
                            recoBuffer, 0, tempHeader.posSize);
                        tempInfo.pos = (Vector3)RawDeserializeEx(recoBuffer, typeof(Vector3));

                        // read rot size
                        recoBuffer = new byte[2];
                        Buffer.BlockCopy(otherNetBuffer,
                            header.startSize + 1 + 2 + 2 + tempHeader.nameSize + 2 + tempHeader.posSize,
                            recoBuffer, 0, 2);
                        tempHeader.rotSize = BitConverter.ToInt16(recoBuffer, 0);

                        if (tempHeader.rotSize <= 0)
                            return;

                        // read rot

                        recoBuffer = new byte[tempHeader.rotSize];
                        Buffer.BlockCopy(otherNetBuffer,
                            header.startSize + 1 + 2 + 2 + tempHeader.nameSize + 2 + tempHeader.posSize + 2,
                            recoBuffer, 0, tempHeader.rotSize);
                        tempInfo.rot = (Quaternion)RawDeserializeEx(recoBuffer, typeof(Quaternion));

                        // if name is contained in othersID, then set position and rotation
                        // on same named object
                        if (othersID.Contains(tempInfo.name))
                        {
                            GameObject[] ob = GameObject.FindGameObjectsWithTag("NET_BALL");

                            foreach (GameObject tempOb in ob)
                            {
                                if (tempOb.name == tempInfo.name)
                                {
                                    tempOb.transform.localPosition = tempInfo.pos;
                                    tempOb.transform.localRotation = tempInfo.rot;
                                }
                            }
                        }
                        // if name is not contained in others ID, then Instantiate Object using
                        // prefabs connected to tempBall
                        else 
                        {
                            UnityEngine.Object ballObject;

                            ballObject = GameObject.Instantiate(tempBall, tempInfo.pos, tempInfo.rot);
                            ballObject.name = tempInfo.name;

                            othersID.AddFirst(tempInfo.name);
                            numOfClient++;
                        }
                    }

                }
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == 10035)
                {
                  //  Debug.Log("nothing received");
                }
            }

        }

        public static object RawDeserializeEx(byte[] rawdatas, Type anytype)
        {
            int rawsize = Marshal.SizeOf(anytype);
            if (rawsize > rawdatas.Length)
                return null;
            GCHandle handle = GCHandle.Alloc(rawdatas, GCHandleType.Pinned);
            IntPtr buffer = handle.AddrOfPinnedObject();
            object retobj = Marshal.PtrToStructure(buffer, anytype);
            handle.Free();
            return retobj;
        }

        public static byte[] RawSerializeEx(object anything)
        {
            int rawsize = Marshal.SizeOf(anything);
            byte[] rawdatas = new byte[rawsize];
            GCHandle handle = GCHandle.Alloc(rawdatas, GCHandleType.Pinned);
            IntPtr buffer = handle.AddrOfPinnedObject();
            Marshal.StructureToPtr(anything, buffer, false);
            handle.Free();
            return rawdatas;
        }

        public static object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();

            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);

            object obj = (object)binForm.Deserialize(memStream);

            return obj;
        }

        // when program is quiting, send "close" message to others
        // in same multi room, so that they can remove my object from
        // their game
        void OnApplicationQuit()
        {
            sendCloseMessage();
        }

        public void sendCloseMessage()
        {
            int bufSize = socket.SendBufferSize;

            Header tempHeader = new Header();

            byte[] sizeArr;  // size byte array
            byte[] closeBuffer; // "close" message

            // set info into buffers
            startBuffer = Encoding.UTF8.GetBytes(header.start);
            nameBuffer = Encoding.UTF8.GetBytes(info.name);

            // total data size to send
            tempHeader.totalSize = (short)(header.startSize + 1 + 2
            + 2 + header.nameSize);

            // start + (char)menu selection(multi) + (short)total Data Size + 
            // (short)nameSize + name
            byte[] result = new byte[tempHeader.totalSize];

            // set start string and total data size into result buffer
            sizeArr = BitConverter.GetBytes(tempHeader.totalSize);

            Buffer.BlockCopy(startBuffer, 0, result, 0, header.startSize);

            // send "multi" character , so that server can treat this client
            // as multi player

            // not multi, but exit. send exit menu with name
            result[header.startSize] = Convert.ToByte(header.exit);
            // totalSize
            Buffer.BlockCopy(sizeArr, 0, result, header.startSize + 1, 2);

            // set (short)nameSize and nameBuffer into result buffer
            sizeArr = BitConverter.GetBytes(header.nameSize);
            Buffer.BlockCopy(sizeArr, 0
                , result, header.startSize + 1 + 2
                , 2);
            Buffer.BlockCopy(nameBuffer, 0
                , result, header.startSize + 1 + 2 + 2
                , header.nameSize);

            // send all data with header
            socket.Send(result);
        }

        void OnDestroy()
        {
            sendCloseMessage();
        }

        void RemovePlayerFromHierarchy(string name)
        {
            GameObject[] obArr = GameObject.FindGameObjectsWithTag("NET_BALL");

            foreach (GameObject ob in obArr)
            {
                if (name == ob.name)
                {
                    othersID.Remove(name);
                    GameObject.Destroy(ob, 0);
                }
            }

            GameObject[] hudArr = GameObject.FindGameObjectsWithTag("HUD");
            string hudName = name + "_HUD";

            foreach (GameObject ob in hudArr)
            {
                if (hudName == ob.name)
                {
                    GameObject.Destroy(ob, 0);
                }
            }
        }

    }
}