#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace ZHFSM
{
	public enum ParameterType
	{
		Int,
		Float,
		Bool,
		Trigger
	}
    /// <summary>
    /// 状态机过度判断的参数
    /// </summary>
	[System.Serializable]
	public class Parameter
	{
#if UNITY_5_3_OR_NEWER
		[SerializeField]
#endif
		protected string m_name;

		public string name
		{ get { return m_name; } set { m_name = value; } }

#if UNITY_5_3_OR_NEWER
		[SerializeField]
#endif
		protected ParameterType m_type;

		public ParameterType type => m_type;

		public float baseValue;

		public Parameter(string name, ParameterType type, float value)
		{
			m_name = name;
			m_type = type;
			baseValue = value;
		}
	}
}