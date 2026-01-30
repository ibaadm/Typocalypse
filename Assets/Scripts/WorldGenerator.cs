using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using System.Linq;

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

    public override void OnNetworkSpawn() {

        playerRed = GameObject.FindWithTag("Player Red").transform;
        lastZombieHandsUpdateX = playerBlue.position.x + zombieHandInitialSpawnOffset;

        if (!IsHost) {
            StartCoroutine(InitializeZombieHandsClient());
            return;
        }

        for (int i = 0; i < noOfZombieHands; i++) {
            currentZombieHands.Add(Instantiate(zombieHands, new Vector3
                ((i + 1) * distanceBetweenZombieHands + playerBlue.position.x, playerBlue.position.y, 0), Quaternion.identity));
            currentZombieHands[i].GetComponent<NetworkObject>().Spawn();
        }
    }

    IEnumerator InitializeZombieHandsClient() {

        while (currentZombieHands.Count != 3) {
            yield return new WaitForSeconds(0.2f);

            ZombieHands[] list = FindObjectsByType<ZombieHands>(FindObjectsSortMode.None);
            foreach (ZombieHands zombieHand in list) {
                if (!currentZombieHands.Contains(zombieHand.gameObject)) {
                    currentZombieHands.Add(zombieHand.gameObject);
                }
            }
        }
        currentZombieHands = currentZombieHands.OrderBy(obj => obj.transform.position.x).ToList();
    }

    void Update() {
        
        UpdateRoads();
        UpdateZombieHands();
    }

    void InitializeRoads() {
        
        for (int i = 0; i < noOfRoadsLeft + noOfRoadsRight; i++) {
            currentRoads.Add(Instantiate(road, new Vector3
                (i - noOfRoadsLeft + playerBlue.position.x, playerBlue.position.y, 0), Quaternion.identity));
        }

        lastRoadUpdateX = playerBlue.position.x;
    }

    void InitializeZombieHands() { if (GameObject.FindWithTag("Player Red")) { return; }

        for (int i = 0; i < noOfZombieHands; i++) {
            currentZombieHands.Add(Instantiate(zombieHands, new Vector3
                ((i + 1) * distanceBetweenZombieHands + playerBlue.position.x, playerBlue.position.y, 0), Quaternion.identity));
        }

        lastZombieHandsUpdateX = playerBlue.position.x + zombieHandInitialSpawnOffset;
    }

    void UpdateRoads() {
        
        float redX = playerRed != null ? playerRed.position.x : float.MaxValue;
        if (Mathf.Min(playerBlue.position.x, redX) > lastRoadUpdateX + 1) {

            currentRoads[0].transform.position = new Vector3
                (currentRoads[^1].transform.position.x + 1, currentRoads[0].transform.position.y, 0);
            lastRoadUpdateX++;

            GameObject temp = currentRoads[0];
            currentRoads.RemoveAt(0);
            currentRoads.Add(temp);
        }
    }

    void UpdateZombieHands() {

        float redX = playerRed != null ? playerRed.position.x : float.MaxValue;
        if (Mathf.Min(playerBlue.position.x, redX) > lastZombieHandsUpdateX + distanceBetweenZombieHands) {
            currentZombieHands[0].transform.position = new Vector3
                (currentZombieHands[^1].transform.position.x + distanceBetweenZombieHands, currentZombieHands[0].transform.position.y, 0);
            lastZombieHandsUpdateX += distanceBetweenZombieHands;

            GameObject temp = currentZombieHands[0];
            currentZombieHands.RemoveAt(0);
            currentZombieHands.Add(temp);
        }
    }
}