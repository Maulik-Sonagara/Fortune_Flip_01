using UnityEngine;
using UnityEngine.UI;
using System;

public class TurboModeController : MonoBehaviour
{
    public static TurboModeController Instance;

    [Header("UI")]
    public Toggle turboToggle;

    [Header("Turbo Settings")]
    public bool isTurboOn = false;

    public event Action<bool> OnTurboChanged;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (turboToggle != null)
        {
            turboToggle.onValueChanged.AddListener(OnTurboToggleChanged);
        }
    }

    private void OnTurboToggleChanged(bool value)
    {
        isTurboOn = !value; 
        AudioManager.Instance.SetTurboMode(isTurboOn);
        OnTurboChanged?.Invoke(isTurboOn);

        Debug.Log("Turbo Mode: " + (isTurboOn ? "ON" : "OFF"));
    }
}
