using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardFlip : MonoBehaviour
{
    public static CardFlip Instance { get; private set; }

    [Header("Card Faces")]
    public Image frontImage;  // "Image"
    public Image backImage;   // "ImageBack"
    public float flipDuration = 0.5f;

    [Header("Glow Settings")]
    public Color glowColor = Color.red;
    public float glowDuration = 1.0f;
    public bool isHitCard = false;

    public Image glowBorder;
    private bool isFront = true;
    private bool isFlipping = false;
    public bool isFlipped = false;

    public bool IsFront => isFront;

    private void Awake()
    {
        Instance = this;

        // Automatically find the GlowBorder child
        Transform glow = transform.Find("GlowBorder");
        if (glow != null)
        {
            glowBorder = glow.GetComponent<Image>();
            if (glowBorder != null)
                glowBorder.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"{name}: GlowBorder child not found!");
        }
    }

    public void SetFaceUp(bool showFront)
    {
        isFront = showFront;
        frontImage.gameObject.SetActive(showFront);
        backImage.gameObject.SetActive(!showFront);
        transform.rotation = Quaternion.identity;
    }

    public void FlipCard()
    {
        if (isFlipping) return;
        AudioManager.Instance.PlayFlip();
        StartCoroutine(FlipAnimation());
    }

    private IEnumerator FlipAnimation()
    {
        isFlipping = true;

        // 🔹 Apply Turbo Mode
        float turboMultiplier = (TurboModeController.Instance != null && TurboModeController.Instance.isTurboOn) ? 2f : 1f;
        float adjustedDuration = flipDuration / turboMultiplier;

        float halfTime = adjustedDuration / 2f;
        Quaternion startRot = Quaternion.identity;
        Quaternion midRot = Quaternion.Euler(0, 90, 0);
        Quaternion endRot = Quaternion.identity;

        float t = 0f;

        // Rotate to 90°
        while (t < halfTime)
        {
            t += Time.deltaTime;
            float progress = t / halfTime;
            transform.rotation = Quaternion.Lerp(startRot, midRot, progress);
            yield return null;
        }

        // Swap faces midway
        isFront = !isFront;
        frontImage.gameObject.SetActive(isFront);
        backImage.gameObject.SetActive(!isFront);
        isFlipped = true;

        // Rotate back to 0°
        t = 0f;
        while (t < halfTime)
        {
            t += Time.deltaTime;
            float progress = t / halfTime;
            transform.rotation = Quaternion.Lerp(midRot, endRot, progress);
            yield return null;
        }

        isFlipping = false;

        if (isHitCard)
            StartCoroutine(GlowEffect());
    }


    private IEnumerator GlowEffect()
    {
        if (glowBorder == null) yield break;

        glowBorder.gameObject.SetActive(true);
        glowBorder.GetComponent<RectTransform>().localPosition = Vector3.zero;
        float hue = 0f;
        float pulseTimer = 0f;

        while (isHitCard)
        {
            // Shift hue over time for rainbow animation
            hue += Time.deltaTime * 0.25f; // hue cycle speed
            if (hue > 1f) hue -= 1f;

            // Calculate pulsing alpha
            pulseTimer += Time.deltaTime * (1f / glowDuration);
            float alpha = Mathf.Abs(Mathf.Sin(pulseTimer * Mathf.PI)); // smooth pulse (0→1→0)

            // Apply color (HSV to RGB)
            Color shiftingColor = Color.HSVToRGB(hue, 1f, 1f);
            shiftingColor.a = alpha;

            glowBorder.color = shiftingColor;

            yield return null; // wait for next frame
        }

        glowBorder.gameObject.SetActive(false);
    }


    public void ResetCard()
    {
        isHitCard = false;
        isFlipped = false;
        if (glowBorder != null)
            glowBorder.gameObject.SetActive(false);
    }
}
