using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIVideoList : MonoBehaviour
    {
        public enum Mode { Share, Select }

        public Mode mode = Mode.Share;

        public RectTransform prfbVideoListCell;

        public AUIScrollViewContents contents;

        private List<AUIVideoListCell> videoCells = new List<AUIVideoListCell>();

        public AUIScrollViewPullReflesh pullReflesh;

        private Fresvii.AppSteroid.Models.ListMeta listMeta;

        private bool isPullRefleshProc;

        public AUIFrame frameTween;

        [HideInInspector]
        public AUICommunityTop auiCommunityTop;

        public AUIScrollRect scrollView;

        public Text title;

        [HideInInspector]
        public AUIFrame parentFrameTween;

        public Fresvii.AppSteroid.Models.User User { get; set; }

        private Fresvii.AppSteroid.Models.ListMeta videoListMeta = null;

        public Text videosNum;

        public Text backButtonText;

        public event Action<Fresvii.AppSteroid.Models.Video, Texture2D> OnVideoSelected;

        private Fresvii.AppSteroid.Models.Video selectedVideo;

        private Texture2D selectedVideoThumbnail;

        public GameObject normalBackIcon, modalBackIcon;

        public bool IsModal;

        void OnEnable()
        {
            pullReflesh.OnPullDownReflesh += OnPullDownReflesh;

            pullReflesh.OnPullUpReflesh += OnPullUpReflesh;

            AUIManager.OnEscapeTapped += Back;
        }

        void OnDisable()
        {
            pullReflesh.OnPullDownReflesh += OnPullDownReflesh;

            pullReflesh.OnPullUpReflesh += OnPullUpReflesh;

            AUIManager.OnEscapeTapped -= Back;
        }

        void OnPullDownReflesh()
        {
            isPullRefleshProc = true;

            GetVideoList(0);
        }

        void OnPullUpReflesh()
        {
            if (listMeta != null && listMeta.NextPage.HasValue)
            {
                isPullRefleshProc = true;

                GetVideoList((uint)listMeta.NextPage);
            }
            else
            {
                pullReflesh.PullRefleshCompleted();
            }
        }

        // Use this for initialization
        IEnumerator Start()
        {
            normalBackIcon.SetActive(!IsModal);

            modalBackIcon.SetActive(IsModal);

            if (IsModal)
            {
                GetComponent<AUITabBarManager>().Off();

                contents.padding.bottom = 0;
            }

            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            if (User == null)
            {
                videosNum.text = FASText.Get("VideosUploadedNumber").Replace("0", FAS.CurrentUser.VideosCount.ToString());
            }
            else
            {
                videosNum.text = FASText.Get("VideosUploadedNumber").Replace("0", User.VideosCount.ToString());
            }

            while (GetComponent<AUIFrame>().Animating)
            {
                yield return 1;
            }

            contents.padding.bottom += (AUITabBar.Instance.gameObject.activeInHierarchy) ? AUITabBar.Instance.GetHeight() : 0;

            GetVideoList(1);

            string objectId = (FAS.CurrentUser.Id == this.User.Id) ? "" : this.User.Id;

            FASUtility.SendPageView("pv.my_page.videos", objectId, System.DateTime.UtcNow, (e) =>
            {
                if (e != null)
                    Debug.LogError(e.ToString());
            });    

        }

        void GetVideoList(uint page)
        {
            if (User == null)
            {
                videosNum.text = FASText.Get("VideosUploadedNumber").Replace("0", FAS.CurrentUser.VideosCount.ToString());

                FASVideo.GetCurrentUserVideoList(page, OnGetVideoList);
            }
            else
            {
                videosNum.text = FASText.Get("VideosUploadedNumber").Replace("0", User.VideosCount.ToString());

                string query = "{\"where\":[{\"collection\":\"users\", \"column\":\"id\", \"value\":\"" + User.Id + "\"}]}";

                FASVideo.GetVideoList(page, query, OnGetVideoList);
            }
        }

        IEnumerator SetContentPadding()
        {
            yield return new WaitForEndOfFrame();

            contents.padding.bottom += (AUITabBar.Instance.gameObject.activeInHierarchy) ? AUITabBar.Instance.GetHeight() : 0;
        }

        private void OnGetVideoList(IList<Fresvii.AppSteroid.Models.Video> videos, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            if (this == null)
            {
                return;
            }

            if (this.enabled == false)
            {
                return;
            }

            if (error != null)
            {
                if (FASSettings.Instance.logLevel <= FAS.LogLevels.Error)
                {
                    Debug.LogError(error.ToString());
                }

                return;
            }

            if (this.listMeta == null)
            {
                this.listMeta = meta;
            }
            else if (this.listMeta.CurrentPage != 0)
            {
                this.listMeta = meta;
            }

            videosNum.text = FASText.Get("VideosUploadedNumber").Replace("0", this.listMeta.TotalCount.ToString());

            bool added = false;

            foreach (Fresvii.AppSteroid.Models.Video video in videos)
            {
                added |= UpdateVideo(video);
            }

            if (isPullRefleshProc)
            {
                pullReflesh.PullRefleshCompleted();

                isPullRefleshProc = false;
            }

            Sort();
        }

        IEnumerator DelayLayout()
        {
            yield return 1;

            contents.ReLayout();
        }

        private void Sort()
        {
            // Sort
            videoCells.Sort((a, b) => System.DateTime.Compare(b.Video.CreatedAt, a.Video.CreatedAt));

            foreach (var obj in videoCells)
            {
                obj.transform.SetSiblingIndex(contents.transform.childCount - 1);
            }

            StartCoroutine(DelayLayout());
        }

        private bool UpdateVideo(Fresvii.AppSteroid.Models.Video video)
        {
            AUIVideoListCell cell = videoCells.Find(x => x.Video.Id == video.Id);

            if (cell != null)
            {
                cell.SetVideo(video, mode);

                return false;
            }

            var item = GameObject.Instantiate(prfbVideoListCell) as RectTransform;

            contents.AddItem(item);

            cell = item.GetComponent<AUIVideoListCell>();

            cell.auiVideoList = this;

            cell.SetVideo(video, mode);

            videoCells.Add(cell);

            cell.gameObject.SetActive(false);

            return true;
        }

        public void RemoveCell(AUIVideoListCell cell)
        {
            videoCells.Remove(cell);

            contents.RemoveItem(cell.GetComponent<RectTransform>());

            Destroy(cell.gameObject);

            StartCoroutine(DelayLayout());
        }

        public void VideoSelected(Fresvii.AppSteroid.Models.Video video, Texture2D thumbnail)
        {
            this.selectedVideo = video;

            this.selectedVideoThumbnail = thumbnail;

            Back();
        }

        public GameObject prfbMyPage, prfbUserPage;

        public void GoToUserPage(Fresvii.AppSteroid.Models.User user)
        {
            if (frameTween.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            if (user.Id == FAS.CurrentUser.Id)
            {
                AUIMyPage myPage = ((GameObject)Instantiate(prfbMyPage)).GetComponent<AUIMyPage>();

                myPage.transform.SetParent(transform.parent, false);

                myPage.transform.SetAsLastSibling();

                myPage.parentFrameTween = this.frameTween;

                myPage.backButtonText.text = title.text;

                myPage.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });
            }
            else
            {
                AUIUserPage userPage = ((GameObject)Instantiate(prfbUserPage)).GetComponent<AUIUserPage>();

                userPage.transform.SetParent(transform.parent, false);

                userPage.Set(user, title.text, this.frameTween);

                userPage.transform.SetAsLastSibling();

                userPage.GetComponent<AUIFrame>().Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });
            }

            this.frameTween.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }

        public void Back()
        {
            if (frameTween.Animating) return;

            parentFrameTween.gameObject.SetActive(true);

            RectTransform rectTransform = GetComponent<RectTransform>();

            if (mode == Mode.Share)
            {
                parentFrameTween.Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

                GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
                {
                    Destroy(this.gameObject);
                });
            }
            else if (mode == Mode.Select)
            {
                if (OnVideoSelected != null)
                {
                    if (OnVideoSelected != null)
                    {
                        OnVideoSelected(selectedVideo, selectedVideoThumbnail);
                    }
                }

                GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(0f, -rectTransform.rect.height), () =>
                {
                    Destroy(this.gameObject);
                });
            }
        }
    }
}