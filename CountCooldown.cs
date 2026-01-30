using UnityEngine;

namespace MS.Systems.CoolDown
{
	[System.Serializable]
	public class CountCooldown : CooldownBase
	{
		[Tooltip("クールダウンに必要なカウント")]
		public int m_RequiredCount = 5;

		[Tooltip("現在のカウント")]
		public int m_CDCount = 0;

		public override void StartCooldown()
		{
			Debug.Log("クールダウン開始");
			m_CDCount = 0;
			m_Complete = false;
		}

		public override void RestartCooldown()
		{
			ResetCooldown();

			StartCooldown();
		}

		public override void ResetCooldown()
		{
			m_CDCount = m_RequiredCount;

			m_Complete = true;
		}

		public float AddCount(int value = 1)
		{
			if (m_Complete)
			{
				return GetCountRatio();
			}

			m_CDCount += value;

			if (m_CDCount >= m_RequiredCount)
			{
				m_Complete = true;
				m_CDCount = m_RequiredCount;
				Debug.Log("クールダウン完了");
			}

			return GetCountRatio();
		}

		public float GetCountRatio()
		{
			return (float)m_CDCount / m_RequiredCount;
		}

	}

}