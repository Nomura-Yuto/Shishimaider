using System;
using UnityEngine;

namespace MS.Games.Sounds
{
	[Serializable]
	public class SoundData
	{
		[SerializeField]
		[Tooltip("オーディオのキー")]
		private string m_Key;

		[SerializeField]
		[Tooltip("オーディオファイル")]
		private AudioClip m_Clip;

		[SerializeField, Range(0, 1)]
		[Tooltip("音量")]
		private float m_Volume = 1f;

		[SerializeField]
		[Tooltip("再生開始までの遅延時間")]
		private float m_Delay = 0f;
		
		[SerializeField]
		[Tooltip("ループ再生の有無")]
		private bool m_IsLoop = false;


		public string Key => m_Key;
		public AudioClip Clip => m_Clip;
		public float Volume => m_Volume;
		public float Delay => m_Delay;
		public bool IsLoop => m_IsLoop;
	}
}