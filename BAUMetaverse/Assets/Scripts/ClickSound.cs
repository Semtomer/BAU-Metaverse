
using UnityEngine;

public class ClickSound : MonoBehaviour
{
    AudioSource clickChannel;

    public void playClickSound()
    {
        clickChannel = GetComponent<AudioSource>();
        clickChannel.Play();
    }
}
