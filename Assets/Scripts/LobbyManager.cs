using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using System.Linq;

public class LobbyManager : NetworkBehaviour {

    Lobby hostLobby;
    Lobby joinedLobby;
    float lobbyHearbeatTimer = 15f;
    float updateLobbyTimer = 1.1f;
    public static LobbyManager instance;
    [HideInInspector] public bool shouldBeHost;
    [SerializeField] bool relayCreated = false;
    [HideInInspector] public bool isRetrying = false;

    void Awake() {

        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    async void Start() {

        var options = new InitializationOptions();
        
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++) {
            if (args[i] == "-profile" && i + 1 < args.Length) {
                options.SetProfile(args[i + 1]);
            }
        }

        await UnityServices.InitializeAsync(options);

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    void Update() {

        ManageLobbyHeartbeat();
        UpdateLobby();
    }

    public async void CreateLobby() {

        if (joinedLobby != null || hostLobby != null) {
            try {
                string currentId = joinedLobby?.Id ?? hostLobby?.Id;
                await LobbyService.Instance.RemovePlayerAsync(currentId, AuthenticationService.Instance.PlayerId);
                Debug.Log("Had to leave a lobby through CreateLobby");
            }
            catch (LobbyServiceException e) {
                Debug.LogWarning($"Leaving lobby in CreateLobby failed (safe to ignore): {e.Message}");
            }
            relayCreated = false;
            joinedLobby = null;
            hostLobby = null;
        }
        try {
            string lobbyName = "myLobby";
            int maxPlayers = 2;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions {
                IsPrivate = true,
                Data = new Dictionary<string, DataObject> {
                    { "KEY_START_GAME", new DataObject(DataObject.VisibilityOptions.Member, "0")}
                }
            };

            hostLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            Debug.Log($"Created lobby! {hostLobby.Name} {hostLobby.MaxPlayers} {hostLobby.Id} {hostLobby.LobbyCode}");
            shouldBeHost = true;
        }
        catch (LobbyServiceException e) {
            Debug.Log($"Failed to create lobby: {e}");
        }
    }

    async void ManageLobbyHeartbeat() {
        if (lobbyHearbeatTimer > 0) {
            lobbyHearbeatTimer -= Time.unscaledDeltaTime;
        }
        else {
            lobbyHearbeatTimer = 15f;
            try {
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
            catch {
            }
        }
    }

    async void UpdateLobby() {
        if (updateLobbyTimer > 0) {
            updateLobbyTimer -= Time.unscaledDeltaTime;
        }
        else {
            updateLobbyTimer = 1.1f;
            try {
                if (shouldBeHost) {
                    if (hostLobby == null) return;

                    Lobby updatedLobby = await LobbyService.Instance.GetLobbyAsync(hostLobby.Id);
                    if (!shouldBeHost || hostLobby == null || hostLobby.Id != updatedLobby.Id) {
                        return;
                    }
                    hostLobby = updatedLobby;

                    if (hostLobby.Players.Count == 2 && !relayCreated) {
                        CreateRelay();
                    }

                }
                else if (joinedLobby != null) {
                    Lobby updatedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                    if (joinedLobby == null|| joinedLobby.Id != updatedLobby.Id) {
                        return;
                    }

                    joinedLobby = updatedLobby;
                    if (joinedLobby.Data["KEY_START_GAME"].Value != "0" && !relayCreated) {
                        JoinRelay(joinedLobby.Data["KEY_START_GAME"].Value);
                    }
                }
            }
            catch {}
        }
    }

    public string GetLobbyCode() {
        if (hostLobby != null) {
            return hostLobby.LobbyCode;
        }
        else {
            return "";
        }
    }

    public async Task JoinLobby(string lobbyCode) {

        if (joinedLobby != null || hostLobby != null) {
            string currentId = joinedLobby?.Id ?? hostLobby?.Id;
            await LobbyService.Instance.RemovePlayerAsync(currentId, AuthenticationService.Instance.PlayerId);
            relayCreated = false;
        }
        shouldBeHost = false;
        joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode[..6]);
        Debug.Log("joined lobby");
    }

    public bool IsMultiplayer() {
        if ((hostLobby != null && hostLobby.Players.Count > 1) ||
            (joinedLobby != null && joinedLobby.Players.Count > 1)) {
            return true;
        }
        
        return false;
    }

    async void CreateRelay() {
        try {
            Debug.Log("Relay created");
            relayCreated = true;
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            string connectionType = "dtls";
            bool isWebSocket = false;
            
            #if UNITY_WEBGL
                connectionType = "wss";
                isWebSocket = true;
                transport.UseWebSockets = true;
            #else
                transport.UseWebSockets = false;
            #endif

            var targetEndpoint = allocation.ServerEndpoints.First(conn => conn.ConnectionType == connectionType);
            Debug.Log($"[Selected Endpoint] {targetEndpoint.ConnectionType} {targetEndpoint.Host}:{targetEndpoint.Port} | WebSocket: {isWebSocket}");

            var relayServerData = new RelayServerData(
                targetEndpoint.Host,
                (ushort)targetEndpoint.Port,
                allocation.AllocationIdBytes,
                allocation.ConnectionData,
                new byte[0],
                allocation.Key,
                targetEndpoint.Secure,
                isWebSocket
            );

            transport.SetRelayServerData(relayServerData);

            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject> {
                    { "KEY_START_GAME", new DataObject(DataObject.VisibilityOptions.Member, joinCode)}
                }
            });

            SceneManager.LoadScene(2);
        }
        catch (RelayServiceException e) {
            Debug.Log(e);
        }
    }

    async void JoinRelay(string joinCode) {
        try {
            relayCreated = true;
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            string connectionType = "dtls";
            bool isWebSocket = false;
            
            #if UNITY_WEBGL
                connectionType = "wss";
                isWebSocket = true;
                transport.UseWebSockets = true;
            #else
                transport.UseWebSockets = false;
            #endif

            var targetEndpoint = joinAllocation.ServerEndpoints.First(conn => conn.ConnectionType == connectionType);
            Debug.Log($"[Joining Endpoint] {targetEndpoint.ConnectionType} {targetEndpoint.Host}:{targetEndpoint.Port} | WebSocket: {isWebSocket}");

            var relayServerData = new RelayServerData(
                targetEndpoint.Host,
                (ushort)targetEndpoint.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData,
                joinAllocation.Key,
                targetEndpoint.Secure,
                isWebSocket
            );

            transport.SetRelayServerData(relayServerData);

            Debug.Log("Joined relay");
            SceneManager.LoadScene(2);
        }
        catch (RelayServiceException e) {
            Debug.Log(e);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DisconnectServerRpc() {
        if (IsHost) {
            HostDisconnecting();
        }
    }
    async void HostDisconnecting() {
        hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions {
            Data = new Dictionary<string, DataObject> {
                { "KEY_START_GAME", new DataObject(DataObject.VisibilityOptions.Member, "0")}
            }
        });
        if (!isRetrying) {
            await LobbyService.Instance.RemovePlayerAsync(hostLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        NetworkManager.Singleton.Shutdown();
    }

    public override void OnNetworkDespawn() {
        if (isRetrying) {
            relayCreated = false;
            isRetrying = false;
        }
    }

    public async void MenuCleanup() {

        if (joinedLobby != null) {
            try {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            }
            catch (LobbyServiceException e) {
                Debug.Log($"Client leave error (ignoring): {e.Message}"); 
            }
            joinedLobby = null;
        }

        isRetrying = false;
        relayCreated = false;
        hostLobby = null;
        shouldBeHost = false;
        SceneManager.LoadScene("MenuScene");
    }
}
