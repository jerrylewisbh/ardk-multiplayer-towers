using System.Collections;
using System.Collections.Generic;
using Niantic.ARDK.Utilities.Input.Legacy;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private Transform spawnPoint;
    GameManager gameManager;

    [SerializeField]
    private GameObject block;
    // Start is called before the first frame update
    void Start()
    {
        //get a reference to the game manager
        gameManager = FindObjectOfType<GameManager>();

        Debug.Log("Start Board: " + gameManager.GetIsHost());
       
        //determine where the cubes will be spawned from
        if (gameManager.GetIsHost())
        {
            spawnPoint = GameObject.FindWithTag("Player1").transform;
        }
        else
        {
            spawnPoint = GameObject.FindWithTag("Player2").transform;
        }
    }

    // Update is called once per frame
    void Update()
    {

        Debug.Log("PlatformAgnosticInput.touchCount: " + PlatformAgnosticInput.touchCount);
       
        
        //create a new cube on the touch position
        if (PlatformAgnosticInput.touchCount > 0)
        {
            Touch touch = PlatformAgnosticInput.GetTouch(0);
            Debug.Log("Touch Phase: " + touch.phase);

            if(touch.phase == TouchPhase.Began)
            {
                Instantiate(block, spawnPoint.position, spawnPoint.rotation);
                Debug.Log("Creating Object");
            }
        }
    }
}
