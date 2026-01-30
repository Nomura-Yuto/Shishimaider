using UnityEngine;

namespace MS.Shaders
{
	/// <summary>
	/// 指定したPositionをマテリアルのVectorパラメーターにセットするクラス
	/// </summary>
	[ExecuteAlways]
	public class MaterialSetPosition : MonoBehaviour
	{
		[SerializeField]
		Transform m_TargetTransfome;

		[SerializeField]
		Material m_Material;

		[SerializeField]
		string m_PropertyName = "";

		Vector3 m_OldPos;

		void OnEnable()
		{
			if (m_TargetTransfome != null)
			{
				m_OldPos = m_TargetTransfome.position;
			}
		}

		void Update()
		{
			if (m_TargetTransfome == null || m_Material == null)
			{
				return;
			}

			if (!m_Material.HasProperty(m_PropertyName))
			{
				return;
			}

			if (m_TargetTransfome.position == m_OldPos)
			{
				return;
			}

			m_OldPos = m_TargetTransfome.position;

			m_Material.SetVector(
				m_PropertyName,
				m_TargetTransfome.position
			);
		}
	}
}
