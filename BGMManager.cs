using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;

namespace MS.Games.Sounds
{
	public class BGMManager : NetworkBehaviour
	{
		[Tooltip("再生をサーバーのみにするか")]
		[SerializeField]
		private bool m_ServerOnly;

		[Tooltip("再生をクライアントのみにするか")]
		[SerializeField]
		private bool m_ClientOnly;

		public bool ServerOnly
		{
			get => m_ServerOnly;
			set => m_ServerOnly = value;
		}
		public bool ClientOnly
		{
			get => m_ClientOnly;
			set => m_ClientOnly = value;
		}

		[SerializeField]
		private AudioSource m_BGMSource;

		[SerializeField]
		private List<SoundData> m_BGMList;

		[SerializeField]
		private float m_FadeDuration = 1.0f;

		private NetworkVariable<FixedString32Bytes> m_CurrentBgmKey = new("");
		private Coroutine m_FadeCoroutine;
		private Coroutine m_ChangeCoroutine;

		// Start is called once before the first execution of Update after the MonoBehaviour is created
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

		public override void OnNetworkSpawn()
		{
			m_CurrentBgmKey.OnValueChanged += OnBgmValueChanged;

			// 既に曲が設定されている状態で参加した場合の初期再生
			if (!string.IsNullOrEmpty(m_CurrentBgmKey.Value.ToString()))
			{
				Play(m_CurrentBgmKey.Value.ToString());
			}
		}

		public override void OnNetworkDespawn()
		{
			m_CurrentBgmKey.OnValueChanged -= OnBgmValueChanged;
		}

		private void OnBgmValueChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
		{
			ChangeBGM(newValue.ToString());
		}

		/// <summary>
		/// BGMの再生
		/// </summary>
		/// <param name="key">再生する音のKey</param>
		/// <param name="useFade">フェードの使用有無</param>
		public void Play(string key, bool useFade = true)
		{
			if(m_ServerOnly && !IsServer)
			{
				return;
			}
			if (m_ClientOnly && IsServer)
			{
				return;
			}

			if (string.IsNullOrEmpty(key))
			{
				Stop(useFade);
				return;
			}

			var data = m_BGMList.Find(s => s.Key == key);
			if (data == null)
			{
				return;
			}

			if (m_BGMSource.clip == data.Clip && m_BGMSource.isPlaying)
			{
				return;
			}

			m_BGMSource.clip = data.Clip;
			m_BGMSource.loop = data.IsLoop;

			if(useFade)
			{
				if (m_FadeCoroutine != null)
				{
					StopCoroutine(m_FadeCoroutine);
				}
				m_FadeCoroutine = StartCoroutine(FadeIn(data.Volume));
			}
			else
			{
				m_BGMSource.volume = data.Volume;
				m_BGMSource.Play();
			}
		}

		/// <summary>
		/// BGMの停止
		/// </summary>
		/// <param name="useFade">フェードの使用有無</param>
		public void Stop(bool useFade = true)
		{
			if (useFade)
			{
				if (m_FadeCoroutine != null)
				{
					StopCoroutine(m_FadeCoroutine);
				}
				m_FadeCoroutine = StartCoroutine(FadeOut());
			}
			else
			{
				m_BGMSource.Stop();
			}
		}

		/// <summary>
		/// BGMの変更
		/// </summary>
		/// <param name="key">再生する音のKey</param>
		/// <param name="useFade">フェードの使用有無</param>
		public void ChangeBGM(string key, bool useFade = true)
		{
			if (m_ChangeCoroutine != null)
			{
				StopCoroutine(m_ChangeCoroutine);
			}

			if (useFade)
			{
				if (m_ChangeCoroutine != null)
				{
					StopCoroutine(m_ChangeCoroutine);
				}
				m_ChangeCoroutine = StartCoroutine(ChangeRoutine(key));
			}
			else
			{
				Stop(false);
				Play(key, false);
			}
		}

		/// <summary>
		/// BGMの一時停止
		/// </summary>
		public void Pause()
		{
			m_BGMSource.Pause();
		}

		/// <summary>
		/// BGMの再開
		/// </summary>
		public void UnPause()
		{
			m_BGMSource.UnPause();
		}

		/// <summary>
		/// BGM全体のボリュームを変更
		/// </summary>
		/// <param name="volume">ボリューム</param>
		public void SetVolume(float volume)
		{
			m_BGMSource.volume = volume;
		}

		public bool IsPlaying()
		{
			return m_BGMSource.isPlaying;
		}



		/// <summary>
		/// BGM変更コルーチン
		/// </summary>
		/// <param name="key">再生する音のKey</param>
		private IEnumerator ChangeRoutine(string key)
		{
			yield return StartCoroutine(FadeOut());

			if (string.IsNullOrEmpty(key))
			{
				yield break;
			}

			var data = m_BGMList.Find(s => s.Key == key);
			if (data != null)
			{
				m_BGMSource.clip = data.Clip;
				m_BGMSource.loop = data.IsLoop;
				yield return StartCoroutine(FadeIn(data.Volume));
			}
		}

		/// <summary>
		/// フェードインコルーチン
		/// </summary>
		/// <param name="maxVolume">フェード終了するボリューム</param>
		private IEnumerator FadeIn(float maxVolume)
		{
			m_BGMSource.volume = 0;
			m_BGMSource.Play();

			float t = 0;
			while (t < m_FadeDuration)
			{
				t += Time.deltaTime;
				m_BGMSource.volume = Mathf.Lerp(0, maxVolume, t / m_FadeDuration);
				yield return null;
			}
			m_BGMSource.volume = maxVolume;
		}


		/// <summary>
		/// フェードアウトコルーチン
		/// </summary>
		private IEnumerator FadeOut()
		{
			if (!m_BGMSource.isPlaying)
			{
				yield break;
			}

			float startVolume = m_BGMSource.volume;
			float t = 0;
			while (t < m_FadeDuration)
			{
				t += Time.deltaTime;
				m_BGMSource.volume = Mathf.Lerp(startVolume, 0, t / m_FadeDuration);
				yield return null;
			}
			m_BGMSource.Stop();
		}


		//========================= RPC =========================

		/// <summary>
		/// BGM再生のリクエストをサーバー/オーナーに送信<br/>
		/// [送り先: Owner]<br/>
		/// [呼び出し権限: 全クライアント](RpcInvokePermission.Everyone)<br/>
		/// </summary>
		/// <param name="key">再生する音のKey</param>
		[Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
		public void PlayRpc(string key)
		{
			m_CurrentBgmKey.Value = key;
		}

		/// <summary>
		/// BGM停止のリクエストをサーバー/オーナーに送信<br/>
		/// [送り先: Owner]<br/>
		/// [呼び出し権限: 全クライアント](RpcInvokePermission.Everyone)<br/>
		/// </summary>
		[Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
		public void StopRpc()
		{
			m_CurrentBgmKey.Value = "";
		}

		/// <summary>
		/// BGM一時停止のリクエストをサーバー/オーナーに送信<br/>
		/// [送り先: Owner]<br/>
		/// [呼び出し権限: 全クライアント](RpcInvokePermission.Everyone)<br/>
		/// </summary>
		[Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
		public void PauseRpc()
		{
			m_BGMSource.Pause();
		}

		/// <summary>
		/// BGM再開のリクエストをサーバー/オーナーに送信<br/>
		/// [送り先: Owner]<br/>
		/// [呼び出し権限: 全クライアント](RpcInvokePermission.Everyone)<br/>
		/// </summary>
		[Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
		public void UnPauseRpc()
		{
			m_BGMSource.UnPause();
		}

		/// <summary>
		/// ボリューム変更のリクエストをサーバー/オーナーに送信<br/>
		/// [送り先: Owner]<br/>
		/// [呼び出し権限: 全クライアント](RpcInvokePermission.Everyone)<br/>
		/// </summary>
		/// <param name="volume">ボリューム</param>
		public void SetVolumeRpc(float volume)
		{
			m_BGMSource.volume = volume;
		}


	}
}
