using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using System.Collections;
using Unity.Netcode;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class LobbyManager : NetworkBehaviour {

    Lobby hostLobby;
    Lobby joinedLobby;
    float lobbyHearbeatTimer = 15f;
    float updateLobbyTimer = 1.1f;
    public static LobbyManager instance;
    LobbyEventCallbacks callBacks = new LobbyEventCallbacks();
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

        CreateLobby();
    }

    void Update() {

        ManageLobbyHeartbeat();
        UpdateLobby();
    }

    async void CreateLobby() {

        try {
            if (joinedLobby != null || hostLobby != null) {
                string currentId = joinedLobby?.Id ?? hostLobby?.Id;
                await LobbyService.Instance.RemovePlayerAsync(currentId, AuthenticationService.Instance.PlayerId);
                relayCreated = false;
                Debug.Log("Had to leave a lobby through CreateLobby");
            }
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
            Debug.Log(e);
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
                    hostLobby = await LobbyService.Instance.GetLobbyAsync(hostLobby.Id);
                    if (hostLobby.Players.Count == 2 && !relayCreated) {
                        CreateRelay();
                    }

                }
                else {
                    joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                    if (joinedLobby.Data["KEY_START_GAME"].Value != "0" && !relayCreated) {
                        JoinRelay(joinedLobby.Data["KEY_START_GAME"].Value);
                    }
                }
            }
            catch {
            }
        }
    }

    public string GetLobbyCode() {
        return hostLobby.LobbyCode;
    }

    public async Task JoinLobby(string lobbyCode) {

        if (joinedLobby != null || hostLobby != null) {
            string currentId = joinedLobby?.Id ?? hostLobby?.Id;
            await LobbyService.Instance.RemovePlayerAsync(currentId, AuthenticationService.Instance.PlayerId);
            relayCreated = false;
        }
        shouldBeHost = false;
        joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode[..6]);
        Debug.Log("should be joined");
    }

    async void CreateRelay() {
        try {
            Debug.Log("Relay created");
            relayCreated = true;
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );
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
            Debug.Log("Tried joining relay");
            relayCreated = true;
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );
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

        CreateLobby();
    }


}
