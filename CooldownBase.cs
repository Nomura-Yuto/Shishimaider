using UnityEngine;

namespace MS.Systems.CoolDown
{
	public enum CooldownType
	{
		None,
		Time,
		Count
	}

	[System.Serializable]
	public abstract class CooldownBase
	{
		protected bool m_Complete = false;

		[Tooltip("コルーチン保存用")]
		protected Coroutine m_CDCoroutine;

		[Tooltip("クールダウンの所有者(このクラスを持つスクリプト)")]
		public MonoBehaviour m_Owner;

		public string m_CooldownName = "Base";

		public CooldownBase(bool init = false)
		{
			m_Complete = init;
		}

		/// <summary>
		/// クールダウン処理開始
		/// </summary>
		public abstract void StartCooldown();

		/// <summary>
		/// クールダウンのリスタート
		/// </summary>
		public abstract void RestartCooldown();

		/// <summary>
		/// クールダウンをリセット(クールダウン完了状態に)
		/// </summary>
		public abstract void ResetCooldown();

		public bool IsComplete()
		{
			return m_Complete;
		}

	}

}