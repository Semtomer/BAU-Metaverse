
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Cinemachine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenuManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject loadingMenu;

    [Header("Sound Butttons"),SerializeField] Button buttonQuarter;
    [SerializeField] Button buttonHalf;
    [SerializeField] Button buttonFull;
    [SerializeField] Button SoundOn;
    [SerializeField] Button SoundOff;

    [Header("Audio Source Objects"), SerializeField] GameObject ClickAudioObject;
    [SerializeField] GameObject GameMusicObject;
    [SerializeField] GameObject PianoAudioObject;

    AudioSource clickSource;
    AudioSource hoverSource;
    AudioSource pianoAudioSource;
    AudioSource gameMusicSource;

    GameObject myCharacter;

    [SerializeField] Color blueColor;

    CinemachineFreeLook cinemachineFL;

    GameObject confirmationForExit;
    GameObject confirmationForMainMenu;
    GameObject pauseMenuButtons;

    private void Start()
    {
        clickSource = ClickAudioObject.GetComponents<AudioSource>()[0];
        hoverSource = ClickAudioObject.GetComponents<AudioSource>()[1];

        pianoAudioSource = PianoAudioObject.GetComponent<AudioSource>();
        gameMusicSource = GameMusicObject.GetComponent<AudioSource>();

        foreach (GameObject character in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (character.GetComponent<PhotonView>().IsMine)
            {
                myCharacter = character;
            }
        }

        cinemachineFL = GameObject.FindWithTag("Cinemachine").GetComponent<CinemachineFreeLook>();

        confirmationForExit = GameObject.FindWithTag("ConfirmationForExit");
        confirmationForMainMenu = GameObject.FindWithTag("ConfirmationForMainMenu");
        pauseMenuButtons = GameObject.FindWithTag("PauseMenuButtons");
        confirmationForMainMenu.SetActive(false);
        confirmationForExit.SetActive(false);

        loadingMenu.SetActive(false);
    }

    public void playClickSound()
    {
        clickSource.Play();
    }

    public void playHoverSound()
    {
        hoverSource.Play();
    }

    public void ExitMetaverse()
    {
        PlayerController.isPaused = !PlayerController.isPaused;
        myCharacter.GetComponent<PhotonView>().RPC("DestroyGrabbableObject", RpcTarget.AllBuffered);

        Application.Quit();
    }

    public void ToggleConformationForExit()
    {
        if (confirmationForExit.activeInHierarchy)
        {
            confirmationForExit.SetActive(false);
            pauseMenuButtons.SetActive(true);
        }
        else
        {
            confirmationForExit.SetActive(true);
            pauseMenuButtons.SetActive(false);
        }
    }

    public void ToggleConformationForMainMenu()
    {
        if (confirmationForMainMenu.activeInHierarchy)
        {
            confirmationForMainMenu.SetActive(false);
            pauseMenuButtons.SetActive(true);
        }
        else
        {
            confirmationForMainMenu.SetActive(true);
            pauseMenuButtons.SetActive(false);
        }
    }

    public void Resume()
    {
        PlayerController.isPaused = !PlayerController.isPaused;

        pauseMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (PlayerController.runningAnimation != "Piano" && PlayerController.runningAnimation != "Sitting")
        {
            myCharacter.GetComponent<CharacterController>().enabled = !PlayerController.isPaused;
        }

        cinemachineFL.enabled = !PlayerController.isPaused;

        myCharacter.GetComponent<PhotonView>().RPC("ToggleAnimations", RpcTarget.AllBuffered);
    }

    public void BackToMainMenu()
    {
        PlayerController.isPaused = !PlayerController.isPaused;

        if (PlayerController.runningAnimation != "Piano" && PlayerController.runningAnimation != "Sitting")
        {
            myCharacter.GetComponent<CharacterController>().enabled = !PlayerController.isPaused;
        }

        cinemachineFL.enabled = !PlayerController.isPaused;

        myCharacter.GetComponent<PhotonView>().RPC("ToggleAnimations", RpcTarget.AllBuffered);

        Destroy(GameObject.Find("RoomManager"));

        loadingMenu.SetActive(true);

        PhotonNetwork.LeaveRoom();
        PhotonNetwork.Disconnect();

        StartCoroutine(waitForMainMenu(0.5f));     
    }

    IEnumerator waitForMainMenu(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene(0);
    }

    public void soundFull()
    {
        buttonQuarter.image.color = blueColor;
        buttonHalf.image.color = blueColor;
        buttonFull.image.color = blueColor;
        SoundOff.gameObject.SetActive(false);
        SoundOn.gameObject.SetActive(true);

        pianoAudioSource.volume = 0.5f;
        gameMusicSource.volume = 0.1f;
    }

    public void soundHalf()
    {
        buttonQuarter.image.color = blueColor;
        buttonHalf.image.color = blueColor;
        buttonFull.image.color = Color.white;
        SoundOff.gameObject.SetActive(false);
        SoundOn.gameObject.SetActive(true);

        pianoAudioSource.volume = 0.25f;
        gameMusicSource.volume = 0.05f;
    }

    public void soundQuarter()
    {
        buttonQuarter.image.color = blueColor;
        buttonHalf.image.color = Color.white;
        buttonFull.image.color = Color.white;
        SoundOff.gameObject.SetActive(false);
        SoundOn.gameObject.SetActive(true);

        pianoAudioSource.volume = 0.125f;
        gameMusicSource.volume = 0.025f;

    }

    public void soundMute()
    {
        buttonQuarter.image.color = Color.white;
        buttonHalf.image.color = Color.white;
        buttonFull.image.color = Color.white;
        SoundOff.gameObject.SetActive(true);
        SoundOn.gameObject.SetActive(false);

        gameMusicSource.volume = 0f;
        pianoAudioSource.volume = 0f;
    }
}
