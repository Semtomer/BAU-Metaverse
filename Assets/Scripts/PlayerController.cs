
//LIBRARY REGION
#region
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
#endregion

public class PlayerController : MonoBehaviour
{
    //VARIABLES REGION
    #region
    Transform _camera;
    GameObject cinemachine;
    GameObject fpsCamera;

    [Header("Movement"), SerializeField] CharacterController characterController;
    float speed = 6f, turnSmoothTime = 0.1f;
    Vector3 direction;
    float turnSmoothVelocity;
    bool isThirdPersonMovement = true;

    [Header("Jumping")]
    [SerializeField] float jumpHeight = 3f;

    [Header("Gravity")]
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float groundDistance = 0.4f;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;
    bool isGrounded;
    bool applyGravity = true;
    Vector3 velocity;

    [Header("Animation")]
    [SerializeField] Animator animator;
    [HideInInspector] public static string runningAnimation = "";

    [Header("Grabbing"), SerializeField] GameObject rightHand;
    [SerializeField] Vector3 itemToShowSpawnPosition, itemToShowSpawnScale;
    [SerializeField] Quaternion itemToShowSpawnRotation;
    [SerializeField] float holdingAnimStart = 0.75f, holdingAnimContinue = 0.75f;
    [HideInInspector] public static Text grabText;
    [HideInInspector] public static GameObject itemWantToGrab;
    GameObject itemToShow;
    GameObject holdingItem;
    bool hasItem = false;
    bool isVolunteerToDropItem = true;

    [Header("Sitting"), SerializeField] Vector3 sitRefPosition;
    [HideInInspector] public static Text sitText;
    [HideInInspector] public static Text sitText2;
    [HideInInspector] public static GameObject objectWantToSit;
    [HideInInspector] public static bool isSitting = false;

    [Header("Piano"), SerializeField] Vector3 pianoSitRefPosition;
    [HideInInspector] public static GameObject PianoToSit;
    [HideInInspector] public static Text pianoText;
    [HideInInspector] public static bool isPlayingPiano = false;
    AudioSource pianoAudioSource;

    GameObject pauseMenu;
    [HideInInspector] public static bool isPaused = false;

    GameObject chatMenu;
    GameObject chatInputFieldGameObject;
    GameObject chatWarning;
    TMP_InputField chatInputField;
    [HideInInspector] public static bool isChatting = false;
    Transform chatBar;
    int chatBarChildCount = 0;

    [Header("Emoji Menu"), SerializeField] Sprite[] emojiSprites = new Sprite[5];
    GameObject emojiMenu;

    bool isSpawned = false;
    Vector3[] spawnPositions = new Vector3[6]
    {   new Vector3(-25f, 3.4f, 10f),
        new Vector3(0f, 3.4f, 0f),
        new Vector3(25f, 3.4f, 0f),
        new Vector3(0f, 3.4f, 4.5f),
        new Vector3(0f, 0f, -22.5f),
        new Vector3(23f, 0f, -14f)
    };

    string nickName;
    PhotonView PV;

    GameObject textBubble;

    AudioSource GameMusicSource;

    Animator doorAnimator1, doorAnimator2;
    [HideInInspector] public static bool tutorialDoorIsClosed = true;

    List<GameObject> playerList;
    #endregion

    //AWAKE
    void Awake()
    {
        PV = GetComponent<PhotonView>();

        if (PV.IsMine)
        {
            _camera = GameObject.FindWithTag("MainCamera").transform;
            cinemachine = GameObject.FindWithTag("Cinemachine");
            cinemachine.GetComponent<CinemachineFreeLook>().Follow = gameObject.transform;
            cinemachine.GetComponent<CinemachineFreeLook>().LookAt = gameObject.transform;   
            fpsCamera = GameObject.FindWithTag("FPSCamera");
            fpsCamera.SetActive(false); 
        }
    }

    //START
    void Start()
    {
        if (PV.IsMine)
        {
            SetSpawnPosition();

            SetReferences();

            SetEmojiMenuSetting();

            ApplyReferenceSettings();

            SetEventTriggerOnChatInputField();

            SelectRandomColor();

            StartCoroutine(UpdateNicknamesAndList());

            StartCoroutine(SetParentToMessages());

            SetFirstTimeSittingAndPlayingBooleans();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //UPDATE
    void Update()
    {
        if (!PV.IsMine)
            return;

        LookNickNameTextsToCamera();

        ApplyGravity();

        runningAnimation = whichAnimRunning();

        if (isThirdPersonMovement)
            ThirdPersonMove();
        else
            FirstPersonMove();

        SetTextVisibility(sitText, true);

        SetTextBubbleVisibility();

        DropItem();

        StartCoroutine(GrabItem());

        ToggleChatMenu();

        StartCoroutine(IEnumerator_ShowWarningMessage());
    }

    //START FUNCTIONS
    #region
    void SetSpawnPosition()
    {
        while (!isSpawned)
        {
            int randomIndex;

            try
            {
                randomIndex = Random.Range(0, 6);
                transform.position = spawnPositions[randomIndex];
            }
            catch (System.IndexOutOfRangeException)
            {
                isSpawned = false;
                continue;
            }

            isSpawned = true;
        }
    }

    void SetEmojiMenuSetting()
    {
        for (int i = 0; i < emojiSprites.Length; i++)
        {
            emojiMenu.GetComponentsInChildren<Image>()[i].sprite = emojiSprites[i];
            emojiMenu.GetComponentsInChildren<Button>()[i].onClick.AddListener(CloseEmojiMenu);
        }

        emojiMenu.GetComponentsInChildren<Button>()[0].onClick.AddListener(HandRaising);
        emojiMenu.GetComponentsInChildren<Button>()[1].onClick.AddListener(Special);
        emojiMenu.GetComponentsInChildren<Button>()[2].onClick.AddListener(Talking);
        emojiMenu.GetComponentsInChildren<Button>()[3].onClick.AddListener(Special2);
        emojiMenu.GetComponentsInChildren<Button>()[4].onClick.AddListener(Waving);
    }

    void SetReferences()
    {
        grabText = GameObject.FindWithTag("GrabText").GetComponent<Text>();

        sitText = GameObject.FindWithTag("SitText").GetComponent<Text>();
        sitText2 = GameObject.FindWithTag("SitText2").GetComponent<Text>();

        pianoText = GameObject.FindWithTag("PianoText").GetComponent<Text>();

        pauseMenu = GameObject.FindWithTag("PauseMenu");

        emojiMenu = GameObject.FindWithTag("EmojiMenu");

        textBubble = GameObject.FindWithTag("TextBubble");

        chatMenu = GameObject.FindWithTag("ChatMenu");
        chatWarning = GameObject.FindWithTag("ChatWarning");
        chatBar = chatMenu.GetComponentInChildren<ScrollRect>().gameObject.GetComponentInChildren<VerticalLayoutGroup>().gameObject.transform;
        chatInputFieldGameObject = GameObject.FindWithTag("ChatInputField");
        chatInputField = chatInputFieldGameObject.GetComponent<TMP_InputField>();

        doorAnimator1 = GameObject.FindWithTag("TutorialDoor").transform.GetChild(0).GetComponent<Animator>();
        doorAnimator2 = GameObject.FindWithTag("TutorialDoor").transform.GetChild(1).GetComponent<Animator>();

        playerList = new List<GameObject>();
    }

    void ApplyReferenceSettings()
    {
        grabText.enabled = false;

        sitText.enabled = false;
        sitText2.enabled = false;

        pianoText.enabled = false;

        pauseMenu.SetActive(false);

        emojiMenu.SetActive(false);

        textBubble.SetActive(false);

        chatMenu.SetActive(false);
        chatWarning.SetActive(false);
    }

    void SetFirstTimeSittingAndPlayingBooleans()
    {
        bool cacheIsSitting = false;
        bool cacheIsPlaying = false;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties["isSittingAnotherPlayer"] != null)
            {
                if ((bool)player.CustomProperties["isSittingAnotherPlayer"])
                    cacheIsSitting = true;            
            }

            if (player.CustomProperties["isPlayingAnotherPlayer"] != null)
            {
                if ((bool)player.CustomProperties["isPlayingAnotherPlayer"])
                    cacheIsPlaying = true;             
            }
        }

        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable()
        {
            { "isSittingAnotherPlayer", cacheIsSitting },
            { "isPlayingAnotherPlayer", cacheIsPlaying }
        };

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            player.SetCustomProperties(customProperties);
        }
    }

    void SetNicknames()
    {
        playerList.Clear();

        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            playerList.Add(player);
            player.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = player.GetComponent<PhotonView>().Owner.NickName;
        }     

        //Sadece kendi karakterin nickname'i gözükmez.
        transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().enabled = false;
    }

    IEnumerator UpdateNicknamesAndList()
    {
        SetNicknames();

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(UpdateNicknamesAndList());
    }
    #endregion

    //ESSENTIAL FUNCTIONS
    #region
    void ApplyGravity()
    {
        if (!isPaused && !isChatting && applyGravity)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }      
    }

    string whichAnimRunning()
    {
        try
        {
            if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Breathing Idle" ||
                animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Standing Idle" ||
                animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Neutral Idle" ||
                animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Idle")
            {
                return "Idle";
            }
            else if (animator.GetBool("Walking"))
            {
                return "Walking";
            }
            else if (animator.GetBool("Running"))
            {
                return "Running";
            }
            else if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Unarmed Jump" ||
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Jump" ||
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Jumping")
            {
                return "Jumping";
            }
            else if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Stand To Sit" ||
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Sitting Idle" ||
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Sitting" ||
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Sit To Stand")
            {
                return "Sitting";
            }
            else if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Picking Up Object")
            {
                return "Grabbing";
            }
            else if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Piano Playing")
            {
                return "Piano";
            }
            else if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Hand Raising" ||
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Salute" ||
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Talking" ||
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Taunt" ||
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Waving" ||
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Arms Hip Hop Dance" ||
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Bashful" ||
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Hip Hop Dancing" ||
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Angry" ||
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Bboy Hip Hop Move" ||
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Punching" ||
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Rumba Dancing" ||
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Rapping" ||
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Yelling" ||
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Locking Hip Hop Dance")
            {
                return "Emoji";
            }
        }
        catch (System.IndexOutOfRangeException)
        {
        }

        return "";
    }

    void ThirdPersonMove()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (!isPaused && direction.magnitude >= 0.1f && runningAnimation != "Grabbing" && runningAnimation != "Sitting" && runningAnimation != "Piano" 
            && !isPlayingPiano && !isSitting && characterController.enabled && !isChatting && !chatInputField.isFocused)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _camera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            characterController.Move(moveDirection.normalized * speed * Time.deltaTime);

            if (speed == 6f)
            {
                animator.SetBool("Running", false);
                animator.SetBool("Walking", true);
            }
            else
            {
                animator.SetBool("Running", true);
                animator.SetBool("Walking", false);
            }

            animator.SetBool("Grabbing", false);
        }
        else
        {
            animator.SetBool("Running", false);
            animator.SetBool("Walking", false);
        }
    }

    void FirstPersonMove()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (!isPaused && direction.magnitude >= 0.1f && runningAnimation != "Grabbing" && runningAnimation != "Sitting" && runningAnimation != "Piano" 
            && !isPlayingPiano && !isSitting && characterController.enabled && !isChatting && !chatInputField.isFocused)
        {
            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            characterController.Move(move * speed * Time.deltaTime);

            if (speed == 6f)
            {
                animator.SetBool("Running", false);
                animator.SetBool("Walking", true);
            }
            else
            {
                animator.SetBool("Running", true);
                animator.SetBool("Walking", false);
            }

            animator.SetBool("Grabbing", false);
        }
        else
        {
            animator.SetBool("Running", false);
            animator.SetBool("Walking", false);
        } 
    }

    public void ChangeMovementSpeed(InputAction.CallbackContext context)
    {
        if (context.performed && PV.IsMine && !isPaused && !isChatting && !chatInputField.isFocused)
        {
            if (speed == 6f)
                speed = 12f; 
            else if (speed == 12f)
                speed = 6f;
        }
    }

    public void SwitchControl(InputAction.CallbackContext context)
    {
        if (context.performed && PV.IsMine && !isPaused && !isChatting && !chatInputField.isFocused)
        {
            isThirdPersonMovement = !isThirdPersonMovement;

            if (isThirdPersonMovement)
            {
                fpsCamera.SetActive(false);

                cinemachine.SetActive(true);
                _camera.gameObject.SetActive(true);
            }
            else
            {
                fpsCamera.SetActive(true);
                FPSCamera.isSwitchedFPS = true;

                cinemachine.SetActive(false);
                _camera.gameObject.SetActive(false);
            }
        }       
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded && runningAnimation != "Grabbing" && runningAnimation != "Sitting" 
            && runningAnimation != "Piano" && !isPaused && !isChatting)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jumping");
        }
    }
    #endregion

    //LOOK FUNCTIONS
    #region
    [HideInInspector]
    public bool IsLooking(GameObject gameObject)
    {
        if (gameObject != null)
        {
            Vector3 character = transform.TransformDirection(Vector3.forward);
            Vector3 item = gameObject.transform.position - transform.position;

            if (gameObject == itemWantToGrab)
            {
                if (Vector3.Dot(character, item) < 4f && Vector3.Dot(character, item) > 2f)
                    return true;
            }
            else if (gameObject == PianoToSit)
            {
                if (Vector3.Dot(character, item) < 4.5f && Vector3.Dot(character, item) > 2f)
                    return true;
            }
        }

        return false;
    }

    [HideInInspector] public bool IsNotLooking(GameObject gameObject)
    {
        if (gameObject != null)
        {
            Vector3 character = transform.TransformDirection(Vector3.forward);
            Vector3 item = gameObject.transform.position - transform.position;

            if (Vector3.Dot(character, item) < -2.5f && Vector3.Dot(character, item) > -4f)
                return true;
        }

        return false;
    }
    #endregion

    //TEXT FUNCTIONS
    #region
    void SetTextVisibility(Text text, bool isVisible)
    {
        if (PV.IsMine && Tutorial.displayText == null)
        {
            if (isVisible && text == sitText && !isSitting && runningAnimation == "Idle" && IsNotLooking(objectWantToSit))
            {
                text.enabled = true;
            }
            else if (isVisible && text == grabText)
            {
                text.enabled = true;
            }
            else if (isVisible && text == pianoText && !isPlayingPiano)
            {
                text.enabled = true;
            }
            else
            {
                text.enabled = false;
            }
        }  
    }

    void SetTextBubbleVisibility()
    {
        if (textBubble != null)
        {
            if (grabText.enabled || sitText.enabled || sitText2.enabled || pianoText.enabled || (Tutorial.displayText != null && Tutorial.displayText.enabled))
                textBubble.SetActive(true);
            else
                textBubble.SetActive(false);
        }     
    }

    void LookNickNameTextsToCamera()
    {
        foreach (GameObject player in playerList)
        {
            if (player != null)
            {
                if (_camera.gameObject.activeInHierarchy)
                    player.transform.GetChild(0).transform.rotation = Quaternion.LookRotation(transform.position - _camera.position - new Vector3(0f, -12f, 0f));
                else
                {
                    player.transform.GetChild(0).transform.LookAt(fpsCamera.GetComponentInParent<Transform>());
                    player.transform.GetChild(0).transform.Rotate(Vector3.up * 180f);
                }
            }
        }
    }
    #endregion

    //GRAB AND DROP ITEM MECHANIC
    #region
    [PunRPC]
    void InstantiateGrabbableObject()
    {
        if (itemToShow == null)
        {
            itemToShow = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "American Pint Glass"), Vector3.zero, Quaternion.identity);
            itemToShow.transform.SetParent(rightHand.transform);
            itemToShow.transform.localPosition = itemToShowSpawnPosition;
            itemToShow.transform.localRotation = itemToShowSpawnRotation;
            itemToShow.transform.localScale = itemToShowSpawnScale;
        }

        foreach (GameObject unnecessaryItemToShow in GameObject.FindGameObjectsWithTag("ItemToShow"))
        {
            if (unnecessaryItemToShow.transform.localScale == new Vector3(1f,1f,1f))
                Destroy(unnecessaryItemToShow);
        }
    }

    [PunRPC]
    void DestroyGrabbableObject()
    {
        if (itemToShow != null)
            Destroy(itemToShow);          
    }

    IEnumerator GrabItem()
    {
        if (runningAnimation == "Idle" && TriggerForObjects.canPickUp == true && hasItem == false 
            && IsLooking(itemWantToGrab) && !isPaused && !isChatting && !chatInputField.isFocused)
        {
            SetTextVisibility(grabText, true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                isVolunteerToDropItem = true;

                animator.SetBool("Grabbing", true);

                SetTextVisibility(grabText, false);

                yield return new WaitForSeconds(holdingAnimStart);

                itemWantToGrab.GetComponent<MeshRenderer>().enabled = false;

                PV.RPC("InstantiateGrabbableObject", RpcTarget.AllBuffered);

                hasItem = true;
                holdingItem = itemWantToGrab;

                yield return new WaitForSeconds(holdingAnimContinue);
                animator.SetBool("Grabbing", false);
            }
        }
        else
        {
            SetTextVisibility(grabText, false);
        }
    }

    void DropItem()
    {
        if (Input.GetKeyDown(KeyCode.E) && hasItem == true && isVolunteerToDropItem && !isPaused && !isChatting && !chatInputField.isFocused)
        {
            animator.SetBool("Grabbing", false);

            holdingItem.GetComponent<MeshRenderer>().enabled = true;
            PV.RPC("DestroyGrabbableObject", RpcTarget.AllBuffered);

            hasItem = false;
            itemWantToGrab = null;
        }
        else if (hasItem && !isVolunteerToDropItem && (runningAnimation == "Emoji" || runningAnimation == "Sitting" || runningAnimation == "Piano"))
        {
            PV.RPC("DestroyGrabbableObject", RpcTarget.AllBuffered);
            isVolunteerToDropItem = true;
        }
        else if (hasItem && isVolunteerToDropItem && runningAnimation != "Emoji" && runningAnimation != "Sitting" && runningAnimation != "Piano")
        {
            PV.RPC("InstantiateGrabbableObject", RpcTarget.AllBuffered);
        }           
    }
    #endregion

    //SIT AND STAND UP MECHANIC
    #region
    void UpdateIsSittingAnotherPlayerBoolean(bool newValue)
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            player.CustomProperties["isSittingAnotherPlayer"] = newValue;
            player.SetCustomProperties(player.CustomProperties);
        }
    }

    public void SitOnAChair(InputAction.CallbackContext context)
    {
        if (context.performed && runningAnimation == "Idle" && TriggerForObjects.canSit == true 
            && !isSitting && !(bool)PhotonNetwork.LocalPlayer.CustomProperties["isSittingAnotherPlayer"] 
            && IsNotLooking(objectWantToSit) && !isPaused && !isChatting && !chatInputField.isFocused)
        {
            isVolunteerToDropItem = false;

            SetTextVisibility(sitText, false);

            foreach (Collider collider in objectWantToSit.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }

            //Need to turn off gravity while sitting.
            applyGravity = false;
       
            Vector3 tempSitPosition = objectWantToSit.transform.position + (objectWantToSit.transform.right * sitRefPosition.x) + (objectWantToSit.transform.forward * sitRefPosition.y) + (objectWantToSit.transform.up * -sitRefPosition.z);
            characterController.Move(tempSitPosition - transform.position);

            //This component is also turned on/off due to the gravity application within itself.
            characterController.enabled = false;

            animator.SetBool("Sitting", true);
            isSitting = true;
            FPSCamera.isStartingToSit = true;
            UpdateIsSittingAnotherPlayerBoolean(true);

            transform.eulerAngles = new Vector3(0, objectWantToSit.transform.rotation.eulerAngles.y, 0);
        }
    }

    public void StandUpChair(InputAction.CallbackContext context)
    {
        try
        {
            if (context.performed && !chatInputField.isFocused && !isChatting)
                StartCoroutine(IEnumerator_StandUpChair());
        }
        catch (System.NullReferenceException e)
        {
            Debug.LogWarning(e);
        }        
    }

    IEnumerator IEnumerator_StandUpChair()
    {
        if (isSitting == true && runningAnimation == "Sitting" && !isPaused)
        {
            animator.SetBool("Sitting", false);
            isSitting = false;
            UpdateIsSittingAnotherPlayerBoolean(false);

            yield return new WaitForSeconds(2);

            //Need to turn on gravity while not sitting.
            applyGravity = true;
            //This component is also turned on/off due to the gravity application within itself.
            characterController.enabled = true;

            Vector3 tempStandUpPosition = objectWantToSit.transform.position + objectWantToSit.transform.up * -3.0f;
            characterController.Move(tempStandUpPosition - transform.position);

            foreach (Collider collider in objectWantToSit.GetComponentsInChildren<Collider>())
            {
                collider.enabled = true;
            }
        } 
    }
    #endregion

    //PLAYING PIANO AND STAND UP MECHANIC
    #region
    void UpdateIsPlayingAnotherPlayerBoolean(bool newValue)
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            player.CustomProperties["isPlayingAnotherPlayer"] = newValue;
            player.SetCustomProperties(player.CustomProperties);
        }
    }

    public void PlayPiano(InputAction.CallbackContext context)
    {
        try
        {
            if (context.performed && runningAnimation == "Idle" && TriggerForObjects.canPlayPiano == true
            && !isPlayingPiano && !(bool)PhotonNetwork.LocalPlayer.CustomProperties["isPlayingAnotherPlayer"]
            && IsLooking(PianoToSit) && !isPaused && !isChatting && !chatInputField.isFocused)
            {
                StartCoroutine(PlayPianoWithDelay());
            }
        }
        catch (System.NullReferenceException e)
        {
            Debug.LogWarning(e);
        }     
    }

    IEnumerator PlayPianoWithDelay()
    {
        PV.RPC("ToggleMainSound", RpcTarget.AllBuffered);

        isVolunteerToDropItem = false;

        SetTextVisibility(pianoText, false);

        foreach (Collider collider in PianoToSit.GetComponentsInChildren<Collider>())
            collider.enabled = false;

        foreach (Collider collider in GameObject.FindWithTag("Piano").GetComponentsInChildren<Collider>())
            collider.enabled = false;

        applyGravity = false;

        if (gameObject.name == "TheBoss(Clone)")
            PianoToSit.transform.position = new Vector3(PianoToSit.transform.position.x, PianoToSit.transform.position.y -0.47f, PianoToSit.transform.position.z);

        Vector3 tempPianoSitPosition = PianoToSit.transform.position + (PianoToSit.transform.right * pianoSitRefPosition.x) + (PianoToSit.transform.forward * pianoSitRefPosition.y) + (PianoToSit.transform.up * -pianoSitRefPosition.z);
        characterController.Move(tempPianoSitPosition - transform.position);

        characterController.enabled = false;

        animator.SetBool("PlayingPiano", true);
        isPlayingPiano = true;
        FPSCamera.isStartingToSit = true;
        UpdateIsPlayingAnotherPlayerBoolean(true);

        transform.eulerAngles = new Vector3(0, PianoToSit.transform.rotation.eulerAngles.y, 0);

        yield return new WaitForSeconds(2);
        PV.RPC("TogglePianoSound", RpcTarget.AllBuffered);
    }

    public void StandUpPiano(InputAction.CallbackContext context)
    {
        try
        {
            if (context.performed && !chatInputField.isFocused && !isChatting)
                StartCoroutine(IEnumerator_StandUpPiano());
        }
        catch (System.NullReferenceException e)
        {
            Debug.LogWarning(e);
        }    
    }

    IEnumerator IEnumerator_StandUpPiano()
    {
        if (isPlayingPiano == true && runningAnimation == "Piano" && !isPaused)
        {
            PV.RPC("TogglePianoSound", RpcTarget.AllBuffered);
            animator.SetBool("PlayingPiano", false);
            isPlayingPiano = false;
            UpdateIsPlayingAnotherPlayerBoolean(false);

            yield return new WaitForSeconds(2);

            PV.RPC("ToggleMainSound", RpcTarget.AllBuffered);

            applyGravity = true;

            characterController.enabled = true;

            Vector3 tempStandUpPosition = PianoToSit.transform.position + PianoToSit.transform.right * 5.0f;
            characterController.Move(tempStandUpPosition - transform.position);

            if (gameObject.name == "TheBoss(Clone)")
                PianoToSit.transform.position = new Vector3(PianoToSit.transform.position.x, PianoToSit.transform.position.y + 0.47f, PianoToSit.transform.position.z);

            foreach (Collider collider in PianoToSit.GetComponentsInChildren<Collider>())
                collider.enabled = true;

            foreach (Collider collider in GameObject.FindWithTag("Piano").GetComponentsInChildren<Collider>())
                collider.enabled = true;
        }
    }

    [PunRPC]
    void TogglePianoSound()
    {
        pianoAudioSource = GameObject.FindWithTag("PianoAudio").GetComponent<AudioSource>();

        if (pianoAudioSource.isPlaying)
            pianoAudioSource.Stop();
        else
            pianoAudioSource.Play();
    }

    [PunRPC]
    void ToggleMainSound()
    {
        GameMusicSource = GameObject.FindWithTag("GameMusicAudio").GetComponent<AudioSource>();

        if (GameMusicSource.mute == true)
            GameMusicSource.mute = false;
        else
            GameMusicSource.mute = true;
    }
    #endregion

    //PAUSE MENU
    #region
    public void Pause(InputAction.CallbackContext context)
    {
        try
        {
            if (context.performed && !chatInputField.isFocused)
            {

                if (emojiMenu.activeInHierarchy)
                    CloseEmojiMenu();

                if (chatMenu.activeInHierarchy)
                    CloseChatMenu();

                isPaused = !isPaused;
                if (isPaused)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    PV.RPC("ToggleAnimations", RpcTarget.AllBuffered);
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    PV.RPC("ToggleAnimations", RpcTarget.AllBuffered);
                }

                pauseMenu.SetActive(isPaused);

                if (runningAnimation != "Piano" && runningAnimation != "Sitting")
                {
                    characterController.enabled = !isPaused;
                }

                cinemachine.GetComponent<CinemachineFreeLook>().enabled = !isPaused;
            }
        }
        catch (System.NullReferenceException e)
        {
            Debug.LogWarning(e);
        }
    }

    [PunRPC]
    void ToggleAnimations()
    {
        if (animator.speed == 1f)
            animator.speed = 0f;
        else
            animator.speed = 1f;
    }
    #endregion

    //CHAT SYSTEM
    #region
    void ToggleChatMenu()
    {
        if (Input.GetKeyDown(KeyCode.T) && !chatInputField.isFocused)
        {
            if (!chatMenu.activeInHierarchy && isGrounded && !isPaused && !isChatting)
            {
                chatMenu.SetActive(true);
                isChatting = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                characterController.enabled = false;
                cinemachine.GetComponent<CinemachineFreeLook>().enabled = false;
            }
            else
            {
                CloseChatMenu();
            }
        }      
    }

    void CloseChatMenu()
    {
        chatInputField.text = "";
        chatMenu.SetActive(false);
        isChatting = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        characterController.enabled = true;
        cinemachine.GetComponent<CinemachineFreeLook>().enabled = true;
    }

    void SelectRandomColor()
    {
        Color[] color = new Color[6]
        {
            new Color(1f,0f,0f,1f),
            new Color(1f,1f,0f,1f),
            new Color(1f,0.5f,0f,1f),
            new Color(1f,0f,1f,1f),
            new Color(0.75f,0f,1f,1f),
            new Color(0f,1f,0f,1f)
        };

        int randomIndex = Random.Range(0, 6);
        nickName = "<color=#" + ColorUtility.ToHtmlStringRGBA(color[randomIndex]) + ">" + PhotonNetwork.NickName;

        //Ayný stringde farklý renkler gerekirse, renk tanýmlamalarýnýn arasýna "</color>" eklemeyi unutma!
        //"<color=#C9CEFF>" + string1 + "</color>" + "<color=#C9CEFF>" + string2; gibi
    }

    public void SendChatMessage(InputAction.CallbackContext context)
    {
        try
        {
            if (context.performed)
            {
                if (chatInputField.text != "")
                {
                    PV.RPC("RPC_SendChatMessage", RpcTarget.AllBuffered, nickName);
                    PV.RPC("RPC_SendChatMessage", RpcTarget.AllBuffered, chatInputField.text);
                    PV.RPC("RPC_SendChatMessage", RpcTarget.AllBuffered, "  ");

                    chatInputField.text = "";
                    chatInputField.ActivateInputField();
                }
            }
        }
        catch (System.NullReferenceException e)
        {
            Debug.LogWarning(e);
        }
    }

    [PunRPC]
    void RPC_SendChatMessage(string message)
    {
        GameObject messagePrefab = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "ChatPrefab"), Vector3.zero, Quaternion.identity);
        messagePrefab.transform.SetParent(chatBar);
        messagePrefab.GetComponent<TMP_Text>().text = message;
    }

    IEnumerator SetParentToMessages()
    {
        foreach (GameObject message in GameObject.FindGameObjectsWithTag("ChatPrefab"))
        {
            if (message.GetComponent<TMP_Text>().text != "")
                message.transform.SetParent(chatBar);
            else
                Destroy(message);
        }

        yield return new WaitForSeconds(1f);

        StartCoroutine(SetParentToMessages());
    }

    IEnumerator IEnumerator_ShowWarningMessage()
    {
        if (chatBarChildCount != chatBar.transform.childCount)
        {
            chatBarChildCount = chatBar.transform.childCount;

            for (int i = 0; i < 3; i++)
            {
                if (!chatMenu.activeInHierarchy && PV.IsMine)
                {
                    chatWarning.SetActive(true);
                    yield return new WaitForSeconds(0.5f);
                    chatWarning.SetActive(false);
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    chatWarning.SetActive(false);
                    break;
                }
            }
        }     
    }

    void OnSelect(BaseEventData eventData)
    {
        chatInputFieldGameObject.transform.GetChild(0).GetChild(2).gameObject.SetActive(false);
    }

    void OnDeselect(BaseEventData eventData)
    {
        chatInputFieldGameObject.transform.GetChild(0).GetChild(2).gameObject.SetActive(true);
    }

    void SetEventTriggerOnChatInputField()
    {
        EventTrigger.Entry selectEntry = new EventTrigger.Entry();
        selectEntry.eventID = EventTriggerType.Select;
        selectEntry.callback.AddListener((data) => { OnSelect((BaseEventData)data); });

        EventTrigger.Entry deselectEntry = new EventTrigger.Entry();
        deselectEntry.eventID = EventTriggerType.Deselect;
        deselectEntry.callback.AddListener((data) => { OnDeselect((BaseEventData)data); });

        EventTrigger eventTrigger = chatInputFieldGameObject.GetComponent<EventTrigger>();
        eventTrigger.triggers.Add(selectEntry);
        eventTrigger.triggers.Add(deselectEntry);
    }
    #endregion

    //EMOJI ANIMATIONS
    #region
    public void HandRaising(InputAction.CallbackContext context)
    {
        if (context.performed && !chatInputField.isFocused && !isChatting)
            HandRaising();
    }

    void HandRaising()
    {
        if (!isPaused && isGrounded && (runningAnimation == "Idle" || runningAnimation == "Emoji"))
        {
            isVolunteerToDropItem = false;
            animator.SetTrigger("HandRaising");
        }
    }

    public void Special(InputAction.CallbackContext context)
    {
        if (context.performed && !chatInputField.isFocused && !isChatting)
            Special();
    }

    void Special()
    {
        if (!isPaused && isGrounded && (runningAnimation == "Idle" || runningAnimation == "Emoji"))
        {
            isVolunteerToDropItem = false;
            animator.SetTrigger("Special");
        }
    }

    public void Talking(InputAction.CallbackContext context)
    {
        if (context.performed && !chatInputField.isFocused && !isChatting)
            Talking();
    }

    void Talking()
    {
        if (!isPaused && isGrounded && (runningAnimation == "Idle" || runningAnimation == "Emoji"))
        {
            isVolunteerToDropItem = false;
            animator.SetTrigger("Talking");
        }
    }

    public void Special2(InputAction.CallbackContext context)
    {
        if (context.performed && !chatInputField.isFocused && !isChatting)
            Special2();
    }

    void Special2()
    {
        if (!isPaused && isGrounded && (runningAnimation == "Idle" || runningAnimation == "Emoji"))
        {
            isVolunteerToDropItem = false;
            animator.SetTrigger("Special2");
        }
    }

    public void Waving(InputAction.CallbackContext context)
    {
        if (context.performed && !chatInputField.isFocused && !isChatting)
            Waving();
    }

    void Waving()
    {
        if (!isPaused && isGrounded && (runningAnimation == "Idle" || runningAnimation == "Emoji"))
        {
            isVolunteerToDropItem = false;
            animator.SetTrigger("Waving");
        }
    }

    public void ToggleEmojiMenu(InputAction.CallbackContext context)
    {
        try
        {
            if (context.performed && !chatInputField.isFocused && !isChatting)
            {
                if (!emojiMenu.activeInHierarchy && !isPaused && isGrounded && (runningAnimation == "Idle" || runningAnimation == "Emoji"))
                {
                    emojiMenu.SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    isPaused = true;
                    characterController.enabled = false;
                    cinemachine.GetComponent<CinemachineFreeLook>().enabled = false;
                }
                else if (emojiMenu.activeInHierarchy)
                {
                    CloseEmojiMenu();
                }
            }
        }
        catch (System.NullReferenceException e)
        {
            Debug.LogWarning(e);
        }      
    }

    void CloseEmojiMenu()
    {
        emojiMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isPaused = false;
        characterController.enabled = true;
        cinemachine.GetComponent<CinemachineFreeLook>().enabled = true;
    }
    #endregion

    //TUTORIAL DOOR
    #region
    public void ToggleTutorialDoor(InputAction.CallbackContext context)
    {
        try
        {
            if (context.performed && isGrounded && !isPaused && !isChatting && !chatInputField.isFocused && TriggerForObjects.canOpenAndClose)
            {
                if (tutorialDoorIsClosed)
                {
                    doorAnimator1.SetBool("openDoor", true);
                    doorAnimator2.SetBool("openDoor", true);
                    tutorialDoorIsClosed = false;
                }
                else if (!tutorialDoorIsClosed)
                {
                    doorAnimator1.SetBool("openDoor", false);
                    doorAnimator2.SetBool("openDoor", false);
                    tutorialDoorIsClosed = true;
                }
            }
        }
        catch (System.NullReferenceException e)
        {
            Debug.LogWarning(e);
        }
    }
    #endregion
}