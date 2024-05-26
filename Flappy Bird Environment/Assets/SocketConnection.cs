using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SocketConnection : MonoBehaviour
{
    [Header("Socket")]
    private const string host = "127.0.0.1"; // localhost
    private const int port = 12345;
    TcpClient client;
    NetworkStream stream;
    private Thread receiveThread;
    private bool isRunning = true;
    static SocketConnection instance;
    public UnityMainThreadDispatcher umtd;

    [Header("Environment")]
    public Transform bird;
    public PipeSpawner spawner;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 30;
        Time.timeScale = 1;
        resetEnv();
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            ConnectToServer();

        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ConnectToServer()
    {
        try
        {
            client = new TcpClient(host, port);
            stream = client.GetStream();

            // Start the receive thread
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception: {e.Message}");
        }
    }

    void ReceiveData()
    {
        Debug.Log("Thread started!");
        byte[] data = new byte[1024];
        while (isRunning)
        {
            try
            {
                int bytesRead = stream.Read(data, 0, data.Length);
                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(data, 0, bytesRead);
                    if (message == "get_state")
                    {
                        // Enqueue the getItems call to be executed on the main thread
                        umtd.Enqueue(() => {
                            string toSend = "";
                            toSend = bird.gameObject.GetComponent<Bird>().envData();
                            byte[] dataToSend = Encoding.UTF8.GetBytes(toSend);
                            stream.Write(dataToSend, 0, dataToSend.Length);
                        });
                    }
                    else if (message.Contains("play_step")) //recieve: play_step:STEP
                                                            //send: REWARD:DONE:OBS_
                    {
                        string[] step = message.Split(':');
                        umtd.Enqueue(() => {
                            string s = "";
                            s = playStep(int.Parse(step[1]));
                            s += bird.gameObject.GetComponent<Bird>().envData();
                            Debug.Log(s);   
                            byte[] dataToSend = Encoding.UTF8.GetBytes(s);
                            stream.Write(dataToSend, 0, dataToSend.Length);

                        });
                    }
                    if (message == "reset")
                    {
                        umtd.Enqueue(() =>
                        {
                            resetEnv();
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception: {e.Message}");
            }
        }
    }

    public void resetEnv()
    {
        GameObject[] pipes = GameObject.FindGameObjectsWithTag("Pipe");

        // Loop through the array and destroy each object
        foreach (GameObject pipe in pipes)
        {
            Destroy(pipe);
        }

        bird.gameObject.GetComponent<Bird>().hasHitPipe = false;
        bird.gameObject.transform.position = new Vector3(-7, -1, 0);
        spawner.time = 7;

    }

    // Update is called once per frame
    public string sendInput()
    {
        string toSend = bird.gameObject.GetComponent<Bird>().envData();


        return toSend;
    }

    public string playStep(int action)
    {
        Bird cc = bird.gameObject.GetComponent<Bird>();
        
        return cc.playAction(action);

    }
}
