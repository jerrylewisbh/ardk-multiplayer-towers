using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryDetection : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        GameManager gm = FindObjectOfType<GameManager>();
        gm.NotifyVictory();
    }
}
