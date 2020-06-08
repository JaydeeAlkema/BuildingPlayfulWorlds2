using UnityEngine;

public class AudioManager : MonoBehaviour
{
	#region Variables
	private AudioManager instance = null;
	#endregion

	#region Monobehaviour
	private void Awake()
	{
		if(!instance || instance != this) instance = this;
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
