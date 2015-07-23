using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUIAppsPreviewCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Video Video;

        public AUIRawImageTextureSetter videoThumbnail;

        public AUIRawImageTextureSetter appIcon;

        public Text textAppName, textGameCategory;

        public event System.Action<Fresvii.AppSteroid.Models.App> OnTapAppButtonAtVideoUI;

        public void SetPreview(Fresvii.AppSteroid.Models.Video video)
        {
            this.Video = video;

            videoThumbnail.delayCount = 2;

            videoThumbnail.Set(this.Video.ThumbnailUrl);
        
            if (this.Video.App != null)
            {
                appIcon.Set(this.Video.App.IconUrl);

                textAppName.text = this.Video.App.Name;

                if (this.Video.App.GameGenres.Count > 0)
                    textGameCategory.text = this.Video.App.GameGenres[0].Name;
            }
          
        }

        public void PlayVideo()
        {
            if (this.Video != null) 
            {
                FASVideo.Play(this.Video, (_video, button) => 
                {
					this.Video = _video;

					SetPreview(this.Video);

					if (button == Util.MoviePlayer.TappedButton.App)
                    {
                        if (OnTapAppButtonAtVideoUI != null)
                        {
                            OnTapAppButtonAtVideoUI(this.Video.App);
                        }
                    }
                }); 

				FASVideo.IncrementVideoPlaybackCount(this.Video.Id, (_video, error)=>{
					
					if(error == null)
					{
						this.Video = _video;

						SetPreview(this.Video);
					}
					
				});
            }
        }

    }
}