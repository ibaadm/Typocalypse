using UnityEngine;
using System.Collections.Generic;
public class WorldGenerator : MonoBehaviour {

    [SerializeField] private Transform player;

    [Header("Roads")]
    [SerializeField] private GameObject road;
    [SerializeField] private int noOfRoadsLeft = 3;
    [SerializeField] private int noOfRoadsRight = 10;
    private List<GameObject> currentRoads = new List<GameObject>();
    private float lastUpdateX;

    void Start() {
        
        InitializeRoads();
    }

    void Update() {
        
        UpdateRoads();
    }

    // Spawn roads to the left and right of the player
    void InitializeRoads() {
        
        for (int i = 0; i < noOfRoadsLeft + noOfRoadsRight; i++) {
            currentRoads.Add(Instantiate(road, new Vector3
                (i - noOfRoadsLeft + player.position.x, player.position.y, 0), Quaternion.identity));
        }

        lastUpdateX = player.position.x;
    }

    // When the player moves enough, move the last road to the front
    void UpdateRoads() {
        
        if (player.position.x > lastUpdateX + 1) {

            currentRoads[0].transform.position = new Vector3
                (currentRoads[^1].transform.position.x + 1, currentRoads[0].transform.position.y, 0);
            lastUpdateX++;

            // Move the first road to the end of the list
            GameObject temp = currentRoads[0];
            currentRoads.RemoveAt(0);
            currentRoads.Add(temp);
        }
    }
}