using MS.Games.Sounds;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace MS.Games.Skills
{
	public class Sonar : PlayerBaseSkill
	{
		[Header("ソナー設定")]
		[SerializeField]
		[Tooltip("探知範囲")]
		private float m_DetectionRadius = 10f;

		[SerializeField]
		[Tooltip("追跡継続時間")]
		private float m_TrackingTime = 5f;

		[SerializeField]
		[Tooltip("スタンの継続時間")]
		private float m_StunDuration = 1.5f;

		private List<Coroutine> m_Coroutines;

		private void Awake()
		{
			m_Coroutines = new List<Coroutine>();
		}

		private void OnDestroy()
		{
			if(m_Coroutines == null)
			{
				return;
			}

			foreach(var coroutine in m_Coroutines)
			{
				if(coroutine != null)
				{
					StopCoroutine(coroutine);
				}
			}
		}

		protected override void OnSkillEnd()
		{
		}

		protected override void OnSkillStart()
		{
			StartCoroutine(SonarEffect());

			// 範囲内のエネミーの取得
			Collider[] hitColliders = Physics.OverlapSphere(transform.position, m_DetectionRadius);

			foreach (var hitCollider in hitColliders)
			{
				SonarRpc(hitCollider);
			}
		}

		/// <summary>
		/// アウトラインの表示コルーチン
		/// </summary>
		/// <param name="outline">表示するアウトラインスクリプト</param>
		public IEnumerator OutlineCoroutine(Outline outline)
		{
			if(outline == null)
			{
				yield break;
			}

			outline.enabled = true;

			yield return new WaitForSeconds(m_TrackingTime);

			if (outline != null)
			{
				outline.enabled = false;
			}
		}


		public IEnumerator SonarEffect()
		{
			var effectPlayer = Owner.EffectHandler.PlayEffect(m_EffectName);
			SoundManager.Instance.SE.PlayAtPositionRpc("Sonar", transform.position);

			yield return new WaitUntil(() => effectPlayer.IsComplate);
			Debug.Log("ソナーエフェクト終了");
			EndSkill();
			yield return null;
		}


		[Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
		public void SonarRpc(Collider col)
		{
			if (col.TryGetComponent(out Outline outline))
			{
				var c = StartCoroutine(OutlineCoroutine(outline));
				m_Coroutines.Add(c);
			}

		}

#if UNITY_EDITOR

		// ソナー範囲の描画
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(transform.position, m_DetectionRadius);
		}
#endif

	}
}

