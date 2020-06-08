using UnityEngine;

public class AudioManager : MonoBehaviour
{
	#region Variables
	private AudioManager instance = null;
	#endregion

	#region Properties
	public AudioManager Instance { get => instance; set => instance = value; }
	#endregion

	#region Monobehaviour
	private void Awake()
	{
		if(!Instance || Instance != this) Instance = this;
	}
	#endregion

	#region Public Voids
	/// <summary>
	/// Plays a sound effect.
	/// </summary>
	/// <param name="clip">Which Audio Clip to play.</param>
	/// <param name="pos">Where the audio will be played. (World Space)</param>
	/// <param name="volume">How loud the clip should be.</param>
	public void PlaySoundFX(AudioClip clip, Vector3 pos, float volume) => AudioSource.PlayClipAtPoint(clip, pos, volume);
	#endregion
}
