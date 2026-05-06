using System;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

// https://www.geogebra.org/graphing    查看 曲线效果
namespace ScrollViewUI
{

    public enum EaseType
    {
        Constant,
        BackIn,
        BackInOut,
        BackOut,
        BounceIn,
        BounceInOut,
        BounceOut,
        CircIn,
        CircInOut,
        CircOut,
        CubicIn,
        CubicInOut,
        CubicOut,
        ElasticIn,
        ElasticInOut,
        ElasticOut,
        ExpoIn,
        ExpoInOut,
        ExpoOut,
        Fade,
        Linear,
        QuadIn,
        QuadInOut,
        QuadOut,
        QuartIn,
        QuartInOut,
        QuartOut,
        QuintIn,
        QuintInOut,
        QuintOut,
        SineIn,
        SineInOut,
        SineOut,
        Smooth
    }


    /// <summary>
    /// （0-1）动画曲线提供四种类型：    In 慢到快     Out 快到慢      InOut  由慢到快再到慢    OutIn 由快到慢再到快
    /// </summary>
    public static class Easing
    {
        public static Func<float, float> constant = (k) => 0;
        public static Func<float, float> linear = (k) => k;
        //平滑效果  渐入渐出
        public static Func<float, float> smooth = (t) =>
        {
            if (t <= 0) return 0;
            if (t >= 1) return 1;
            return t * t * (3 - 2 * t);
        };
        //渐隐效果
        public static Func<float, float> fade = (t) =>
        {
            if (t <= 0) return 0;
            if (t >= 1) return 1;
            return t * t * t * (t * (t * 6 - 15) + 10);
        };
        // quad  平方
        public static Func<float, float> quadIn = (k) => k * k;
        public static Func<float, float> quadOut = (k) => k * (2 - k);
        public static Func<float, float> quadInOut = (k) => (k *= 2) < 1 ? 0.5f * k * k : -0.5f * (--k * (k - 2) - 1);

        // cubic  立方 
        public static Func<float, float> cubicIn = (k) => k * k * k;
        public static Func<float, float> cubicOut = (k) => (--k) * k * k + 1;
        public static Func<float, float> cubicInOut = (k) => (k *= 2) < 1 ? 0.5f * k * k * k : 0.5f * ((k -= 2) * k * k + 2);

        // quart   4次方
        public static Func<float, float> quartIn = (k) => k * k * k * k;
        public static Func<float, float> quartOut = (k) => 1 - (--k) * k * k * k;
        public static Func<float, float> quartInOut = (k) => (k *= 2) < 1 ? 0.5f * k * k * k * k : -0.5f * ((k -= 2) * k * k * k - 2);

        // quint   5次方
        public static Func<float, float> quintIn = (k) => k * k * k * k * k;
        public static Func<float, float> quintOut = (k) => (--k) * k * k * k * k + 1;
        public static Func<float, float> quintInOut = (k) => (k *= 2) < 1 ? 0.5f * k * k * k * k * k : 0.5f * ((k -= 2) * k * k * k * k + 2);

        // sine   正弦曲线 
        public static Func<float, float> sineIn = (k) => 1 - Mathf.Cos(k * Mathf.PI / 2);
        public static Func<float, float> sineOut = (k) => Mathf.Sin(k * Mathf.PI / 2);
        public static Func<float, float> sineInOut = (k) => 0.5f * (1 - Mathf.Cos(Mathf.PI * k));

        // expo   指数曲线
        public static Func<float, float> expoIn = (k) => k == 0 ? 0 : Mathf.Pow(1024, k - 1);
        public static Func<float, float> expoOut = (k) => k == 1 ? 1 : 1 - Mathf.Pow(2, -10 * k);
        public static Func<float, float> expoInOut = (k) =>
        {
            if (k == 0) return 0;
            if (k == 1) return 1;
            if ((k *= 2) < 1) return 0.5f * Mathf.Pow(1024, k - 1);
            return 0.5f * (-Mathf.Pow(2, -10 * (k - 1)) + 2);
        };

        // circ  循环公式 
        public static Func<float, float> circIn = (k) => 1 - Mathf.Sqrt(1 - k * k);
        public static Func<float, float> circOut = (k) => Mathf.Sqrt(1 - (--k) * k);
        public static Func<float, float> circInOut = (k) =>
        {
            if ((k *= 2) < 1) return -0.5f * (Mathf.Sqrt(1 - k * k) - 1);
            return 0.5f * (Mathf.Sqrt(1 - (k -= 2) * k) + 1);
        };

        // elastic  弹簧回震
        public static Func<float, float> elasticIn = (k) =>
        {

            if (k == 0) return 0;
            if (k == 1) return 1;
            float p = 0.4f;
            float a = 1;
            float s2;
            if (a < 1)
            {
                a = 1;
                s2 = p / 4;
            }
            else s2 = p * (float)Mathf.Asin(1 / a) / (2 * (float)Mathf.PI);
            return -(a * (float)Mathf.Pow(2, 10 * (k -= 1)) * (float)Mathf.Sin((k - s2) * (2 * Mathf.PI) / p));
        };
        public static Func<float, float> elasticOut = (k) =>
        {

            if (k == 0) return 0;
            if (k == 1) return 1;
            float p = 0.4f;
            float a = 1;
            float s2;
            if (a < 1)
            {
                a = 1;
                s2 = p / 4;
            }
            else s2 = p * (float)Mathf.Asin(1 / a) / (2 * (float)Mathf.PI);
            return (a * (float)Mathf.Pow(2, -10 * k) * (float)Mathf.Sin((k - s2) * (2 * Mathf.PI) / p) + 1);
        };
        public static Func<float, float> elasticInOut = (k) =>
        {

            if (k == 0) return 0;
            if ((k *= 2) == 2) return 1;
            float p = 0.3f * 1.5f;
            float a = 1;
            float s2;
            if (a < 1)
            {
                a = 1;
                s2 = p / 4;
            }
            else s2 = p * (float)Mathf.Asin(1 / a) / (2 * (float)Mathf.PI);
            if (k < 1) return -0.5f * (a * (float)Mathf.Pow(2, 10 * (k -= 1)) * (float)Mathf.Sin((k - s2) * (2 * Mathf.PI) / p));
            return a * (float)Mathf.Pow(2, -10 * (k -= 1)) * (float)Mathf.Sin((k - s2) * (2 * Mathf.PI) / p) * 0.5f + 1;
        };

        // back  回退效果
        public static Func<float, float> backIn = (k) => k * k * ((1.70158f + 1) * k - 1.70158f);
        public static Func<float, float> backOut = (k) => --k * k * ((1.70158f + 1) * k + 1.70158f) + 1;
        public static Func<float, float> backInOut = (k) =>
        {
            var s = 1.70158f * 1.525f;
            if ((k *= 2) < 1) return 0.5f * (k * k * ((s + 1) * k - s));
            return 0.5f * ((k -= 2) * k * ((s + 1) * k + s) + 2);
        };

        // bounce  弹跳效果
        public static Func<float, float> bounceIn = (k) => 1 - bounceOut(1 - k);
        public static Func<float, float> bounceOut = (k) =>
        {
            if (k < (1 / 2.75f))
            {
                return 7.5625f * k * k;
            }
            else if (k < (2 / 2.75f))
            {
                return 7.5625f * (k -= (1.5f / 2.75f)) * k + 0.75f;
            }
            else if (k < (2.5 / 2.75))
            {
                return 7.5625f * (k -= (2.25f / 2.75f)) * k + 0.9375f;
            }
            else
            {
                return 7.5625f * (k -= (2.625f / 2.75f)) * k + 0.984375f;
            }
        };
        public static Func<float, float> bounceInOut = (k) => k < 0.5f ? 0.5f * bounceIn(k * 2) : 0.5f * bounceOut(k * 2 - 1) + 0.5f;



        // OutIn 方法 
        public static Func<Func<float, float>, Func<float, float>, Func<float, float>> _makeOutIn = (fnIn, fnOut) =>
        {
            return (k) =>
            {
                if (k < 0.5) return fnOut(k * 2) / 2;
                return fnIn(2 * k - 1) / 2 + 0.5f;
            };
        };

        public static Func<float, float> quadOutIn = _makeOutIn(quadIn, quadOut);
        public static Func<float, float> cubicOutIn = _makeOutIn(cubicIn, cubicOut);
        public static Func<float, float> quartOutIn = _makeOutIn(quartIn, quartOut);
        public static Func<float, float> quintOutIn = _makeOutIn(quintIn, quintOut);
        public static Func<float, float> sineOutIn = _makeOutIn(sineIn, sineOut);
        public static Func<float, float> expoOutIn = _makeOutIn(expoIn, expoOut);
        public static Func<float, float> circOutIn = _makeOutIn(circIn, circOut);
        public static Func<float, float> backOutIn = _makeOutIn(backIn, backOut);
        public static Func<float, float> bounceOutIn = _makeOutIn(bounceIn, bounceOut);




   

    }
}