using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour {

    Lobby hostLobby;
    Lobby joinedLobby;
    float lobbyHearbeatTimer = 15f;
    float updateLobbyTimer = 1.1f;
    public string lobbyCodeInput = "";
    public static LobbyManager instance;
    LobbyEventCallbacks callBacks = new LobbyEventCallbacks();
    bool shouldBeHost;
    bool relayCreated = false;
    bool inDuelScene = false;

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
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    void Update() {

        if (Keyboard.current.tabKey.wasPressedThisFrame) {
            CreateLobby();
        }
        if (Keyboard.current.rightShiftKey.wasPressedThisFrame) {
            JoinLobby(lobbyCodeInput);
        }
        if (Keyboard.current.rightCtrlKey.wasPressedThisFrame) {
            print(hostLobby.Players.Count);
        }

        ManageLobbyHeartbeat();
        UpdateLobby();
    }

    async void CreateLobby() {

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
                        Debug.Log("Client joined");
                        CreateRelay();
                    }
                }
                else {
                    joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                    if (joinedLobby.Data["KEY_START_GAME"].Value != "0" && !relayCreated) {
                        JoinRelay(joinedLobby.Data["KEY_START_GAME"].Value);
                    }
                    if (IsClient && !inDuelScene) {
                        LoadDuelSceneServerRpc();
                        inDuelScene = true;
                    }
                }
            }
            catch {
            }
        }
    }

    async void JoinLobby(string lobbyCode) {

        try {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            Debug.Log("should be joined");
            shouldBeHost = false;
        }
        catch {
            Debug.Log("join error");
        }

    }

    async void LeaveLobby() {
        try {
            await LobbyService.Instance.RemovePlayerAsync(hostLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch {
            Debug.Log("error leaving lobby");
        }
        // need to leave relay too? should change relayCreated to false in that case
    }

    async void CreateRelay() {
        try {
            relayCreated = true;
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
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
            NetworkManager.Singleton.StartHost();
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
            transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e) {
            Debug.Log(e);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void LoadDuelSceneServerRpc() {
        NetworkManager.Singleton.SceneManager.LoadScene("DuelScene", LoadSceneMode.Single);
        Debug.Log("switch scenes called");
    }
}
