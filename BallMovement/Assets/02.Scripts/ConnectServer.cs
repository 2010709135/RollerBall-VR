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

public class Info
{
    public string name;
    public Vector3 pos;
    public Quaternion rot;
}

public class Header
{
    public string start;
    public string close;
    public char ID;
    public char single;
    public char multi;
    public char rank;
    public char exit;

    public short startSize;
    public short closeSize;
    public short nameSize;
    public short totalSize;
    public short posSize;
    public short rotSize;
}

public class ConnectServer : MonoBehaviour
{

    public Socket socket;

    public new string name; // My ID
	public string password;
    public Header header;

    public Info info;

    byte[] startBuffer;
    byte[] nameBuffer;

    public const int BUF_SIZE = 1024;
    public const char ID = (char)0;
    public const char SINGLE = (char)1;
    public const char MULTI = (char)2;
    public const char RANK = (char)3;
    public const char EXIT = (char)4;
    
    public bool VRMode = true;

    // Use this for initialization
    void Awake()
    {
		setVRMode (false);

        IPAddress ip = IPAddress.Parse("192.168.0.9");
        int port = 9001;

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(ip, port);

    }

    private void Start()
    {
        header = new Header();

        header.start = "Start";
        header.close = "Close";
        header.ID = ID;
        header.single = SINGLE;
        header.multi = MULTI;
        header.rank = RANK;
        header.exit = EXIT;
    }
    // Update is called once per frame
    void Update()
    {

    }

    public Socket getSocket()
    {
        return socket;
    }

    public String getName()
    {
        return info.name;
    }
       
    public void MakeAliveScene()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void sendData()
    {
        info = new Info();
        info.name = name;

        // set Packet header
        header.startSize = (short)header.start.Length;
        header.nameSize = (short)info.name.Length;

        byte[] sizeArr;  // size byte array for sending message


        // total data size to send
        // "Start" + menu sel(char) + name length(short) + "name"
        header.totalSize = (short)(header.startSize + 1 + 2 + header.nameSize);

        // start + total Data Size + (short)nameSize + name 
        //             + (short)posSize + pos + (short)rotSize + rot
        byte[] result = new byte[header.totalSize];

        // "Start" and name String to Bytes
        startBuffer = Encoding.UTF8.GetBytes(header.start);
        nameBuffer = Encoding.UTF8.GetBytes(info.name);

        // set start string and menu sel size into result buffer
        Buffer.BlockCopy(startBuffer, 0, result, 0, header.startSize);
        result[header.startSize] = Convert.ToByte(header.ID);
        // set (short)nameSize and nameBuffer into result buffer
        sizeArr = BitConverter.GetBytes(header.nameSize);

        Buffer.BlockCopy(sizeArr, 0, result, header.startSize + 1, 2);
        Buffer.BlockCopy(nameBuffer, 0, result, header.startSize + 1 + 2, nameBuffer.Length);

        socket.Send(result); // send to server
    }

    public void setVRMode(bool sel)
    {
        VRMode = sel;
    }

    public bool getVRMode()
    {
        return VRMode;
    }
}
