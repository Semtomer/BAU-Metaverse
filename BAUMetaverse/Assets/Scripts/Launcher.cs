
using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections.Generic;
using Photon.Realtime;
using System.Linq;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_InputField roomPasswordInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] GameObject playerListItemPrefab;
    [SerializeField] GameObject startGameButton;

    [SerializeField] bool isAdmin = false;
    [SerializeField] GameObject createRoomButton;

    Hashtable roomProperty = new Hashtable();
    RoomOptions roomOptions = new RoomOptions();

    [SerializeField] Text createRoomErrorText;
    [SerializeField] GameObject errorBackground;

    [SerializeField] TMP_InputField nicknameInputField;
    [SerializeField] Text nicknameErrorText;
    [SerializeField] GameObject nicknameErrorBackground;

    Hashtable customProperties;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Connecting to Master");
        PhotonNetwork.ConnectUsingSettings();

        customProperties = new Hashtable();     
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        createRoomButton.SetActive(isAdmin);
        Debug.Log("Joined to Lobby");
        
        if (PhotonNetwork.NickName == "" || PhotonNetwork.NickName == null) 
            MenuManager.Instance.OpenMenu("nickname");
        else
            MenuManager.Instance.OpenMenu("title");

        //PhotonNetwork.NickName = "Player " + Random.Range(1, 1001).ToString("0000");
        ChangeCharacterSelection("Joe");
    }

    public void SetNickname()
    {
        if (string.IsNullOrEmpty(nicknameInputField.text))
        {
            if (!nicknameErrorBackground.activeInHierarchy)
                nicknameErrorBackground.SetActive(true);

            nicknameErrorText.text = "Please type a nickname!";
            return;
        }

        PhotonNetwork.NickName = nicknameInputField.text;
        MenuManager.Instance.OpenMenu("title");
    } 

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text) && string.IsNullOrEmpty(roomPasswordInputField.text))
        {
            if(!errorBackground.activeInHierarchy) 
                errorBackground.SetActive(true);

            createRoomErrorText.text = "Please type the name and password of the room you want to create!";
            return;
        }
        else if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            if (!errorBackground.activeInHierarchy)
                errorBackground.SetActive(true);

            createRoomErrorText.text = "Please type the name of the room you want to create!";
            return;
        }
        else if (string.IsNullOrEmpty(roomPasswordInputField.text))
        {
            if (!errorBackground.activeInHierarchy)
                errorBackground.SetActive(true);

            createRoomErrorText.text = "Please type the password of the room you want to create!";
            return;
        }

        roomProperty.Clear();

        roomProperty.Add("Password", roomPasswordInputField.text);
        roomOptions.CustomRoomProperties = roomProperty;
        roomOptions.CustomRoomPropertiesForLobby = new string[] {"Password"};

        PhotonNetwork.CreateRoom(roomNameInputField.text, roomOptions); 
    }

    public override void OnJoinedRoom()
    {
        roomNameInputField.text = "";
        roomPasswordInputField.text = "";
        createRoomErrorText.text = "";
        errorBackground.SetActive(false);

        MenuManager.Instance.OpenMenu("room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Count(); i++)
        {
            if (players[i].CustomProperties.TryGetValue("SelectedCharacter", out object selectedCharacter))
            {
                Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i], selectedCharacter as string);
            }
        }

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }
     
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if (returnCode == 32766)
        {
            if (!errorBackground.activeInHierarchy)
                errorBackground.SetActive(true);

            if (createRoomErrorText.text != "Room Creation Failed: " + returnCode + ", " + message)
                createRoomErrorText.text = "Room Creation Failed: " + returnCode + ", " + message;                 
        }
        else
        {
            errorText.text = "Room Creation Failed: " + returnCode + ", " + message;
            MenuManager.Instance.OpenMenu("error");
        }       
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(1);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("loading");
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("title");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform transform in roomListContent)
        {
            Destroy(transform.gameObject);
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList)
                continue;

            GameObject roomListItemObject = Instantiate(roomListItemPrefab, roomListContent);
            RoomListItem roomListItem = roomListItemObject.GetComponent<RoomListItem>();
            roomListItem.SetUp(roomList[i]);

            Transform textTransform = roomListItemObject.transform.GetChild(1);
            TMP_Text countText = textTransform.GetComponent<TMP_Text>();

            if (countText != null)
                countText.text = roomList[i].PlayerCount.ToString() ;
            else
                Debug.LogWarning("TMP_Text component not found in the child object.");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (newPlayer.CustomProperties.TryGetValue("SelectedCharacter", out object selectedCharacter))
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer, selectedCharacter as string);
        }
    }

    public void ChangeCharacterSelection(string newCharacter)
    {
        customProperties["SelectedCharacter"] = newCharacter;
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
    }

    public void OpenPlaceHolder(GameObject placeHolder)
    {
        placeHolder.SetActive(true);
    }

    public void ClosePlaceHolder(GameObject placeHolder)
    {
        placeHolder.SetActive(false);
    }

    public void BackButtonForCreateRoom()
    {
        MenuManager.Instance.OpenMenu("title");
        roomNameInputField.text = "";
        roomPasswordInputField.text = "";
        createRoomErrorText.text = "";
        errorBackground.SetActive(false);
    }

    public void Exit()
    {
        Application.Quit();
    }
}   
