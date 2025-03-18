
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class TriggerForObjects : MonoBehaviour
{
    [HideInInspector] public static bool canPickUp;
    [HideInInspector] public static bool canSit;
    [HideInInspector] public static bool canPlayPiano;

    List<GameObject> insideObjects = new List<GameObject>();

    [HideInInspector] public static bool canOpenAndClose;

    void Start()
    {
        canPickUp = false;
        canSit = false;
        canPlayPiano = false;
        canOpenAndClose = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!insideObjects.Contains(other.gameObject) && other.gameObject.tag == "Player")
            insideObjects.Add(other.gameObject);

        if (other.GetComponent<PhotonView>().IsMine)
        {
            if (other.gameObject.tag == "Player" && gameObject.tag == "GrabbableItem")
            {
                canPickUp = true;
                PlayerController.itemWantToGrab = gameObject;
            }
            else if (other.gameObject.tag == "Player" && gameObject.tag == "ObjectForSit" && insideObjects.Count == 1)
            {
                canSit = true;
                PlayerController.objectWantToSit = gameObject;
                PlayerController.sitText2.enabled = true;
            }
            else if (other.gameObject.tag == "Player" && gameObject.tag == "PianoSit" && insideObjects.Count == 1)
            {
                canPlayPiano = true;
                PlayerController.PianoToSit = gameObject;
                PlayerController.pianoText.enabled = true;
            }
            else if (other.gameObject.tag == "Player" && gameObject.tag == "TutorialDoor")
            {
                canOpenAndClose = true;
            }
            else if (other.gameObject.tag == "Player" && gameObject.tag == "InfinityWall")
            {
                GameObject.FindWithTag("TutorialScene").SetActive(false);

                if (Tutorial.currentStep != 10 && GameObject.FindWithTag("TextBubble") != null)
                {
                    Destroy(Tutorial.displayText.gameObject);
                    Destroy(GameObject.FindWithTag("TextBubble"));
                    Destroy(GameObject.Find("TutorialManager"));
                }                    

                foreach (GameObject door in GameObject.FindGameObjectsWithTag("DFirstDoor"))
                    door.GetComponent<MeshCollider>().enabled = false;

                Vector3 teleportationPoint = new Vector3(-62.5f, 58f, -15.5f);
                other.gameObject.GetComponent<CharacterController>().Move(teleportationPoint - other.gameObject.transform.position);
                other.gameObject.transform.eulerAngles = new Vector3(0, -90f, 0);

                foreach (GameObject door in GameObject.FindGameObjectsWithTag("DFirstDoor"))
                    door.GetComponent<MeshCollider>().enabled = true;
            }
        }   
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<PhotonView>().IsMine)
        {
            if (other.gameObject.tag == "Player" && gameObject.tag == "GrabbableItem")
            {
                PlayerController.itemWantToGrab = gameObject;
            }
            else if (other.gameObject.tag == "Player" && gameObject.tag == "ObjectForSit" && insideObjects.Count == 1)
            {
                PlayerController.objectWantToSit = gameObject;
                canSit = true;

                if (other.gameObject.GetComponent<PlayerController>().IsNotLooking(PlayerController.objectWantToSit))
                {
                    PlayerController.sitText.enabled = true;
                    PlayerController.sitText2.enabled = false;
                }         
                else
                {
                    PlayerController.sitText2.enabled = true;
                }
            }
            else if (other.gameObject.tag == "Player" && gameObject.tag == "PianoSit" && insideObjects.Count == 1)
            {
                PlayerController.PianoToSit = gameObject;
                canPlayPiano = true;

                if (other.gameObject.GetComponent<PlayerController>().IsLooking(PlayerController.PianoToSit))
                    PlayerController.pianoText.enabled = true;
                else
                    PlayerController.pianoText.enabled = false;
            }
            else if (insideObjects.Count > 1)
            {
                canSit = false;
                PlayerController.objectWantToSit = null;

                canPlayPiano = false;
                PlayerController.PianoToSit = null;
                PlayerController.pianoText.enabled = false;
            }        
        }  
    }

    private void OnTriggerExit(Collider other)
    {
        if (insideObjects.Contains(other.gameObject))
            insideObjects.Remove(other.gameObject);

        if (other.GetComponent<PhotonView>().IsMine)
        {
            if (other.gameObject.tag == "Player" && gameObject.tag == "GrabbableItem")
            {
                canPickUp = false;
                PlayerController.itemWantToGrab = null;
            }
            else if (other.gameObject.tag == "Player" && gameObject.tag == "ObjectForSit")
            {
                canSit = false;
                PlayerController.objectWantToSit = null;
                PlayerController.sitText2.enabled = false;
            }
            else if (other.gameObject.tag == "Player" && gameObject.tag == "PianoSit")
            {
                canPlayPiano = false;
                PlayerController.PianoToSit = null;
                PlayerController.pianoText.enabled = false;
            }
            else if (other.gameObject.tag == "Player" && gameObject.tag == "TutorialDoor")
            {
                canOpenAndClose = false;
            }
        }
    }
}
