using UnityEngine;
using MS.Systems.CoolDown;
using MS.SO.Notification;
using UnityEngine.InputSystem;
using TNRD;

namespace MS.Games.Skills
{
	public enum CDStartTiming
	{
		OnSkillStart,
		OnSkillEnd
	}

	public abstract class PlayerBaseSkill : MonoBehaviour, IInputActionHandler
	{
		[Tooltip("クールダウンの開始タイミング")]
		public CDStartTiming m_CDTiming = CDStartTiming.OnSkillStart;

		[Tooltip("クールダウンクラス")]
		public CountCooldown m_CoolDownClass = new CountCooldown();

        [SerializeField]
        private bool m_CanMove = true;
		[SerializeField]
		private bool m_CanRotate = true;

		[SerializeField]
		protected string m_EffectName;

        private bool m_IsFinished = true;

		public bool CanMove => m_CanMove;
		public bool CanRotate => m_CanRotate;

		public NotificationFloat SkillCountRate
		{
			get;
			set;
		}

		public Player Owner
		{
			get;
			set;
		}

        public void Action(InputAction.CallbackContext context)
		{
			if (!context.started)
			{
				return;
			}

			if (Owner.CurrentState is Player.PlayerMoveState || Owner.CurrentState is Player.PlayerIdleState)
			{
				ActivateSkill();
			}
		}

		/// <summary>
		/// スキルの有効化
		/// </summary>
		/// <returns>スキルが発動できたか</returns>
		public bool ActivateSkill()
		{
			if (!m_IsFinished)
			{
				Debug.Log("スキル発動中");
				return false;
			}

			if (m_CoolDownClass != null)
			{
				if (!m_CoolDownClass.IsComplete())
				{
					Debug.Log("クールダウン中");
					return false;
				}

				if (m_CDTiming == CDStartTiming.OnSkillStart)
				{
					m_CoolDownClass.StartCooldown();
				}
			}

			m_IsFinished = false;

			if (Owner != null)
			{
				Owner.ToSkillState();
			}
			OnSkillStart();
			SkillCountRate.Value = 0;

			return true;
		}


		/// <summary>
		/// スキル終了時の処理
		/// </summary>
		public void EndSkill()
		{
			if (m_CoolDownClass != null)
			{
				if (m_CDTiming == CDStartTiming.OnSkillEnd)
				{
					m_CoolDownClass.StartCooldown();
				}
			}

			m_IsFinished = true;

			if (Owner != null && Owner.CurrentState is not Player.PlayerDeadState)
			{
				Owner.ToMoveState();
			}
			OnSkillEnd();
		}

		/// <summary>
		/// スキル発動時に実行
		/// </summary>
		protected abstract void OnSkillStart();

		/// <summary>
		/// スキル終了時に実行
		/// </summary>
		protected abstract void OnSkillEnd();

		public void AddCount(int value = 1)
		{
			float ratio = m_CoolDownClass.AddCount(value);
            SkillCountRate.Value = ratio;

			Debug.Log($"スキルカウント:{value}");
			Debug.Log($"スキルカウントレート:{ratio}");
		}
	}

}
