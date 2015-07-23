using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUICommentGridCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Comment Comment { get; protected set; }

        public AUIRawImageTextureSetter image;

        private AUIForum auiForum;

        public void OnEnable()
        {
            if (this.Comment != null)
            {
                StartCoroutine(Set());
            }
        }

        IEnumerator Set()
        {
            yield return 1;

            if (!this.gameObject.activeSelf)
            {
                yield break;
            }

            if (Comment.VideoState == Models.Comment.VideoStatus.Ready)
            {
                image.Set(Comment.Video.ThumbnailUrl);
            }
            else
            {
                image.Set(Comment.ImageThumbnailUrl);
            }
        }

        public void SetComment(Fresvii.AppSteroid.Models.Comment comment, AUIForum auiForum)
        {
            this.Comment = comment;

            this.auiForum = auiForum;

            if (this.gameObject.activeInHierarchy)
            {
                StartCoroutine(Set());
            }
        }

        public void OnClicked()
        {
            auiForum.GoToThread(Comment.ThreadId, Comment, true, false);
        }
    }
}
