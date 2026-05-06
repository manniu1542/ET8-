
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1.插值 某个数值 。   2.生成动画。  （开始 动画的回调 ,完成动画的回调）     3.关闭动画 
namespace ScrollViewUI
{

    public class DGTween : IDisposable
    {
        public Action actStartFun = null;
        public Action actFinishFun = null;
        public EaseType type = EaseType.Linear;
        /// <summary>
        /// 正在 运动 调用的 协程
        /// </summary>
        public IEnumerator ienumLerp;
        public bool isKill;
        public void Dispose()
        {
            actStartFun = null;
            actFinishFun = null;
            ienumLerp = null;
        }
    }
    public static class DGTweenSystem
    {
        public static DGTween SetEase(this DGTween self, EaseType type)
        {
            self.type = type;
            return self;
        }
        public static DGTween OnStart(this DGTween self, Action actStartFun)
        {
            self.actStartFun = actStartFun;
            return self;
        }

        public static DGTween OnComplete(this DGTween self, Action actFinishFun)
        {
            self.actFinishFun = actFinishFun;
            return self;
        }


    }
    public class DOTweenAnimCurve : MonoBehaviour
    {
        private static object lockHelp = new object();
        private static DOTweenAnimCurve _instance;
        public static DOTweenAnimCurve Instance
        {
            get
            {
                lock (lockHelp)
                {
                    if (_instance == null)
                    {

                        GameObject go = new GameObject("DOTweenAnimCurve");
                        _instance = go.AddComponent<DOTweenAnimCurve>();
                        _instance.Init();
                        DontDestroyOnLoad(go);

                    }
                }
                return _instance;

            }
        }


        //0-1的变动

        public Dictionary<EaseType, Func<float, float>> animCurveDic = new Dictionary<EaseType, Func<float, float>>();
        public void Init()
        {
            animCurveDic.Add(EaseType.Constant, Easing.constant);
            animCurveDic.Add(EaseType.BackIn, Easing.backIn);
            animCurveDic.Add(EaseType.BackInOut, Easing.backInOut);
            animCurveDic.Add(EaseType.BackOut, Easing.backOut);
            animCurveDic.Add(EaseType.BounceIn, Easing.bounceIn);
            animCurveDic.Add(EaseType.BounceInOut, Easing.bounceInOut);
            animCurveDic.Add(EaseType.BounceOut, Easing.bounceOut);
            animCurveDic.Add(EaseType.CircIn, Easing.circIn);
            animCurveDic.Add(EaseType.CircInOut, Easing.circInOut);
            animCurveDic.Add(EaseType.CircOut, Easing.circOut);
            animCurveDic.Add(EaseType.CubicIn, Easing.cubicIn);
            animCurveDic.Add(EaseType.CubicInOut, Easing.cubicInOut);
            animCurveDic.Add(EaseType.CubicOut, Easing.cubicOut);
            animCurveDic.Add(EaseType.ElasticIn, Easing.elasticIn);
            animCurveDic.Add(EaseType.ElasticInOut, Easing.elasticInOut);
            animCurveDic.Add(EaseType.ElasticOut, Easing.elasticOut);
            animCurveDic.Add(EaseType.ExpoIn, Easing.expoIn);
            animCurveDic.Add(EaseType.ExpoInOut, Easing.expoInOut);
            animCurveDic.Add(EaseType.ExpoOut, Easing.expoOut);
            animCurveDic.Add(EaseType.Fade, Easing.fade);
            animCurveDic.Add(EaseType.Linear, Easing.linear);
            animCurveDic.Add(EaseType.QuadIn, Easing.quadIn);
            animCurveDic.Add(EaseType.QuadInOut, Easing.quadInOut);
            animCurveDic.Add(EaseType.QuadOut, Easing.quadOut);
            animCurveDic.Add(EaseType.QuartIn, Easing.quartIn);
            animCurveDic.Add(EaseType.QuartInOut, Easing.quartInOut);
            animCurveDic.Add(EaseType.QuartOut, Easing.quartOut);
            animCurveDic.Add(EaseType.QuintIn, Easing.quintIn);
            animCurveDic.Add(EaseType.QuintInOut, Easing.quintInOut);
            animCurveDic.Add(EaseType.QuintOut, Easing.quintOut);
            animCurveDic.Add(EaseType.SineIn, Easing.sineIn);
            animCurveDic.Add(EaseType.SineInOut, Easing.sineInOut);
            animCurveDic.Add(EaseType.SineOut, Easing.sineOut);
            animCurveDic.Add(EaseType.Smooth, Easing.smooth);


        }


        #region 移动 动画

        public DGTween DOAnchorPosY(RectTransform rtf, float endV, float time, Action<float> actUpdateFun = null)
        {
            //TODO: DGTween 对象池子生成。 回收要重置
            return To(rtf.anchoredPosition.y, endV, time, progress =>
            {

                rtf.anchoredPosition = new Vector2(rtf.anchoredPosition.x, progress);
                actUpdateFun?.Invoke(progress);
            });
        }
        public DGTween DOAnchorPosX(RectTransform rtf, float endV, float time, Action<float> actUpdateFun = null)
        {
            //TODO: DGTween 对象池子生成。 回收要重置
            return To(rtf.anchoredPosition.x, endV, time, progress =>
            {

                rtf.anchoredPosition = new Vector2(progress, rtf.anchoredPosition.y);
                actUpdateFun?.Invoke(progress);
            });
        }
        #endregion


        public DGTween To(float startV, float endV, float time, Action<float> actUpdateFun)
        {
            //TODO: DGTween 对象池子生成。 回收要重置
            DGTween animCurveClass = new DGTween();
            animCurveClass.ienumLerp = StartLerp(animCurveClass, startV, endV, time, actUpdateFun);
            StartCoroutine(animCurveClass.ienumLerp);

            return animCurveClass;
        }
        public DGTween ToVe3(Vector3 startV, Vector3 endV, float time, Action<Vector3> actUpdateFun)
        {
            //TODO: DGTween 对象池子生成。 回收要重置
            DGTween animCurveClass = new DGTween();
            animCurveClass.ienumLerp = StartVe3Lerp(animCurveClass, startV, endV, time, actUpdateFun);
            StartCoroutine(animCurveClass.ienumLerp);

            return animCurveClass;
        }

        public void KillAnimCurve(DGTween animCurve)
        {
            if(animCurve==null)return;
            if (!animCurve.isKill && animCurve.ienumLerp != null)
            {
                StopCoroutine(animCurve.ienumLerp);
            }
            animCurve.ienumLerp = null;
            animCurve.Dispose();
        }

        IEnumerator StartLerp(DGTween ani, float startV, float endV, float time, Action<float> actUpdateFun)
        {
            float curTime = 0;
            yield return null; // ani.type 的赋值要在 yield 之后，给他们赋值再 链式写法SetEase才能生效
            Func<float, float> aniCur = animCurveDic[ani.type];
            ani.actStartFun?.Invoke();

            while (curTime < time)
            {
                curTime += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(curTime / time);
                float progress = aniCur(k);

                float curV = Mathf.LerpUnclamped(startV, endV, progress);
                actUpdateFun?.Invoke(curV);

                yield return null;

            }

            actUpdateFun?.Invoke(endV);
            ani.actFinishFun?.Invoke();
            ani.isKill = true;
        }
        IEnumerator StartVe3Lerp(DGTween ani, Vector3 startV, Vector3 endV, float time, Action<Vector3> actUpdateFun)
        {
            float curTime = 0;
            Func<float, float> aniCur = animCurveDic[ani.type];
            yield return null;
            ani.actStartFun?.Invoke();

            while (curTime < time)
            {
                curTime += Time.unscaledDeltaTime;

                float progress = aniCur(curTime / time);

                var curV = Vector3.Lerp(startV, endV, progress);
                actUpdateFun?.Invoke(curV);
                yield return null;

            }

            actUpdateFun?.Invoke(endV);
            ani.actFinishFun?.Invoke();
            ani.isKill = true;
        }

    }

}
