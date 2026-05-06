using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ET.ScrollCustomRange;

namespace ET
{
    [ChildOf(typeof(ScrollCustomRange))]
    public class ScrollCustomRangeItemComponent : Entity, IAwake<Transform>, IDestroy
    {
        public Transform transform;
        public GameObject go;

        public RectTransform rtf;
        public ContentSizeFitter contenSizeFit;
        public LayoutGroup layoutGroup;
        public bool IsUsing;

        public ItemData data;
        public ScrollCustomRange scrParent;

        //需要强制刷新的ui
        public List<TextMeshProUGUI> arrTxtNeedUpdateMesh;
    }
}