using UnityEngine;
using Unity.Netcode;

namespace MS.Games.Sounds
{
	public class SoundManager : NetworkBehaviour
	{
		public static SoundManager Instance
		{
			get;
			private set;
		}

		[SerializeField]
		BGMManager m_BGMManager;

		[SerializeField]
		SEManager m_SEManager;

		public BGMManager BGM => m_BGMManager;
		public SEManager SE => m_SEManager;

		private void Awake()
		{
			if (Instance == null)
            {
				Instance = this;
                
                // ルートオブジェクトの場合のみDontDestroyOnLoad
                if (transform.parent == null)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
		}
    }
}