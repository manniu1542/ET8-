using UnityEngine;

namespace ET
{
    public class RoundScale123_ScrollUITweenAnim : IScrollUITweenAnim
    {
        public void OnUpdateItemAnim(RectTransform rtfItem, ScrollCycleView scroll)
        {
            var rtfScrollView = scroll.rtfScrollView;

            // 获取父物体中心点坐标（局部坐标）
            float gridCenter = rtfScrollView.sizeDelta.x / 2;

            // 获取子物体中心点坐标（局部坐标）
            float itemCenter = scroll.rtfGrid.anchoredPosition.x + rtfItem.anchoredPosition.x + scroll.cellSize.x / 2;

            // 调整 itemCenter 以匹配 ScrollCycleView 的布局方式
            // 定义 "近" 的阈值（cellSize 的 1/4）
            float nearThreshold = scroll.cellSize.x / 5;
            // 计算 maxDistance（从中心到角落的距离）
            float maxDistance = rtfScrollView.rect.size.x / 2f;
            // 计算两者之间的距离
            float distance = Mathf.Abs(gridCenter - itemCenter);

      
            // 计算缩放比例：
            // - 如果距离 ≤ nearThreshold，缩放 = 1.0
            // - 否则，在 nearThreshold 到 maxDistance 之间从 1.0 降到 0.8
            float scale;
            if (distance <= nearThreshold)
            {
                scale = 1f;
                rtfItem.Find("root/Image/red").gameObject.SetActive(true);
                rtfItem.Find("root/Image/blue").gameObject.SetActive(false);
            }
            else
            {
                rtfItem.Find("root/Image/red").gameObject.SetActive(false);
                rtfItem.Find("root/Image/blue").gameObject.SetActive(true);
                // 计算距离在 nearThreshold ~ maxDistance 之间的比例
                float t = Mathf.Clamp01((distance - nearThreshold) / (maxDistance - nearThreshold));

             
                // 从 1.0 降到 0.8
                scale = Mathf.Lerp(1f, 0.8f, t*2);
            }

            // 应用缩放
            rtfItem.Find("root").localScale = new Vector3(scale, scale, 1f);
        }
    }
}