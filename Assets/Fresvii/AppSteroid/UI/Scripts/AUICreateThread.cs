using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUICreateThread : MonoBehaviour
    {
        public AUIRawImageTextureSetter clipImage;

        public InputField commentInputFiled;

        public Button buttonCreateNewThread;

        private Fresvii.AppSteroid.Models.Video video = null;

        public GameObject prfbVideoList;

        public AUIFrame frameTween;

        [HideInInspector]
        public AUIForum auiForum;

        public InputField titleInputField;

        void Start()
        {
            ValidateCreateNewThread();
        }

        void OnEnable()
        {
            AUIManager.OnEscapeTapped += GoToForum;
        }

        void OnDisable()
        {
            AUIManager.OnEscapeTapped -= GoToForum;
        }

        void OnDestroy()
        {

        }

        public void OnEndEdit(string text)
        {
            ValidateCreateNewThread();
        }

        public void OnClickCreateNewThread()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("Offline"), delegate(bool del) { });

                return;
            }

            GoToForum();

            AUIForumThreadCell cell;

            Fresvii.AppSteroid.Models.Thread _thread = new Fresvii.AppSteroid.Models.Thread();

            _thread.User = FAS.CurrentUser;

            _thread.CreatedAt = _thread.LastUpdateAt = _thread.UpdateAt = System.DateTime.Now;

            _thread.Comment = new  Fresvii.AppSteroid.Models.Comment();

            _thread.Comment.Video = video;

            _thread.Comment.Text = commentInputFiled.text;

            _thread.Title = titleInputField.text;

            clipImage.dontDestroy = true;

            cell = auiForum.CreateNewThreadCell(_thread, clipImage.GetTexture());

            auiForum.MoveToTopThread();

            if (video != null)
            {                
                Fresvii.AppSteroid.FASForum.CreateVideoThread(titleInputField.text, commentInputFiled.text, video.Id, (thread, error) =>
                {
                    if (error != null)
                    {
                        CreateThreadError(error);

                        auiForum.RemoveCell(cell);
                    }
                    else
                    {
                        cell.SetThraed(thread);
                    }
                });
            }
            else
            {
                if (clipImage.GetTexture() != null)
                {
                    Fresvii.AppSteroid.Util.DialogManager.ShowProgressSpinnerDialog("", FASText.Get("Uploading"), false);
                }

                Fresvii.AppSteroid.FASForum.CreateThread(titleInputField.text, commentInputFiled.text, clipImage.GetTexture(), (thread, error) =>
                {
                    Fresvii.AppSteroid.Util.DialogManager.HideProgressSpinnerDialog();

                    if (error != null)
                    {
                        CreateThreadError(error);

                        auiForum.RemoveCell(cell);
                    }
                    else
                    {
                        cell.SetThraed(thread);
                    }
                });
            }
        }

        public void GoToForum()
        {
            auiForum.gameObject.SetActive(true);

            RectTransform rectTransform = GetComponent<RectTransform>();

            auiForum.GetComponent<AUIFrame>().Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

            GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
            {
                Destroy(this.gameObject);
            });
        }

        private void CreateThreadError(Fresvii.AppSteroid.Models.Error error)
        {
            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("ThreadCreateError"), delegate(bool del) { });

            if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error)
            {
                Debug.LogError(error.ToString());
            }
        }

        public void OnClickDeleteClipImage()
        {
            clipImage.ReleaseTexture();

            this.video = null;

            ValidateCreateNewThread();
        }

        public void OnClickSelectItem()
        {          
            List<string> buttons = new List<string>();

            buttons.Add(FASText.Get("ChoosePhoto"));

            buttons.Add(FASText.Get("TakePhoto"));

            buttons.Add(FASText.Get("ChooseMovie"));
            
            AUIActionSheet.Show(buttons.ToArray(), (button) => 
            {
                if (button == FASText.Get("ChoosePhoto"))
                {
                    Fresvii.AppSteroid.Util.ImagePicker.Show(this, Util.ImagePicker.Type.Gallery, (texture) =>
                    {
                        if (texture != null)
                        {
                            clipImage.ReleaseTexture();

                            clipImage.SetTexture(texture);
                        }

                        ValidateCreateNewThread();
                    });
                }
                else if (button == FASText.Get("TakePhoto"))
                {
                    Fresvii.AppSteroid.Util.ImagePicker.Show(this, Util.ImagePicker.Type.Camera, (texture) =>
                    {
                        if (texture != null)
                        {
                            clipImage.ReleaseTexture();

                            clipImage.SetTexture(texture);
                        }

                        ValidateCreateNewThread();
                    });
                }
                else if (button == FASText.Get("ChooseMovie"))
                {
                    GoToVideoList();
                }
            });
        }

        void GoToVideoList()
        {
            if (frameTween.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            GameObject go = Instantiate(prfbVideoList) as GameObject;

            go.GetComponent<RectTransform>().SetParent(transform.parent, false);

            go.transform.SetAsLastSibling();

            AUIVideoList videoList = go.GetComponent<AUIVideoList>();

            videoList.IsModal = true;

            videoList.backButtonText.text = FASText.Get("Cancel");

            videoList.parentFrameTween = this.frameTween;

            videoList.mode = AUIVideoList.Mode.Select;

            videoList.frameTween.Animate(new Vector2(0f, -rectTransform.rect.height), Vector2.zero, () =>
            {
                this.gameObject.SetActive(false);
            });

            videoList.OnVideoSelected += (video, thumbnail) =>
            {
                if (video != null)
                {
                    clipImage.ReleaseTexture();

                    clipImage.SetTexture(thumbnail);

                    this.video = video;
                }

                ValidateCreateNewThread();
            };
        }

        private void ValidateCreateNewThread()
        {
            if (!string.IsNullOrEmpty(commentInputFiled.text) || clipImage.GetTexture() != null || video != null)
            {
                buttonCreateNewThread.interactable = true;
            }
            else
            {
                buttonCreateNewThread.interactable = false;
            }

            clipImage.gameObject.SetActive(clipImage.GetTexture() != null);
        }
    }
}