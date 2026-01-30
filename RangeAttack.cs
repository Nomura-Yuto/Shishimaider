using UnityEngine;
using System.Collections;
using MS.Attributes;
using MS.Games.Sounds;

namespace MS.Games.Skills
{
	public class RangeAttack : PlayerBaseSkill
	{
		[Header("範囲攻撃設定")]
		[SerializeField]
		[Tooltip("攻撃範囲")]
		private float m_AttackRange = 10f;

		[SerializeField]
		[Tooltip("ダメージ")]
		private IntReference m_Damage;

		[SerializeField, TagSelector]
		[Tooltip("攻撃対象のタグ")]
		private string m_TargetTag;

		void Update()
		{
			
		}

		protected override void OnSkillEnd()
		{
			
		}

		protected override void OnSkillStart()
		{
			StartCoroutine(AttackCoroutine());
		}

		/// <summary>
		/// 攻撃処理
		/// </summary>
		private IEnumerator AttackCoroutine()
		{
			IEffectPlayer player = Owner.EffectHandler?.PlayEffect(m_EffectName);

			if(player != null)
			{
				yield return new WaitUntil(() => ((RangeAttackEffectElement)player).AttackTrigger);
			}

			SoundManager.Instance.SE.PlayAtPositionRpc("Bite", transform.position);
			
			// 範囲内のエネミーの取得
			Collider[] hitColliders = Physics.OverlapSphere(transform.position, m_AttackRange);

			foreach (var hitCollider in hitColliders)
			{
				if (hitCollider.gameObject.tag == m_TargetTag)
				{
					GameObject enemy = hitCollider.gameObject;

					if (hitCollider.transform.parent != null)
					{
						enemy = hitCollider.transform.parent.gameObject;
					}

					InflictDamage(enemy);
				}
			}

			if (player != null)
			{
				yield return new WaitUntil(() => player.IsComplate);
			}

			EndSkill();
		}

		/// <summary>
		/// 対象にダメージを与える
		/// </summary>
		/// <param name="target">ダメージを与える対象</param>
		public void InflictDamage(GameObject target)
		{
			if (target.TryGetComponent(out IDamagable damagable))
			{
				bool isDead = damagable.Damage(m_Damage.Value);

				Debug.Log(target.gameObject.name + "に攻撃");

				if (isDead && damagable is Enemy enemy)
				{
					Debug.Log(target.gameObject.name + "を倒した");
				}

				if (isDead && damagable is FryEnemy fryEnemy)
				{
					Debug.Log(target.gameObject.name + "を倒した");
				}
			}
		}


#if UNITY_EDITOR
		// 範囲の描画
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(transform.position, m_AttackRange);
		}
#endif

	}
}

