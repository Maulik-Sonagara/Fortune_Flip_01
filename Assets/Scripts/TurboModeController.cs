using UnityEngine;
using UnityEngine.UI;

public class TurboModeController : MonoBehaviour
{
    public static TurboModeController Instance;

    [Header("UI")]
    public Toggle turboToggle;

    [Header("Turbo Settings")]
    public bool isTurboOn = false;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (turboToggle != null)
            turboToggle.onValueChanged.AddListener(OnTurboToggleChanged);
    }

    private void OnTurboToggleChanged(bool value)
    {
        isTurboOn = !value;

        // Tell audio manager
        AudioManager.Instance.SetTurboMode(isTurboOn);
        Debug.Log("Turbo Mode: " + (isTurboOn ? "ON" : "OFF"));
    }
}
