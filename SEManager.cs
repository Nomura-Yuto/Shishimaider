using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

namespace MS.Games.Sounds
{
	public class SEManager : NetworkBehaviour
	{
		[Tooltip("サーバー(ホスト)でのみ再生")]
		[SerializeField]
		private bool m_ServerOnly;

		[Tooltip("クライアントでのみ再生")]
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
		private AudioSource m_SESource;

		[SerializeField]
		private List<SoundData> m_SEList;

		[Tooltip("ループSE管理用辞書")]
		private Dictionary<string, AudioSource> m_LoopingSources = new Dictionary<string, AudioSource>();

		// Start is called once before the first execution of Update after the MonoBehaviour is created
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

		/// <summary>
		/// SEの再生
		/// </summary>
		/// <param name="key">再生する音のKey</param>
		public void Play(string key)
		{
			if (m_ServerOnly && !IsServer)
			{
				return;
			}
			if (m_ClientOnly && IsServer)
			{
				return;
			}

			var data = m_SEList.Find(s => s.Key == key);

			if (data == null)
			{
				return;
			}

			if (data.Delay > 0)
			{
				StartCoroutine(DelayPlay(data));
			}
			else
			{
				m_SESource.PlayOneShot(data.Clip, data.Volume);
			}
		}

		/// <summary>
		/// SEの再生
		/// </summary>
		/// <param name="key">再生する音のKey</param>
		/// <param name="position">再生する座標</param>
		public void PlayAtPosition(string key, Vector3 position)
		{
			if (m_ServerOnly && !IsServer)
			{
				return;
			}
			if (m_ClientOnly && IsServer)
			{
				return;
			}

			var data = m_SEList.Find(s => s.Key == key);
			if (data == null)
			{
				return;
			}

			if (data.Delay > 0)
			{
				StartCoroutine(DelayPlayAtPosition(data, position));
			}
			else
			{
				AudioSource.PlayClipAtPoint(data.Clip, position, data.Volume);
			}
		}

		/// <summary>
		/// SEの再生
		/// </summary>
		/// <param name="key">再生する音のKey</param>
		/// <param name="transform">親子付けするTransform</param>
		public void PlayLoop(string key, Transform parent)
		{
			if (m_ServerOnly && !IsServer)
			{
				return;
			}
			if (m_ClientOnly && IsServer)
			{
				return;
			}

			var data = m_SEList.Find(s => s.Key == key);
			if (data == null)
			{
				return;
			}

			if (data.Delay > 0)
			{
				StartCoroutine(DelayPlayLoop(data, parent));
			}
			else
			{
				CreateSource(data, parent);
			}
		}

		/// <summary>
		/// SEの停止
		/// </summary>
		public void Stop()
		{
			m_SESource.Stop();
		}

		/// <summary>
		/// ループSEの停止
		/// </summary>
		/// <param name="key">停止する音のKey</param>
		/// <param name="transform">親子付けしたTransform</param>
		public void StopLoop(string key, Transform parent = null)
		{
			string dictKey = parent != null ? $"{key}_{parent.name}" : key;
			if (m_LoopingSources.TryGetValue(dictKey, out AudioSource source))
			{
				if (source != null) Destroy(source.gameObject);
				m_LoopingSources.Remove(dictKey);
			}
		}

		/// <summary>
		/// SEの一時停止
		/// </summary>
		public void Pause()
		{
			m_SESource.Pause();
		}

		/// <summary>
		/// SEの再開
		/// </summary>
		public void UnPause()
		{
			m_SESource.UnPause();
		}

		/// <summary>
		/// SE全体のボリュームを変更
		/// </summary>
		/// <param name="volume">ボリューム</param>
		public void SetVolume(float volume)
		{
			m_SESource.volume = volume;
		}


		/// <summary>
		/// ループ用・追従用のAudioSourceを生成して辞書に追加
		/// </summary>
		/// <param name="data">追加するサウンドデータ</param>
		/// <param name="parent">親子付けするTransform</param>
		private AudioSource CreateSource(SoundData data, Transform parent = null)
		{
			string dictKey = parent != null ? $"{data.Key}_{parent.name}" : data.Key;

			if (m_LoopingSources.ContainsKey(dictKey))
			{
				return null;
			}

			GameObject obj = new GameObject($"SE_{data.Key}");
			obj.transform.SetParent(parent ?? transform);
			obj.transform.localPosition = Vector3.zero;

			AudioSource newSource = obj.AddComponent<AudioSource>();
			newSource.clip = data.Clip;
			newSource.volume = data.Volume;
			newSource.loop = data.IsLoop;
			newSource.spatialBlend = (parent != null || transform != transform) ? 1.0f : 0.0f;

			newSource.Play();

			if (data.IsLoop)
			{
				m_LoopingSources.Add(dictKey, newSource);
			}
			else
			{
				// ループしない場合再生終了後に削除予約
				Destroy(obj, data.Clip.length + 0.1f);
			}

			return newSource;
		}

		/// <summary>
		/// SEの遅延再生コルーチン
		/// </summary>
		/// <param name="data">サウンドデータ</param>
		private IEnumerator DelayPlay(SoundData data)
		{
			yield return new WaitForSeconds(data.Delay);
			m_SESource.PlayOneShot(data.Clip, data.Volume);
		}

		/// <summary>
		/// SEの遅延再生コルーチン
		/// </summary>
		/// <param name="data">サウンドデータ</param>
		/// <param name="position">再生する座標</param>
		private IEnumerator DelayPlayAtPosition(SoundData data, Vector3 position)
		{
			yield return new WaitForSeconds(data.Delay);
			AudioSource.PlayClipAtPoint(data.Clip, position, data.Volume);
		}

		/// <summary>
		/// ループSEの遅延再生コルーチン
		/// </summary>
		/// <param name="data">サウンドデータ</param>
		/// <param name="position">再生する座標</param>
		private IEnumerator DelayPlayLoop(SoundData data, Transform parent)
		{
			yield return new WaitForSeconds(data.Delay);
			CreateSource(data, parent);
		}


		//========================= RPC =========================

		/// <summary>
		/// SE再生のリクエストをサーバー/オーナーに送信<br/>
		/// [送り先: Owner]<br/>
		/// [呼び出し権限: 全クライアント](RpcInvokePermission.Everyone)<br/>
		/// </summary>
		/// <param name="key">再生する音のKey</param>
		[Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
		public void PlayRpc(string key)
		{
			Play(key);
		}

		/// <summary>
		/// SE再生のリクエストをサーバー/オーナーに送信<br/>
		/// [送り先: Owner]<br/>
		/// [呼び出し権限: 全クライアント](RpcInvokePermission.Everyone)<br/>
		/// </summary>
		/// <param name="key">再生する音のKey</param>
		/// <param name="position">再生する座標</param>
		[Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
		public void PlayAtPositionRpc(string key, Vector3 position)
		{
			PlayAtPosition(key, position);
		}

		/// <summary>
		/// SE停止のリクエストをサーバー/オーナーに送信<br/>
		/// [送り先: Owner]<br/>
		/// [呼び出し権限: 全クライアント](RpcInvokePermission.Everyone)<br/>
		/// </summary>
		[Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
		public void StopRpc()
		{
			m_SESource.Stop();
		}

		/// <summary>
		/// SE一時停止のリクエストをサーバー/オーナーに送信<br/>
		/// [送り先: Owner]<br/>
		/// [呼び出し権限: 全クライアント](RpcInvokePermission.Everyone)<br/>
		/// </summary>
		[Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
		public void PauseRpc()
		{
			m_SESource.Pause();
		}

		/// <summary>
		/// SE再開のリクエストをサーバー/オーナーに送信<br/>
		/// [送り先: Owner]<br/>
		/// [呼び出し権限: 全クライアント](RpcInvokePermission.Everyone)<br/>
		/// </summary>
		[Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
		public void UnPauseRpc()
		{
			m_SESource.UnPause();
		}

		/// <summary>
		/// ボリューム変更のリクエストをサーバー/オーナーに送信<br/>
		/// [送り先: Owner]<br/>
		/// [呼び出し権限: 全クライアント](RpcInvokePermission.Everyone)<br/>
		/// </summary>
		/// <param name="volume">ボリューム</param>
		public void SetVolumeRpc(float volume)
		{
			m_SESource.volume = volume;
		}


	}
}
