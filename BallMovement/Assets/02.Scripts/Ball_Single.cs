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


namespace UnityStandardAssets.Vehicles.Ball
{
    public class Ball_Single : Ball
    {
       

        private void Awake()
        {
			
            m_Rigidbody = GetComponent<Rigidbody>();
            // Set the maximum angular velocity.
            GetComponent<Rigidbody>().maxAngularVelocity = m_MaxAngularVelocity;


            tr = GetComponent<Transform>();

            GetComponent<MeshRenderer>().enabled = false;
            
            //GameObject ob = GameObject.FindGameObjectWithTag("CONNECT");
            //socket = ob.GetComponent<ConnectServer>().getSocket();

           

            info = new Info();
            //        info.name = ID;

            //info.name = ob.GetComponent<ConnectServer>().getName();
            info.name = "Tester";
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

            //socket.Blocking = false;   // make socket non-blocking mode

            Debug.Log("Awake() is done");


            byte[] sizeArr;  // size byte array

            // total data size to send
            // "Start" + menu sel(char) + totalSize(short) + name length(short) + "name"
            header.totalSize = (short)(header.startSize + 1 + 2 + header.nameSize);

            // start + total Data Size + (short)nameSize + name + (short)posSize + pos + (short)rotSize + rot
            byte[] result = new byte[header.totalSize];

            startBuffer = Encoding.UTF8.GetBytes(header.start);

            // set start string and menu sel size into result buffer
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(header.start), 0, result, 0, header.startSize);
            result[header.startSize] = Convert.ToByte(header.single);
            // set (short)nameSize and nameBuffer into result buffer
            sizeArr = BitConverter.GetBytes(header.nameSize);

            Buffer.BlockCopy(sizeArr, 0, result, header.startSize + 1, 2);
            Buffer.BlockCopy(nameBuffer, 0, result, header.startSize + 1 + 2, nameBuffer.Length);

            //socket.Send(result);




			GameObject pos = null;
			int positionNum = (int)UnityEngine.Random.Range(1,5);
			Debug.Log ("PositionNum : " + positionNum);

			switch (positionNum)
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
			mapSeq = new int[8];
			for (int i = 0; i < 8; i++) {
				mapSeq [i] = UnityEngine.Random.Range (0, 9);
			}

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

        void sendCloseMessage()
        {
            Debug.Log("sendCloseMessage()");

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
            // (short)nameSize + name + (short) close size + close
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
        }

    }
}
