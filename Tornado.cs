using UnityEngine;
using System.Collections;
using MS.Attributes;
using MS.Games.Sounds;
using Unity.Netcode;

namespace MS.Games.Skills
{
	public class Tornado : PlayerBaseSkill
	{
		[System.Serializable]
		[Tooltip("エネミ－の種類に対するノックバック力")]
		struct KnockbackForce
		{
			public float fryEnemy;
			public float normalEnemy;
		}

		[Header("トルネード設定")]
		[SerializeField]
		[Tooltip("攻撃範囲：半径")]
		private float m_Range = 5f;

		[SerializeField]
		[Tooltip("攻撃範囲：高さ")]
		private float m_Height = 2f;

		[SerializeField]
		[Tooltip("ノックバックする力")]
		KnockbackForce m_KnockbackForce;

		[SerializeField]
		[Tooltip("ノックバック持続時間")]
		private float m_KnockbackDuration = 0.5f;

		[SerializeField, TagSelector]
		[Tooltip("吹き飛ばす対象タグ")]
		private string m_TargetTag;

		[SerializeField]
		[Tooltip("当たり判定用コライダー")]
		private CapsuleCollider m_Collider;


		// Start is called once before the first execution of Update after the MonoBehaviour is created
		void Start()
		{
			m_CoolDownClass.m_Owner = this;

			m_Collider.enabled = false;
			m_Collider.radius = m_Range;
			m_Collider.center = new Vector3(0f, m_Height * 0.5f, 0f);
			m_Collider.height = m_Range * 2f + m_Height;
		}

		protected override void OnSkillEnd()
		{

		}

		protected override void OnSkillStart()
		{
			StartCoroutine(CollisionEnabled());
		}

		private void OnTriggerEnter(Collider other)
		{
			if(other.gameObject.tag == m_TargetTag)
			{
				GameObject enemy = other.gameObject;

				if (other.transform.parent != null)
				{
					enemy = other.transform.parent.gameObject;
				}

				Knockback(enemy);
			}
		}

		/// <summary>
		/// コリジョンの有効化コルーチン
		/// </summary>
		public IEnumerator CollisionEnabled()
		{
			var effect = Owner.EffectHandler.PlayEffect(m_EffectName);
			SoundManager.Instance.SE.PlayAtPositionRpc("Tornado", transform.position);

			m_Collider.enabled = true;

			while (!effect.IsComplate)
			{
				transform.position = new Vector3(Owner.transform.position.x, 0f, Owner.transform.position.z);
				yield return null;
			}

			m_Collider.enabled = false;

			EndSkill();
		}

		/// <summary>
		/// ノックバック処理
		/// </summary>
		/// <param name="enemy">ノックバックさせる対象</param>
		private void Knockback(GameObject enemy)
		{
			if (!enemy.TryGetComponent(out NetworkObject netObj))
			{
				return;
			}

			Vector3 direction = enemy.transform.position - transform.position;
			direction.y = 0f;
			direction.Normalize();

			Vector3 force = direction;

			float stunTime = m_KnockbackDuration;

			if (enemy.TryGetComponent(out FryEnemy fryEnemy))
			{
				force *= m_KnockbackForce.fryEnemy;
				fryEnemy.KnockbackRpc(force, stunTime);
			}
			else if (enemy.TryGetComponent(out Enemy normalEnemy))
			{
				force *= m_KnockbackForce.normalEnemy;
				normalEnemy.KnockbackRpc(force, stunTime);
			}
		}


#if UNITY_EDITOR
		// トルネード範囲の描画
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.cyan;
			DrawCylinder(transform.position, m_Range, m_Height);
		}

		// シリンダーの描画
		void DrawCylinder(Vector3 pos, float radius, float height)
		{
			// 側面縦線の数
			int sideLineCount = 8;

			Vector3 bottom = new Vector3(pos.x, 0f, pos.z);
			Vector3 top = bottom + Vector3.up * height;

			DrawCircle(top, radius);
			DrawCircle(bottom, radius);

			for (int i = 0; i < sideLineCount; i++)
			{
				float angle = i * Mathf.PI * 2f / sideLineCount;

				Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

				Gizmos.DrawLine(top + dir * radius, bottom + dir * radius);
				Gizmos.DrawLine(top, top + dir * radius);
				Gizmos.DrawLine(bottom, bottom + dir * radius);
			}
		}

		// 円の描画
		void DrawCircle(Vector3 center, float radius)
		{
			int segments = 32;
			float angle = 0f;

			Vector3 start = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

			for (int i = 1; i <= segments; i++)
			{
				angle = i * Mathf.PI * 2f / segments;
				Vector3 next = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
				Gizmos.DrawLine(start, next);
				start = next;
			}
		}
#endif
	}
}