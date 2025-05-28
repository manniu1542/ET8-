using System;
using System.Text;

namespace ET
{
    public static class StringHashHelper
    {
        // bkdr hash
        /// <summary>
        /// 字符串哈希算法
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long GetLongHashCode(this string str)
        {
            //质数，避免计算结果导致哈希冲突（不一样的两个str得出同样的 数字，冲突）
            const uint seed = 1313; // 31 131 1313 13131 131313 etc..
            
            ulong hash = 0;
            for (int i = 0; i < str.Length; ++i)
            {
                char c = str[i];
                byte high = (byte)(c >> 8);
                byte low = (byte)(c & byte.MaxValue);
                hash = hash * seed + high;
                hash = hash * seed + low;
            }
            return (long)hash;
        }
        /// <summary>
        /// 根据字符串的哈希值进行mode求余。伪随机
        /// </summary>
        /// <param name="strText"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static int Mode(this string strText, int mode)
        {
            if (mode <= 0)
            {
                throw new Exception($"string mode < 0: {strText} {mode}");
            }
            return (int)((ulong)strText.GetLongHashCode() % (uint)mode);
        }
    }
}