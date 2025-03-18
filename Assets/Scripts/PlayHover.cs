using UnityEngine;
using UnityEngine.EventSystems;

public class PlayHover : MonoBehaviour, IPointerEnterHandler
{
    SoundManager soundManager;

    private void Awake()
    {
        soundManager = GameObject.FindWithTag("SoundManager")?.GetComponent<SoundManager>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (soundManager != null)
        {
            soundManager.playHoverSound();
        }
    }
}
