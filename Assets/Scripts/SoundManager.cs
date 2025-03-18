
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    AudioSource mainSource;
    AudioSource oneShotSounds;

    [SerializeField] AudioClip clickSound, hoverSound;

    [SerializeField] Button buttonQuarter;
    [SerializeField] Button buttonHalf;
    [SerializeField] Button buttonFull;
    [SerializeField] Button SoundOn;
    [SerializeField] Button SoundOff;

    [SerializeField] Color blueColor;

    private void Awake()
	{
        if (gameObject.tag == "SoundManager")
        {
            mainSource = GetComponents<AudioSource>()[0];
            oneShotSounds = GetComponents<AudioSource>()[1];

            soundHalf();
        }
        else
        {
            GameObject soundManager = GameObject.FindWithTag("SoundManager");
            mainSource = soundManager.GetComponents<AudioSource>()[0];
            oneShotSounds = soundManager.GetComponents<AudioSource>()[1];
        }           
    }

    public void playClickSound()
    {
        oneShotSounds.PlayOneShot(clickSound);
    }

    public void playHoverSound()
    {
        oneShotSounds.PlayOneShot(hoverSound);
    }

    public void soundFull()
    {
        mainSource.volume = 1f;
        buttonQuarter.image.color = blueColor;
        buttonHalf.image.color = blueColor;
        buttonFull.image.color = blueColor;
        SoundOff.gameObject.SetActive(false);
        SoundOn.gameObject.SetActive(true);
    }

    public void soundHalf()
    {
        mainSource.volume = 0.5f;
        buttonQuarter.image.color = blueColor;
        buttonHalf.image.color = blueColor;
        buttonFull.image.color = Color.white;
        SoundOff.gameObject.SetActive(false);
        SoundOn.gameObject.SetActive(true);
    } 

    public void soundQuarter()
    {
        mainSource.volume = 0.25f;
        buttonQuarter.image.color = blueColor;
        buttonHalf.image.color = Color.white;
        buttonFull.image.color = Color.white;
        SoundOff.gameObject.SetActive(false);
        SoundOn.gameObject.SetActive(true);

    } 

    public void soundMute()
    {
        mainSource.volume = 0f;
        buttonQuarter.image.color = Color.white;
        buttonHalf.image.color = Color.white;
        buttonFull.image.color = Color.white;
        SoundOff.gameObject.SetActive(true);
        SoundOn.gameObject.SetActive(false);
    }
}
