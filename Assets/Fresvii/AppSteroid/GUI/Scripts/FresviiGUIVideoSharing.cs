using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Fresvii.AppSteroid.Gui
{
    public class FresviiGUIVideoSharing : MonoBehaviour
    {
        private static FresviiGUIVideoSharing instance;

        public int guiDepth;

        public Camera videoGuiCamera;

        private int screenWidth;

        public CanvasScaler canvasScaler;

        public Vector2 canvasScalerMatch;

        public Canvas canvas;

        [HideInInspector]
        public string returnSceneName;

        public EventSystem eventSystem;

		private bool onlySharing = false;

        public FresviiGUIShareVideo shareVideo;

        public FresviiGUIUploadedVideo uploadedVideo;

		public static void Show(int guiDepth, Fresvii.AppSteroid.Models.Video video, Texture2D videoThumbnail)
        {
            if (instance != null)
            {
				Destroy(instance.gameObject);
            }

            Instantiate(Resources.Load("GuiPrefabs/VideoSharingGUI"));

            instance.guiDepth = guiDepth;

            instance.onlySharing = true;

            instance.shareVideo.Set(instance, video, videoThumbnail);
        }

		private bool goToUploaded = false;

        public static void Hide()
        {
            if (instance == null) return;

            Destroy(instance.gameObject);
        }

        void Awake()
        {
			FresviiGUIText.SetUp(Application.systemLanguage);

			if(instance != null)
			{
				Destroy(instance.gameObject);
			}

            instance = this;

			if(FASPlayVideo.videoShareGuiMode == FASPlayVideo.VideoShareGuiMode.WithLegacyGUI)
			{
				instance.canvas.renderMode = RenderMode.ScreenSpaceCamera;
			}
			else if(FASPlayVideo.videoShareGuiMode == FASPlayVideo.VideoShareGuiMode.WithUGUI)
			{
				instance.canvas.renderMode = RenderMode.ScreenSpaceOverlay;				
			}

			instance.gameObject.transform.SetAsLastSibling();
        }

		IEnumerator Start(){

            if (onlySharing)
            {
                this.returnSceneName = "";
            }
            else
            {
                if (!System.IO.File.Exists(FASVideo.VideoFilePath))
                {
                    goToUploaded = false;

                    Hide();

                    yield break;
                }

                this.guiDepth = FASVideo.ModalGuiDepth;

                this.returnSceneName = FASGui.ReturnSceneName;

                shareVideo.Set(this, FASVideo.VideoFilePath);
            } 
            
            while (!FAS.Initialized)
			{
				yield return 1;
			}

            if (!FASUser.IsLoggedIn())
            {
                Login(); 
            }
		}

        private int retryCount = 3;

        void Login()
        {
            FASUser.RelogIn((error) =>
            {
                if (error != null)
                {
                    if (error.Code == (int)Fresvii.AppSteroid.Models.Error.ErrorCode.NotSignUp)
                    {
                        Debug.LogError("Video sharing : Not signed up");

                        Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel("Yes", "No", "Close");

                        Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog("Not signed up", (del) =>
                        {
                            
                        });

                        Hide();

                        FASPlayVideo.GuiEndCallback();
                    }
                    else
                    {
                        if (--retryCount > 0)
                        {
                            Login();
                        }
                        else 
                        {
                            Debug.LogError("Unknown");

                            Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel("Yes", "No", "Close");

                            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FresviiGUIText.Get("UnknownError"), (del) =>
                            {

                            });

                            Hide();

                            FASPlayVideo.GuiEndCallback();
                        }
                    }
                }
            });
        }

		void OnDestroy()
        {
            if (!goToUploaded)
            {
                FASPlayVideo.GuiEndCallback();
            }

			onlySharing = false;

			FASGesture.Resume();
		}

        void Update()
        {
            FASGesture.Pause();

			if(EventSystem.current == null){
				
				eventSystem.gameObject.SetActive(true);
				
				EventSystem.current = eventSystem;
			}

            if (screenWidth != Screen.width)
            {
                screenWidth = Screen.width;

                if (Screen.width > Screen.height)
                {
                    canvasScaler.matchWidthOrHeight = canvasScalerMatch.y;
                }
                else
                {
                    canvasScaler.matchWidthOrHeight = canvasScalerMatch.x;
                }
            }
        }

        void OnGUI()
        {
            if (FASPlayVideo.videoShareGuiMode != FASPlayVideo.VideoShareGuiMode.WithLegacyGUI)
            {
                return;
            }

            videoGuiCamera.enabled = true;

            GUI.depth = guiDepth;

            if (Event.current.type == EventType.Repaint)
            {
                videoGuiCamera.Render();
            }

            if (Event.current.isMouse && Event.current.button >= 0)
            {
                Event.current.Use();
            }
        }
    }
}