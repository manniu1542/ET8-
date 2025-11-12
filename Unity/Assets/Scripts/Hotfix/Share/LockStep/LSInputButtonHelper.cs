namespace ET
{
    /// <summary>
    /// 对枚举值的解析
    /// </summary>
    public static class LSInputButtonHelper
    {
        /// <summary>
        /// 检查是否包含指定标志
        /// </summary>
        public static bool HasButton(this LSInputButton current, LSInputButton button)
        {
            return (current & button) != 0;
        }

        /// <summary>
        /// 添加一个标志（返回新的结果）
        /// </summary>
        public static LSInputButton AddButton(this LSInputButton current, LSInputButton button)
        {
            return current | button;
        }

        /// <summary>
        /// 移除一个标志（返回新的结果）
        /// </summary>
        public static LSInputButton RemoveButton(this LSInputButton current, LSInputButton button)
        {
            return current & ~button;
        }

        /// <summary>
        /// 清空所有标志（相当于重置）
        /// </summary>
        public static void Reset(this LSInputButton current)
        {
            current = LSInputButton.None;
        }
    }
}