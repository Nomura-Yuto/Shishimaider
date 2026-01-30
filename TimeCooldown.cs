using UnityEngine;
using System.Collections;

namespace MS.Systems.CoolDown
{

	[System.Serializable]
	public class TimeCooldown : CooldownBase
	{
		[Tooltip("クールダウンの時間")]
		public float m_CDTime = 5f;

		public TimeCooldown(bool init = false)
			: base(init) { }

		public override void StartCooldown()
		{
			Debug.Log(m_CooldownName + "：クールタイム開始");

			m_CDCoroutine = m_Owner.StartCoroutine(CDCoroutine());
		}

		public override void RestartCooldown()
		{
			ResetCooldown();

			StartCooldown();
		}

		public override void ResetCooldown()
		{
			if (m_CDCoroutine != null)
			{
				m_Owner.StopCoroutine(m_CDCoroutine);
				m_CDCoroutine = null;
			}

			m_Complete = true;
		}

		/// <summary>
		/// 時間制クールダウン処理開始
		/// </summary>
		private IEnumerator CDCoroutine()
		{
			m_Complete = false;

			yield return new WaitForSeconds(m_CDTime);

			m_Complete = true;

			Debug.Log(m_CooldownName + "：クールタイム終了");
		}

	}

}