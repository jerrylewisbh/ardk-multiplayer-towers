using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Niantic.ARDK.AR.Networking;
using Niantic.ARDK.AR.Networking.ARNetworkingEventArgs;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.Networking;
using Niantic.ARDK.Networking.MultipeerNetworkingEventArgs;
using Niantic.ARDK.Utilities;
using System.Linq;
using Niantic.ARDK.Utilities.Input.Legacy;
using Niantic.ARDK.AR.HitTest;
using System.Text;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    private ARNetworkingManager aRNetworkingManager;


    [SerializeField]
    private GameObject Button;
    [SerializeField]
    private GameObject Canvas;

    private IARNetworking arNetworking;

    private IPeer host;
    private IPeer self;
    private bool isHost;

    [SerializeField]
    private GameObject peerAvatar;

    private bool gameStarted = false;

    [SerializeField]
    private GameObject fieldPrefab;


    private GameObject field;

    [SerializeField]
    private GameObject endGame;
    [SerializeField]
    private Text endText;

    /// Hash-maps of game objects (cubes are drawn per peer)
    private Dictionary<IPeer, GameObject> peerGameObjects = new Dictionary<IPeer, GameObject>();
    private Dictionary<IPeer, PeerState> peerStates = new Dictionary<IPeer, PeerState>();

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Join()
    {
        ARNetworkingFactory.ARNetworkingInitialized += OnARNetworkingSessionInitialized;
        aRNetworkingManager.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(gameStarted && field == null)
        {

            if (PlatformAgnosticInput.touchCount > 0)
            {
                Touch touch = PlatformAgnosticInput.GetTouch(0);
                var currentFrame = arNetworking.ARSession.CurrentFrame;
                if (currentFrame == null)
                    return;

                var results =
                  currentFrame.HitTest
                  (
                    Camera.main.pixelWidth,
                    Camera.main.pixelHeight,
                    touch.position,
                    ARHitTestResultType.ExistingPlaneUsingExtent
                  );

                if (results.Count <= 0)
                {
                    Debug.Log("Unable to place the field at the chosen location. Can't find a valid surface");
                    return;
                }

                // Get the closest result
                var result = results[0];
                var hitPosition = result.WorldTransform.ToPosition();

                field = Instantiate(fieldPrefab, hitPosition, Quaternion.identity);


                string vector = $"{hitPosition.x}|{hitPosition.y}|{hitPosition.z}";

                byte[] bytes = Encoding.ASCII.GetBytes(vector);

                arNetworking.Networking.BroadcastData(0, bytes, TransportType.UnreliableUnordered);


            }

        }
    }

     void OnARNetworkingSessionInitialized(AnyARNetworkingInitializedArgs args)
    {
        arNetworking = args.ARNetworking;
        arNetworking.Networking.Connected += OnConnected;

        arNetworking.Networking.PeerAdded += OnPeerAdded;

        arNetworking.Networking.PeerRemoved += PeerRemoved;

        arNetworking.PeerPoseReceived += OnPeerPoseReceived;

        arNetworking.PeerStateReceived += OnPeerStateReceived;

        arNetworking.Networking.PeerDataReceived += OnPeerDataReceived;
        
    }

    private void OnPeerDataReceived(PeerDataReceivedArgs args)
    {

        Debug.Log("Data Received");

        if(args.Tag == 0)
        {
            byte[] bytes = args.CopyData();
            string vector = Encoding.ASCII.GetString(bytes);

            string[] parts = vector.Split('|');

            Vector3 vector3 = new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));

            field = Instantiate(fieldPrefab, vector3, Quaternion.identity);
        }

        else if(args.Tag == 1)
        {
            Time.timeScale = 0;

            endGame.SetActive(true);


            Debug.Log("There's a winner");
            if(args.Peer == self)
            {
                endText.text = "YOU WON!";
            }
            else
            {
                endText.text = "YOU LOST!";
            }
        }
    }

    private void PeerRemoved(PeerRemovedArgs args)
    {
        Debug.Log("Peer Left: " + args.Peer.Identifier);
        if (peerStates.ContainsKey(args.Peer))
        {
            peerStates.Remove(args.Peer);
        }
        
    }

    private void OnPeerStateReceived(PeerStateReceivedArgs args)
    {
        var peer = args.Peer;
        var syncPeerState = args.State;

        peerStates[peer] = syncPeerState;
        Debug.Log("Peer Id: " + peer.Identifier.ToString() + " " + syncPeerState);

        UpdateGameStatus();


    }

    private void OnPeerPoseReceived(PeerPoseReceivedArgs args)
    {
        var peer = args.Peer;
        var peerGameObject = peerGameObjects[peer];

        var pose = args.Pose;
        peerGameObject.transform.position = pose.ToPosition();
        peerGameObject.transform.rotation = pose.ToRotation();
    }

    private void OnPeerAdded(PeerAddedArgs args)
    {
        // Instantiating peer object
        peerGameObjects[args.Peer] =
          Instantiate
          (
            peerAvatar,
            new Vector3(99999, 99999, 99999),
            Quaternion.identity
          );


    }

    private void OnConnected(ConnectedArgs args)
    {
        self = args.Self;
        host = args.Host;
        isHost = args.IsHost;
        Debug.Log("Is Host " + isHost);
    }

    private void UpdateGameStatus()
    {


        Debug.Log("Peer status count: " + peerStates.Count);

        if (!isHost)
            return;

        if (peerStates.Count < 2)
            return;

     

        if(peerStates.Values.All(item => item == PeerState.Stable))
        {
            Button.SetActive(true);
        }
    }

    public void StartGame()
    {
        Canvas.SetActive(false);
        gameStarted = true;

    }

    private void OnApplicationQuit()
    {
        if (arNetworking != null)
        {
            arNetworking.Networking.Leave();
        }
        Debug.Log("On Application Quit");
    }

    public bool GetIsHost()
    {
        return isHost;
    }

    public void NotifyVictory()
    {
        arNetworking.Networking.BroadcastData(1, self.Identifier.ToByteArray(), TransportType.ReliableOrdered, true);
    }
}
