using UnityEngine;
using UnityEngine.UI;

public class AudioToggle : MonoBehaviour
{
    [Header("UI Toggle")]
    public Toggle audioToggle; // assign in Inspector

    private void Start()
    {
        // Load saved preference (1 = unmuted, 0 = muted)
        bool isMuted = PlayerPrefs.GetInt("AudioMuted", 0) == 1;
        audioToggle.isOn = !isMuted;

        // Apply audio state
        SetAudio(!isMuted);

        // Listen for toggle value change
        audioToggle.onValueChanged.AddListener(OnAudioToggleChanged);
    }

    private void OnAudioToggleChanged(bool isOn)
    {
        SetAudio(isOn);

        // Save user preference
        PlayerPrefs.SetInt("AudioMuted", isOn ? 0 : 1);
        PlayerPrefs.Save();
    }

    private void SetAudio(bool enable)
    {
        if (AudioManager.Instance == null) return;

        // Mute / unmute all audio sources
        AudioManager.Instance.cardSpreadSource.mute = !enable;
        AudioManager.Instance.flipSource.mute = !enable;
        AudioManager.Instance.winSource.mute = !enable;
        AudioManager.Instance.JokerSource.mute = !enable;
        AudioManager.Instance.loseSource.mute = !enable;
        AudioManager.Instance.bgMusicSource.mute = !enable;
    }
}
