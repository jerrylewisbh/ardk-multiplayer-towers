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
    private GameObject Button; //Start Game Button
    [SerializeField]
    private GameObject Canvas; //Referene to the UI (Join button, Session ID)

    private IARNetworking arNetworking; //Reference to the AR Networking object

    //Reference to the game host on the networks
    private IPeer host;
    //Reference to the local player on the network
    private IPeer self;

    //if the local player is the host or not
    private bool isHost;

    //Prefab that represents each player on the scene
    [SerializeField]
    private GameObject peerAvatar;

    private bool gameStarted = false;

    [SerializeField]
    private GameObject boardPrefab;

    private GameObject board;

    [SerializeField]
    private GameObject endGame;
    [SerializeField]
    private Text endText;

    /// Hash-map of game objects (spheres are drawn per peer)
    private Dictionary<IPeer, GameObject> peerGameObjects = new Dictionary<IPeer, GameObject>();
    //Hash-map with every peer and its latest status 
    private Dictionary<IPeer, PeerState> peerStates = new Dictionary<IPeer, PeerState>();

    // Start is called before the first frame update
    void Start()
    {
    }


    //Called my the join button 
    public void Join()
    {
        //Listens to the Network Session being initialized 
        ARNetworkingFactory.ARNetworkingInitialized += OnARNetworkingSessionInitialized;
        //Enables the networking manager
        aRNetworkingManager.enabled = true;
    }


    void OnARNetworkingSessionInitialized(AnyARNetworkingInitializedArgs args)
    {
        //stores a reference to the AR Networking
        arNetworking = args.ARNetworking;


        //SUBSCRIBING TO NETWORKING CALLBACK EVENTS

        arNetworking.Networking.Connected += OnConnected; //When the local player connects to the network

        arNetworking.Networking.PeerAdded += OnPeerAdded; //when a new peer is added 


        arNetworking.PeerPoseReceived += OnPeerPoseReceived; //when a peer position changes 

        arNetworking.PeerStateReceived += OnPeerStateReceived; //when a peer state changes

        arNetworking.Networking.PeerDataReceived += OnPeerDataReceived; //when a peer sends some data

    }


    //CALLBACK DEFINITIONS
    private void OnConnected(ConnectedArgs args)
    {
        self = args.Self;
        host = args.Host;
        isHost = args.IsHost;
        Debug.Log("Is Host " + isHost);
    }
    private void OnPeerAdded(PeerAddedArgs args)
    {
        // Instantiating peer object on a random location (the location will be updated when the peer moves)
        peerGameObjects[args.Peer] =
          Instantiate
          (
            peerAvatar,
            new Vector3(99999, 99999, 99999),
            Quaternion.identity
          );
    }
    private void OnPeerPoseReceived(PeerPoseReceivedArgs args)
    {
        var peer = args.Peer;
        var peerGameObject = peerGameObjects[peer];

        var pose = args.Pose;
        peerGameObject.transform.position = pose.ToPosition();
        peerGameObject.transform.rotation = pose.ToRotation();
    }
    private void OnPeerStateReceived(PeerStateReceivedArgs args)
    {
        var peer = args.Peer;
        var syncPeerState = args.State;

        //Stores the peer state
        peerStates[peer] = syncPeerState;

        Debug.Log("Peer Id: " + peer.Identifier.ToString() + " " + syncPeerState);

        //everytime a player state changes, determine if the game is ready to start
        VerifyStartLogic();

    }
    private void OnPeerDataReceived(PeerDataReceivedArgs args)
    {

        Debug.Log("Data Received");

        //Check the tag to determine what kind of data was received
        if (args.Tag == 0) //0 was the arbitrary tag we defined to represent "Instantiate the board"
        {

            //Desserialize the board position
            byte[] bytes = args.CopyData();
            string vector = Encoding.ASCII.GetString(bytes);
            string[] parts = vector.Split('|');

            Vector3 vector3 = new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));

            board = Instantiate(boardPrefab, vector3, Quaternion.identity);
        }

        else if (args.Tag == 1) //1 is the arbitraty tag we defined to represent that the game ended 
        {
            Time.timeScale = 0; //pauses the time, so input and phisics are also paused

            endGame.SetActive(true); //enable the interface 


            //Update the interface to tell who won
            Debug.Log("There's a winner");
            if (args.Peer == self)
            {
                endText.text = "YOU WON!";
            }
            else
            {
                endText.text = "YOU LOST!";
            }
        }
    }
    private void VerifyStartLogic()
    {

        Debug.Log("Peer status count: " + peerStates.Count);

        //only the host checks the logic
        if (!isHost)
            return;

        //needs 2 players to start
        if (peerStates.Count < 2)
            return;

        //all palyers in the list must be in the Stable state
        if (peerStates.Values.All(item => item == PeerState.Stable))
        {
            Button.SetActive(true);
        }
    }
    public void StartGame()
    {
        Canvas.SetActive(false);
        gameStarted = true;

    }
    public bool GetIsHost()
    {
        return isHost;
    }
    public void NotifyVictory()
    {
        //Notify all players in the netwrok that the game ended
        arNetworking.Networking.BroadcastData(1, self.Identifier.ToByteArray(), TransportType.ReliableOrdered, true);
    }
    // Update is called once per frame
    void Update()
    {
        //if the game was started and there is no board yet
        if (gameStarted && board == null && isHost)
        {

            //check if there is any touch on the screen
            if (PlatformAgnosticInput.touchCount > 0)
            {
                Touch touch = PlatformAgnosticInput.GetTouch(0);

                //Get the current camera frame
                var currentFrame = arNetworking.ARSession.CurrentFrame;
                if (currentFrame == null)
                    return;

                //trace a ray from touch position in the direction of the mapped planes
                var results =
                  currentFrame.HitTest
                  (
                    Camera.main.pixelWidth,
                    Camera.main.pixelHeight,
                    touch.position,
                    ARHitTestResultType.ExistingPlaneUsingExtent
                  );

                //if no surfaces were found, skip
                if (results.Count <= 0)
                {
                    Debug.Log("Unable to place the field at the chosen location. Can't find a valid surface");
                    return;
                }

                // Get the closest point on the plane
                var result = results[0];
                var hitPosition = result.WorldTransform.ToPosition();

                //Create the board (on the host only)
                board = Instantiate(boardPrefab, hitPosition, Quaternion.identity);

                //serialize the board position to send it through the networking
                string vector = $"{hitPosition.x}|{hitPosition.y}|{hitPosition.z}";
                byte[] bytes = Encoding.ASCII.GetBytes(vector);

                //send the chosen board position to the peers
                arNetworking.Networking.BroadcastData(0, bytes, TransportType.UnreliableUnordered);


            }

        }
    }

}
