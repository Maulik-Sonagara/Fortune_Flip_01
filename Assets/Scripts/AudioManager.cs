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

    [Header("Turbo Settings")]
    public float normalPitch = 1f;
    public float turboPitch = 1.3f; // 30% faster music in turbo
    public float musicTransitionSpeed = 2f; // how fast pitch transitions

    private bool isTurboActive = false;
    private float targetPitch = 1f;

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

    private void Update()
    {
        // Smooth pitch transition
        if (bgMusicSource != null)
            bgMusicSource.pitch = Mathf.Lerp(bgMusicSource.pitch, targetPitch, Time.deltaTime * musicTransitionSpeed);
    }

    public void PlayCardSpread() => PlaySound(cardSpreadSource);
    public void PlayFlip() => PlaySound(flipSource);
    public void PlayJoker() => PlaySound(JokerSource);
    public void PlayWin() => PlaySound(winSource);
    public void PlayLose() => PlaySound(loseSource);

    private void PlaySound(AudioSource source)
    {
        if (source != null && source.clip != null)
        {
            source.pitch = Random.Range(0.95f, 1.05f);
            source.PlayOneShot(source.clip, Random.Range(0.9f, 1.1f));
        }
    }

    // Called when turbo is toggled
    public void SetTurboMode(bool active)
    {
        isTurboActive = active;
        targetPitch = isTurboActive ? turboPitch : normalPitch;
    }
}
