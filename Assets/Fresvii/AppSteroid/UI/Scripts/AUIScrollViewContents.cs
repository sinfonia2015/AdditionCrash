using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIScrollViewContents : MonoBehaviour
    {
        public RectTransform contentsNode;

        private List<RectTransform> contents = new List<RectTransform>();

        public bool vertical, horizontal;

        public bool grid;

        public Vector2 gridSize;

        public RectOffset padding;

        public float space;

        private Vector2 pivotPos;

        public RectTransform scrollView;

        public RectTransform[] initObjects;

        public RectTransform[] lastObjects;

        public RectOffset paddingPortrait, paddingLandscape;

        public float spacePortrait, spaceLandscape;

        public int gridNum = 4;

        void Start()
        {
            pivotPos = Vector2.zero;

            if(vertical)
            {
                pivotPos.y = - padding.top;

                contentsNode.sizeDelta = new Vector2(contentsNode.sizeDelta.x, - pivotPos.y);
            }

            if (horizontal)
            {
                pivotPos.x = padding.left;

                contentsNode.sizeDelta = new Vector2(pivotPos.x, contentsNode.sizeDelta.y);
            }

            if (grid)
            {
                CalcPadding();
            }

            foreach (RectTransform rectTransform in initObjects)
            {
                AddItem(rectTransform);
            }

            foreach (RectTransform rectTransrom in lastObjects)
            {
                AddItem(rectTransrom);
            }
        }

        bool calcurated;

        void CalcPadding()
        {
            if (!calcurated)
            {
                float gridLength = (Mathf.Min(AUIManager.Instance.sizedCanvas.rect.width, AUIManager.Instance.sizedCanvas.rect.height) - (gridNum - 1) * space - padding.left - padding.right) / gridNum;

                gridSize = new Vector2(gridLength, gridLength);

                calcurated = true;
            }

            int numXGrid = Mathf.FloorToInt(contentsNode.rect.width / gridSize.x);

            float spaceWidth = spaceLandscape * (numXGrid - 1);

            if(spaceWidth + numXGrid * gridSize.x > contentsNode.rect.width)
            {
                numXGrid--;
            }

            paddingLandscape.left = paddingLandscape.right = (int)(0.5f * (contentsNode.rect.width - numXGrid * gridSize.x - (numXGrid - 1) * spaceLandscape));
        }

        void OnEnable()
        {
            //ReLayout();

            AUIManager.OnScreenSizeChanged += OnScreenSizeChanged;

            StartCoroutine(ResetPosition(true));
        }

        void OnDisable()
        {
            AUIManager.OnScreenSizeChanged -= OnScreenSizeChanged;
        }

        void OnScreenSizeChanged()
        {
            CalcPadding();

            StartCoroutine(ResetPosition(false));
        }

        public void ReLayout()
        {
            if (this.gameObject.activeInHierarchy)
            {
                StartCoroutine(ResetPosition(false));
            }
        }

        public void ReLayoutDelayFrame()
        {
            if (this.gameObject.activeInHierarchy)
            {
                StartCoroutine(ResetPosition(true));
            }
        }

        IEnumerator ResetPosition(bool delayFrame)
        {
            if (delayFrame)
            {
                yield return 1;
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }

            foreach (RectTransform rectTransrom in lastObjects)
            {
                rectTransrom.SetAsLastSibling();
            }

            if (grid)
            {
                if (Screen.width > Screen.height)
                {
                    padding = paddingLandscape;
                }
                else
                {
                    padding = paddingPortrait;
                }
            }

            if (vertical)
            {
                pivotPos.y = -padding.top;

                contentsNode.sizeDelta = new Vector2(contentsNode.sizeDelta.x, -pivotPos.y);

                for(int i = 0; i < contentsNode.childCount; i++)
                {
                    RectTransform item = contentsNode.GetChild(i).GetComponent<RectTransform>();

                    AUIScrollViewItemLayout layout = item.gameObject.GetComponent<AUIScrollViewItemLayout>();

                    if (layout != null && layout.ignore)
                    {
                        continue;
                    }

                    float height = item.sizeDelta.y;

                    item.anchoredPosition = new Vector2(item.anchoredPosition.x, pivotPos.y);

                    contentsNode.sizeDelta = new Vector2(contentsNode.sizeDelta.x, -pivotPos.y + height + padding.bottom);

                    pivotPos.y = pivotPos.y - height - space;
                }
            }

            if (horizontal)
            {
                pivotPos.x = padding.left;

                contentsNode.sizeDelta = new Vector2(pivotPos.x, contentsNode.sizeDelta.y);

                for (int i = 0; i < contentsNode.childCount; i++)
                {
                    RectTransform item = contentsNode.GetChild(i).GetComponent<RectTransform>();

                    AUIScrollViewItemLayout layout = item.gameObject.GetComponent<AUIScrollViewItemLayout>();

                    if (layout != null && layout.ignore)
                    {
                        continue;
                    }

                    float width = item.sizeDelta.x;

                    item.anchoredPosition = new Vector2(pivotPos.x, item.anchoredPosition.y);

                    contentsNode.sizeDelta = new Vector2(pivotPos.x + width + padding.right, contentsNode.sizeDelta.y);

                    pivotPos.x = pivotPos.x + width + space;
                }
            }

            if (grid)
            {
                pivotPos = new Vector2(padding.left, -padding.top);

                contentsNode.sizeDelta = new Vector2(contentsNode.sizeDelta.x, -pivotPos.y);

                for (int i = 0; i < contentsNode.childCount; i++)
                {
                    if (pivotPos.x + gridSize.x + space > contentsNode.rect.width)
                    {
                        pivotPos.x = padding.left;

                        pivotPos.y = pivotPos.y - gridSize.y - space;

                        contentsNode.sizeDelta = new Vector2(contentsNode.sizeDelta.x, -pivotPos.y + gridSize.y + padding.bottom);
                    }

                    RectTransform item = contentsNode.GetChild(i).GetComponent<RectTransform>();

                    AUIScrollViewItemLayout layout = item.gameObject.GetComponent<AUIScrollViewItemLayout>();

                    if (layout != null && layout.ignore)
                    {
                        continue;
                    }

                    item.anchoredPosition = pivotPos;

                    item.sizeDelta = gridSize;

                    pivotPos.x = pivotPos.x + gridSize.x + space;
                }
            }
        }

        public void AddItem(RectTransform item)
        {
            item.SetParent(contentsNode, false);

            if (this.gameObject.activeInHierarchy)
            {
                StartCoroutine(SetPosition(item));
            }
        }

        public void AddItem(RectTransform item, int index)
        {
            item.SetParent(contentsNode, false);

            item.SetSiblingIndex(index);

            if (this.gameObject.activeInHierarchy)
            {
                StartCoroutine(SetPosition(item));
            }
        }

        public void RemoveItem(RectTransform item)
        {            
            if (vertical)
            {
                float height = item.sizeDelta.y;

                contentsNode.sizeDelta = new Vector2(contentsNode.sizeDelta.x, contentsNode.sizeDelta.y - height - space);

                pivotPos.y += height + space;
            }

            if (horizontal)
            {
                float width = item.sizeDelta.x;

                contentsNode.sizeDelta = new Vector2(contentsNode.sizeDelta.x - width - space, contentsNode.sizeDelta.y);

                pivotPos.x -= width + space;
            }

            contents.Remove(item);
        }

        public void RemoveItem(RectTransform item, Vector2 size)
        {
            if (vertical)
            {
                contentsNode.sizeDelta = new Vector2(contentsNode.sizeDelta.x, contentsNode.sizeDelta.y - size.y - space);

                pivotPos.y += size.y + space;
            }

            if (horizontal)
            {
                contentsNode.sizeDelta = new Vector2(contentsNode.sizeDelta.x - size.x - space, contentsNode.sizeDelta.y);

                pivotPos.x -= size.x + space;
            }

            contents.Remove(item);
        }

        public void Clear()
        {
            contents.Clear();

            Start();
        }

        IEnumerator SetPosition(RectTransform item)
        {
            yield return new WaitForEndOfFrame();

            if (item == null)
            {
                yield break;
            }

            if (vertical)
            {
                float height = item.sizeDelta.y;

                item.anchoredPosition = new Vector2(item.anchoredPosition.x, pivotPos.y);

                contentsNode.sizeDelta = new Vector2(contentsNode.sizeDelta.x, -pivotPos.y + height + padding.bottom);

                pivotPos.y = pivotPos.y - height - space;
            }

            if (horizontal)
            {
                float width = item.sizeDelta.x;

                item.anchoredPosition = new Vector2(pivotPos.x, item.anchoredPosition.y);

                contentsNode.sizeDelta = new Vector2(pivotPos.x + width + padding.right, contentsNode.sizeDelta.y);

                pivotPos.x = pivotPos.x + width + space;
            }

            if (grid)
            {
                if (pivotPos.x + gridSize.x + space > contentsNode.rect.width)
                {
                    pivotPos.x = padding.left;

                    pivotPos.y = pivotPos.y - gridSize.y - space;

                    contentsNode.sizeDelta = new Vector2(contentsNode.sizeDelta.x, -pivotPos.y + gridSize.y + padding.bottom);
                }

                item.anchoredPosition = pivotPos;

                item.sizeDelta = gridSize;

                pivotPos.x = pivotPos.x + gridSize.x + space;
            }

            contents.Add(item);
        }

        public void SetScorllPosition(Vector2 pos)
        {
            contentsNode.anchoredPosition = pos;
        }

        void Update()
        {
            if (vertical || grid)
            {
                foreach (RectTransform item in contents)
                {
                    AUIScrollViewItemLayout layout = item.gameObject.GetComponent<AUIScrollViewItemLayout>();

                    if (layout != null && layout.ignore)
                    {
                        continue;
                    }

                    if (item.sizeDelta.y < scrollView.rect.height)
                    {
                        item.gameObject.SetActive(
                               contentsNode.anchoredPosition.y + item.anchoredPosition.y - item.sizeDelta.y <= 0f
                            && contentsNode.anchoredPosition.y + item.anchoredPosition.y > -scrollView.rect.height
                        );
                    }
                    else
                    {
                        item.gameObject.SetActive(
                               ( contentsNode.anchoredPosition.y + item.anchoredPosition.y <= 0f &&
                                 contentsNode.anchoredPosition.y + item.anchoredPosition.y > -scrollView.rect.height)
                            || (contentsNode.anchoredPosition.y + item.anchoredPosition.y - item.sizeDelta.y > -scrollView.rect.height &&
                                 contentsNode.anchoredPosition.y + item.anchoredPosition.y - item.sizeDelta.y > -scrollView.rect.height)
                            || ( contentsNode.anchoredPosition.y + item.anchoredPosition.y >= 0f &&
                                 contentsNode.anchoredPosition.y + item.anchoredPosition.y - item.sizeDelta.y < -scrollView.rect.height)
                        );

                    }
                }
            }

            if (horizontal)
            {
                foreach (RectTransform item in contents)
                {
                    item.gameObject.SetActive(
                            contentsNode.anchoredPosition.x + item.anchoredPosition.x + item.sizeDelta.x >= 0f
                        && contentsNode.anchoredPosition.x + item.anchoredPosition.x < scrollView.rect.width
                    );                 
                }
            }
        }
    }
}