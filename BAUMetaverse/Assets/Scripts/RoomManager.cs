
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;
    [SerializeField] ToggleGroup radioButtons;
    string selectedCharacter = "Joe";

    [SerializeField] GameObject passwordMenu;
    [HideInInspector] public RoomInfo info;
    [SerializeField] TMP_InputField checkPasswordInputField;
    [SerializeField] Button backButton;
    [SerializeField] GameObject scrollView;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public void SelectCharacter()
    {
        Toggle selectedToggle = radioButtons.ActiveToggles().FirstOrDefault();
        selectedCharacter = selectedToggle.name;
        Launcher.Instance.ChangeCharacterSelection(selectedCharacter);
    }

    public void SetVisiblePasswordMenu(bool isVisible)
    {
        checkPasswordInputField.text = "";

        backButton.interactable = !isVisible;

        GameObject[] roomListButtons = GameObject.FindGameObjectsWithTag("RoomListButton");
        foreach (GameObject roomListButton in roomListButtons)
        {
            roomListButton.GetComponent<Button>().interactable = !isVisible;
        }

        scrollView.GetComponent<ScrollRect>().enabled = !isVisible;

        passwordMenu.GetComponentInChildren<Text>().enabled = false;

        passwordMenu.SetActive(isVisible);   
    }

    public void JoinRoom()
    {
        if (checkPasswordInputField.text == info.CustomProperties["Password"] as string)
        {
            checkPasswordInputField.text = "";
            SetVisiblePasswordMenu(false);
            Launcher.Instance.JoinRoom(info);
        }
        else
        {
            checkPasswordInputField.text = "";
            passwordMenu.GetComponentInChildren<Text>().enabled = true;
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == 1)
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", selectedCharacter), Vector3.zero, Quaternion.identity);
    }
}
