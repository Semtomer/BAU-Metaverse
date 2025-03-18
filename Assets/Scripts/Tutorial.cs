
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

[System.Serializable]
public class TextStep
{
    public string stepText;
}

public class Tutorial : MonoBehaviour
{
    [SerializeField] List<TextStep> textSteps = new List<TextStep>();
    [HideInInspector] public static int currentStep = 0;

    [HideInInspector] public static Text displayText;

    [SerializeField] GameObject tutorialText;
    [SerializeField] GameObject tutorialCheckPoint;
    [SerializeField] GameObject tutorialJumpCheckpoint;

    [Header("Checkpoint positions for tutorial steps"), SerializeField] Vector3 pianoposition;
    [SerializeField] Vector3 sitposition;
    [SerializeField] Vector3 grabposition;
    [SerializeField] Vector3 doorposition;

    private void Start()
    {
        displayText = tutorialText.GetComponent<Text>();
        ShowCurrentStep();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote) && !PlayerController.isChatting)
        {
            Destroy(gameObject);
            Destroy(tutorialText);
        }

        if (Input.GetKeyDown(KeyCode.V) && currentStep == 10 && !PlayerController.isChatting) 
        {
            Destroy(gameObject);
            Destroy(tutorialText);
        }

        if (Input.GetKeyDown(KeyCode.V) && currentStep == 0 && !PlayerController.isChatting)
        {
            tutorialCheckPoint.SetActive(true);
            ShowNextStep();
        }

        if (currentStep == 3 && PlayerController.runningAnimation == "Running")
        {
            ShowNextStep();
        }
        else if (currentStep == 4 && PlayerController.runningAnimation == "Emoji")
        {         
            ShowNextStep();     
        }
        else if (currentStep == 5 && PlayerController.isChatting)
        {  
            tutorialCheckPoint.SetActive(true);
            ShowNextStep();
            transform.position = pianoposition;
        }
        else if (currentStep == 6 && PlayerController.isPlayingPiano)
        {
            ShowNextStep();
            transform.position = sitposition;
        }
        else if (currentStep == 7 && PlayerController.isSitting)
        {
            ShowNextStep();
            transform.position = grabposition;
        }
    
        PlayerController.sitText2.enabled = false;
        PlayerController.sitText.enabled = false;
        PlayerController.pianoText.enabled = false;
        PlayerController.grabText.enabled = false;

        setTutorialVisibility();
    }

    void setTutorialVisibility()
    {
        if ( (PlayerController.isPaused )   )
        {
            displayText.enabled = false;
        }
        else
        {
            displayText.enabled = true;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<PhotonView>().IsMine && other.gameObject.tag =="Player")
        {
            if (currentStep==1)
            {
                ShowNextStep();
                tutorialJumpCheckpoint.SetActive(true);
            }

            else if (currentStep==8 && PlayerController.runningAnimation == "Grabbing")
            {
                ShowNextStep();
                transform.position = doorposition;
            }

            else if (currentStep==9 && PlayerController.tutorialDoorIsClosed==false)
            {
                ShowNextStep();
                tutorialCheckPoint.SetActive(false);
            }
        }     
    }

    private void ShowCurrentStep()
    {
        if (currentStep >= 0 && currentStep < textSteps.Count)
        {
            TextStep step = textSteps[currentStep];
            displayText.text = step.stepText;
        }        
    }

    public void ShowNextStep()
    {
        currentStep++;
        if (currentStep < textSteps.Count)
        {
            ShowCurrentStep();
        }
        else
        {
            // Son adýma gelindiðinde texti kapat
            Destroy(gameObject);
            Destroy(tutorialText);
        }
    }
}
