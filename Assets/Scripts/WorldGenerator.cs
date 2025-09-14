using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class WorldGenerator : NetworkBehaviour {

    [SerializeField] private Transform playerBlue;
    private Transform playerRed;

    [Header("Roads")]
    [SerializeField] private GameObject road;
    [SerializeField] private int noOfRoadsLeft = 3;
    [SerializeField] private int noOfRoadsRight = 10;
    private List<GameObject> currentRoads = new List<GameObject>();
    private float lastRoadUpdateX;

    [Header("Zombie Hands")]
    [SerializeField] private GameObject zombieHands;
    [SerializeField] private float distanceBetweenZombieHands = 2f;
    [SerializeField] private int noOfZombieHands = 10;
    [SerializeField] private float zombieHandInitialSpawnOffset = 2.5f;
    private List<GameObject> currentZombieHands = new List<GameObject>();
    private float lastZombieHandsUpdateX;


    void Start() {
        
        InitializeRoads();
        InitializeZombieHands();
    }

    // Handle network changes
    public override void OnNetworkSpawn() {

        playerRed = GameObject.FindWithTag("Player Red").transform;

        if (!IsHost) { return; }
        for (int i = 0; i < noOfZombieHands; i++) {
            currentZombieHands.Add(Instantiate(zombieHands, new Vector3
                ((i + 1) * distanceBetweenZombieHands + playerBlue.position.x, playerBlue.position.y, 0), Quaternion.identity));
            currentZombieHands[i].GetComponent<NetworkObject>().Spawn();
        }
        lastZombieHandsUpdateX = playerBlue.position.x + zombieHandInitialSpawnOffset;
    }

    void Update() {
        
        UpdateRoads();
        UpdateZombieHands();
    }

    // Spawn roads to the left and right of the player
    void InitializeRoads() {
        
        for (int i = 0; i < noOfRoadsLeft + noOfRoadsRight; i++) {
            currentRoads.Add(Instantiate(road, new Vector3
                (i - noOfRoadsLeft + playerBlue.position.x, playerBlue.position.y, 0), Quaternion.identity));
        }

        lastRoadUpdateX = playerBlue.position.x;
    }

    // Spawn zombie hands in front of the player, instantiate when server starts if dueling
    void InitializeZombieHands() { if (FindAnyObjectByType<DuelManager>() != null) { return; }

        for (int i = 0; i < noOfZombieHands; i++) {
            currentZombieHands.Add(Instantiate(zombieHands, new Vector3
                ((i + 1) * distanceBetweenZombieHands + playerBlue.position.x, playerBlue.position.y, 0), Quaternion.identity));
        }

        // Allow the zombie hands to spawn in front of the player
        // But teleport to the front after it fully passes by the player
        lastZombieHandsUpdateX = playerBlue.position.x + zombieHandInitialSpawnOffset;
    }

    // When the player moves enough, move the last road to the front
    void UpdateRoads() {
        
        float redX = playerRed != null ? playerRed.position.x : float.MaxValue;
        if (Mathf.Min(playerBlue.position.x, redX) > lastRoadUpdateX + 1) {

            currentRoads[0].transform.position = new Vector3
                (currentRoads[^1].transform.position.x + 1, currentRoads[0].transform.position.y, 0);
            lastRoadUpdateX++;

            // Move the first road to the end of the list
            GameObject temp = currentRoads[0];
            currentRoads.RemoveAt(0);
            currentRoads.Add(temp);
        }
    }

    // When the player moves enough, move the last zombie hand to the front
    void UpdateZombieHands() { if (IsClient && !IsHost) { return; }

        float redX = playerRed != null ? playerRed.position.x : float.MaxValue;
        if (Mathf.Min(playerBlue.position.x, redX) > lastZombieHandsUpdateX + distanceBetweenZombieHands) {

            currentZombieHands[0].transform.position = new Vector3
                (currentZombieHands[^1].transform.position.x + distanceBetweenZombieHands, currentZombieHands[0].transform.position.y, 0);
            lastZombieHandsUpdateX += distanceBetweenZombieHands;

            // Move the first zombie hand to the end of the list
            GameObject temp = currentZombieHands[0];
            currentZombieHands.RemoveAt(0);
            currentZombieHands.Add(temp);
        }
    }
}