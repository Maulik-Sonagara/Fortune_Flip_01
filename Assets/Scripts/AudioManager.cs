using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource cardSpreadSource;
    public AudioSource flipSource;
    public AudioSource winSource;
    public AudioSource JokerSource;
    public AudioSource loseSource;
    public AudioSource bgMusicSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayCardSpread()
    {
        PlaySound(cardSpreadSource);
    }

    public void PlayFlip()
    {
        PlaySound(flipSource);
    }

    public void PlayJoker()
    {
        PlaySound(JokerSource);
    }

    public void PlayWin()
    {
        PlaySound(winSource);
    }

    public void PlayLose()
    {
        PlaySound(loseSource);
    }

    private void PlaySound(AudioSource source)
    {
        if (source != null && source.clip != null)
        {
            source.Play();
        }
    }
}
