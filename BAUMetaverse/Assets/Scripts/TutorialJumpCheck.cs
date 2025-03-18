
using UnityEngine;

public class TutorialJumpCheck : MonoBehaviour
{
    [SerializeField] GameObject tutorialObject;
    [SerializeField] GameObject tutorialCheckpoint;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag=="Player" && Tutorial.currentStep==2)
        {
            tutorialObject.GetComponent<Tutorial>().ShowNextStep();
            tutorialCheckpoint.SetActive(false);
            Destroy(gameObject);
        }
    }
    
}
