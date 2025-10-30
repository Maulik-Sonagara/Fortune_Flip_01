using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardFlip : MonoBehaviour
{
    public Image frontImage;
    public Image backImage;
    public float flipDuration = 0.5f;

    private bool isFront = true;
    private bool isFlipping = false;
    public bool isFlipped = false;

    public bool IsFront => isFront;

    public void SetFaceUp(bool showFront)
    {
        isFront = showFront;
        frontImage.gameObject.SetActive(showFront);
        backImage.gameObject.SetActive(!showFront);
        transform.rotation = Quaternion.identity; // reset rotation
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

        float halfTime = flipDuration / 2f;
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

        // Swap face midway
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
    }
}
