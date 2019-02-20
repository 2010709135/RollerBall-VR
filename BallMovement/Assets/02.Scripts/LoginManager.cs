using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Net.Sockets;  
using System.Text;  
using UnityEngine.SceneManagement;

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


public class LoginManager : MonoBehaviour {
	public InputField ID_input;
	public InputField Password_input;
	public Canvas alter;

	string ID;
	string password;
	Socket socket;

	byte[] otherNetBuffer;
	byte[] recoBuffer;

	public GameObject connectManager;
	ConnectServer cs;

	Header tempHeader;

	string OK = "OK";
	string NO = "NO";

	byte[] startBuffer;
	byte[] result;

	// Use this for initialization
	void Start () {
		otherNetBuffer = new byte[1024];
		recoBuffer = new byte[1024];

		connectManager = GameObject.FindGameObjectWithTag("CONNECT");

		if (connectManager == null)
		{
			connectManager = (GameObject)Instantiate(Resources.Load("ConnectManager"));
		}
		cs = connectManager.GetComponent<ConnectServer> ();
		socket = cs.socket;

		tempHeader = new Header ();
		startBuffer = new byte[tempHeader.startSize];
	}

	void typingComplete(){
		connectManager.GetComponent<ConnectServer>().sendData();
	}



	public void onClickOK(){
		ID = ID_input.text;
		password = Password_input.text;

		cs.name = ID;
		cs.password = password;

		while (true) {
			cs.sendData ();
			byte[] buf = new byte[2];

			try {
				int recvSize = socket.Receive(otherNetBuffer);

				// read start string 
				recoBuffer = new byte[tempHeader.startSize];
				Buffer.BlockCopy(otherNetBuffer, 0, recoBuffer, 0, tempHeader.startSize);

				tempHeader.start = Encoding.Default.GetString(recoBuffer);
				if (cs.header.start.Equals(tempHeader.start))
				{
					// read total data size 
					recoBuffer = new byte[2];
					Buffer.BlockCopy(otherNetBuffer, tempHeader.startSize, recoBuffer, 0, 2);

					tempHeader.totalSize = BitConverter.ToInt16(recoBuffer, 0);

					// if read data is less than total data, skip below
					if (recvSize >= tempHeader.totalSize)
					{
						recoBuffer = new byte[2];
						Buffer.BlockCopy(otherNetBuffer, tempHeader.startSize + 2, recoBuffer, 0, 2);

						string IsOK;

						IsOK = Encoding.Default.GetString(recoBuffer);

						if(IsOK == OK){
                            moveToLobbyScene();
                            break;
						}else if(IsOK == NO)
                        {
                            alter.GetComponent<Canvas>().enabled = true;
                            break;
                        }
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
	}

	void moveToLobbyScene(){
		connectManager.GetComponent<ConnectServer>().MakeAliveScene();
		SceneManager.LoadSceneAsync("BallLobby");
	}

	public void onClickAccept(){
		alter.GetComponent<Canvas>().enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
	
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
}
