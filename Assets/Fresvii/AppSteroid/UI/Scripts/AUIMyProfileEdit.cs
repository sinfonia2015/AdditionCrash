using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIMyProfileEdit : MonoBehaviour
    {
        public AUIRawImageTextureSetter auiRawImage;

        public InputField userName;

        public InputField description;

        public Button buttonApply;

        private string initName, initDescription;

        private bool textureEdited;

        private Texture2D loadedTexture;

        [HideInInspector]
        public AUIMyPage auiMyPage;

        public AUIFrame frameTween;

        public RectTransform rectDescription;

        public Vector2 descriptionPortlaitSize, descriptionLandscapeSize;

        // Use this for initialization
        void OnEnable()
        {
            buttonApply.interactable = false;

            StartCoroutine(Init());

            AUIManager.OnScreenSizeChanged += OnScreenSizeChanged;
        }

        void OnDisable()
        {
            AUIManager.Instance.HideLoadingSpinner();

            AUIManager.OnScreenSizeChanged -= OnScreenSizeChanged;
        }

        IEnumerator Init()
        {
            while (!FAS.Initialized)
            {
                yield return 1;
            }

            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            if (FAS.CurrentUser != null)
            {
                auiRawImage.Set(FAS.CurrentUser.ProfileImageUrl);

                initName = userName.text = FAS.CurrentUser.Name;

                initDescription = description.text = FAS.CurrentUser.Description;
            }

            OnScreenSizeChanged();
        }

        void OnScreenSizeChanged()
        {
            if (Screen.width > Screen.height)
            {
                rectDescription.sizeDelta = descriptionLandscapeSize;
            }
            else
            {
                rectDescription.sizeDelta = descriptionPortlaitSize;
            }
        }

        public void GoToMyPage()
        {
            auiMyPage.gameObject.SetActive(true);

            RectTransform rectTransform = GetComponent<RectTransform>();

            auiMyPage.GetComponent<AUIFrame>().Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

            GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
            {
                Destroy(this.gameObject);
            });
        }

        public void OnClickChangeImage()
        {
            List<string> buttons = new List<string>();

            buttons.Add(FASText.Get("ChoosePhoto"));

            buttons.Add(FASText.Get("TakePhoto"));

            AUIActionSheet.Show(buttons.ToArray(), (button) =>
            {
                if (button == FASText.Get("ChoosePhoto"))
                {
                    Fresvii.AppSteroid.Util.ImagePicker.Show(this, Util.ImagePicker.Type.Gallery, (texture) =>
                    {
                        if (texture != null)
                        {
                            auiRawImage.SetTexture(texture);

                            loadedTexture = texture;

                            textureEdited = true;

                            Validate();
                        }

                    });
                }
                else if (button == FASText.Get("TakePhoto"))
                {
                    Fresvii.AppSteroid.Util.ImagePicker.Show(this, Util.ImagePicker.Type.Camera, (texture) =>
                    {
                        if (texture != null)
                        {
                            auiRawImage.SetTexture(texture);

                            loadedTexture = texture;

                            textureEdited = true;

                            Validate();
                        }

                    });
                }
            });
        }

        public void OnClickApplyButton()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), (del) => { });

                return;
            }

            AUIManager.Instance.ShowLoadingSpinner(true);

            FASUser.PatchAccount(userName.text, description.text, (loadedTexture != null) ? loadedTexture : null, (user, error) =>
            {
                AUIManager.Instance.HideLoadingSpinner();

                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                if (error == null)
                {
                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("ProfileUpdateSuccess"), (del)=>{ });

                    GoToMyPage();
                }
                else
                {
                    Debug.LogError(error.ToString());

                    if (error.Code == (int)Fresvii.AppSteroid.Models.Error.ErrorCode.NameHasAlredyBeenTaken)
                    {
                        Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("NameHasAlredyBeenTaken"), (del) => { });

                        userName.text = FAS.CurrentUser.Name;
                    }
                    else
                    {
                        Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("ProfileUpdateError"), (del) => { });
                    }
                }
            });
        }

        public void OnEndEdit()
        {
            Validate();
        }

        void Validate()
        {
            buttonApply.interactable = (initName != userName.text || initDescription != description.text || textureEdited);
        }
    }
}