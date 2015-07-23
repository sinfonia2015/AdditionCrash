using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    [ExecuteInEditMode()]
    public class AUICanvasScaleManager : MonoBehaviour
    {
        public CanvasScaler canvasScaler;

        public float baseWidth = 750f;

        public float baseHeight = 1333f;

        public float minScale = 0.9f;

        public float scale = 1.0f;

        // Use this for initialization
        void Awake()
        {
			canvasScaler.matchWidthOrHeight = 0.0f;

			//AUIManager.OnScreenSizeChanged += OnScreenSizeChanged;

			SetCanvasScale ();
        }

		void OnDestroy()
		{
			//AUIManager.OnScreenSizeChanged -= OnScreenSizeChanged;		
		}

        IEnumerator Start()
        {
            while (canvasScaler == null)
            {
                yield return 1;
            }

            int width = Mathf.Min(Screen.width, Screen.height);

            scale = width / baseWidth;

            if (canvasScaler != null && EventSystem.current != null)
                EventSystem.current.pixelDragThreshold = (int)(EventSystem.current.pixelDragThreshold * canvasScaler.scaleFactor);
        }

		void OnScreenSizeChanged()
		{
			SetCanvasScale ();
		}

		public void SetCanvasScale()
		{
			if(Mathf.Min(Screen.width, Screen.height) < baseWidth)
            {
                canvasScaler.referenceResolution = new Vector2(Screen.width, canvasScaler.referenceResolution.y);
            }
            else
            {
                if(Screen.width < Screen.height)
                    canvasScaler.referenceResolution = new Vector2(baseWidth, canvasScaler.referenceResolution.y);
                else
                    canvasScaler.referenceResolution = new Vector2(baseHeight, canvasScaler.referenceResolution.y);
            }
		}

        // Update is called once per frame
#if UNITY_EDITOR

        int _width;

        void Update()
        {
            int width = Mathf.Min(Screen.width, Screen.height);

            if (_width != width)
            {
                _width = width;
            }

            //scale = width / baseWidth;
        }
#endif 

        /*void Awake()
        {
            if (Screen.width > Screen.height)
            {
                canvasScaler.matchWidthOrHeight = (float)Screen.height / (float)Screen.width;
            }
            else
            {
                canvasScaler.matchWidthOrHeight = 0f;
            }
        }

        void OnEnable()
        {
            AUIManager.OnScreenSizeChanged += OnScreenSizeChanged;
        }

        void OnDisable()
        {
            AUIManager.OnScreenSizeChanged -= OnScreenSizeChanged;
        }

        void OnScreenSizeChanged()
        {
            Set();
        }

        void Set()
        {
            if (Screen.width > Screen.height)
            {
                canvasScaler.matchWidthOrHeight = 1.0f - (float)Screen.height / (float)Screen.width;
            }
            else
            {
                canvasScaler.matchWidthOrHeight = 0f;
            }

            Debug.Log("#### Size Set : " + canvasScaler.matchWidthOrHeight);

        }

#if UNITY_EDITOR

        int _width;

        void Update()
        {
            int width = Mathf.Min(Screen.width, Screen.height);

            if (_width != width)
            {
                Set();

                _width = width;
            }
        }
#endif*/
    }
}