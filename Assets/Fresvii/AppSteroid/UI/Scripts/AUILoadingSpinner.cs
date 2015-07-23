using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUILoadingSpinner : MonoBehaviour
    {
        public float speed = 180f;

        private RectTransform rectTransform;

        // Update is called once per frame
        void Start()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        void Update()
        {
            rectTransform.localEulerAngles += Vector3.forward * speed * Time.deltaTime;
        }
    }
}