using UnityEngine;
using UnityEngine.UI;

namespace TexanGame
{
    public class CanvasScalerAdjuster : MonoBehaviour
    {
        public CanvasScaler canvasScaler;  // Reference to the Canvas Scaler component
        public float phoneMatchValue = 0f; // Match value for phones (width priority)
        public float tabletMatchValue = 1f; // Match value for tablets (height priority)
        public float windowsMatchValue = 0.5f;

        private Vector2 lastResolution;    // Store the last screen resolution to detect changes

        void Start()
        {
            lastResolution = new Vector2(Screen.width, Screen.height);
            AdjustCanvasScaler();
        }

        void Update()
        {
            // Check if the screen resolution has changed
            if (Screen.width != lastResolution.x || Screen.height != lastResolution.y)
            {
                lastResolution = new Vector2(Screen.width, Screen.height);
                AdjustCanvasScaler(); // Recalculate and adjust the canvas scaler
            }
        }

        void AdjustCanvasScaler()
        {
           /* if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                // Set match value for Windows devices
                canvasScaler.matchWidthOrHeight = windowsMatchValue;
            }*/
            if (IsTabletDevice())
            {
                // Set match value for tablets
                canvasScaler.matchWidthOrHeight = tabletMatchValue;
            }
            else
            {
                // Set match value for phones
                canvasScaler.matchWidthOrHeight = phoneMatchValue;
            }
        }

        bool IsTabletDevice()
        {

            // Check screen diagonal size to determine if it's a tablet or phone
            float screenWidth = Screen.width / Screen.dpi;
            float screenHeight = Screen.height / Screen.dpi;
            float screenSize = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));

            //Debug.Log("screenWidth : " + screenWidth + "screenHeight : " + screenHeight + "screenSize : " + screenSize);

            if(screenSize >= 6.5f)
            {
                if(screenHeight < 4.5f)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            else
            {
                return false ;
            }

            // A threshold of around 6.5 inches can be considered as a tablet
           // return screenSize >= 6.5f;
        }
    }
}
