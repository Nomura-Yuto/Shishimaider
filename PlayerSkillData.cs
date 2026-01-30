using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;
using MS.Attributes;
using MS.Games.Skills;

namespace MS.SO.PlayerSkill
{
	/// <summary>
	/// スキルデータ制御
	/// </summary>
	[CreateAssetMenu(fileName = "PlayerSkillData", menuName = "Scriptable Objects/Games/PlayerSkillData")]
	public class PlayerSkillData : ScriptableObject
	{
		[SerializeField]
		[Tooltip("スキル情報一覧")]
		private List<PlayerSkillAsset> m_SkillAssets;
		[SerializeField, EditAvailability(EditAvailabilityMode.AlwaysDisabled)]
		private PlayerSkillAsset m_SelectedSkillAsset = null;

		public PlayerSkillAsset SelectedSkill
		{
			get
			{
				if(m_SelectedSkillAsset != null)
				{
                    Debug.Log($"選択されたスキル: {m_SelectedSkillAsset.SkillName}");
                    return m_SelectedSkillAsset;
				}

				Debug.Log("選択されたスキルがありません");

				// スキルが選択されなかった場合は、リスト内のランダムなスキルを渡す
				if(m_SkillAssets.Count > 0)
				{
                    int random = Random.Range(0, m_SkillAssets.Count - 1);
                    return m_SkillAssets[random];
                }
				else
				{
					return null;
				}
			}
			set => m_SelectedSkillAsset = value;	
		}

		/// <summary>
		/// スキル情報一覧の取得
		/// </summary>
		public List<PlayerSkillAsset> GetSkillList()
		{
			return m_SkillAssets;
		}

		/// <summary>
		/// スキルの検索 (スキル名での検索)
		/// </summary>
		/// <returns>存在する場合そのスキルアセット / 無い場合はnull</returns>
		public PlayerSkillAsset FindSkill(string skillName)
		{
			return m_SkillAssets.Find(skill => skill.SkillName == skillName);
		}

        private void OnEnable()
        {
            m_SelectedSkillAsset = null;
        }

        private void OnDisable()
        {
            m_SelectedSkillAsset = null;
        }
    }

	[System.Serializable]
	public class PlayerSkillAsset
	{
		[SerializeField]
		[Tooltip("スキルのオブジェクト")]
		private PlayerBaseSkill m_SkillPrefab;

        [SerializeField]
        [Tooltip("スキル名")]
        private string m_SkillName;

        [SerializeField]
        [Tooltip("スキルアイコン")]
		[PreviewSprite(50)]
        private Sprite m_Icon;

        [SerializeField]
        [Tooltip("スキル動画")]
        private VideoClip m_Video;

        [SerializeField]
        [Tooltip("スキル説明")]
		[Multiline]
        private  string m_Explanation;

		public PlayerBaseSkill SkillPrefab => m_SkillPrefab;
		public string SkillName => m_SkillName;
		public Sprite Icon => m_Icon;
		public VideoClip Video => m_Video;
		public string Explanation => m_Explanation;
	}
}


