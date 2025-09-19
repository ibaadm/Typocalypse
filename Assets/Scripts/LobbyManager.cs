using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class LobbyManager : NetworkBehaviour {

    Lobby hostLobby;
    float lobbyHearbeatTimer = 15f;
    float updateLobbyTimer = 1.1f;
    public string lobbyCodeInput = "";
    public static LobbyManager instance;
    LobbyEventCallbacks callBacks = new LobbyEventCallbacks();
    bool shouldBeHost;

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
                hostLobby = await LobbyService.Instance.GetLobbyAsync(hostLobby.Id);
            }
            catch {
            }
        }
    }

    async void JoinLobby(string lobbyCode) {

        try {
            hostLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
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
    }
}
