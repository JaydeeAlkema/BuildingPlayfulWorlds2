using UnityEngine;

public class AudioManager : MonoBehaviour
{
	#region Variables
	private static AudioManager instance = null;
	#endregion

	#region Properties
	public static AudioManager GetInstance() => instance;
	#endregion

	#region Monobehaviour
	private void Awake()
	{
		if(!GetInstance() || GetInstance() != this) SetInstance(this);
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

	/// <summary>
	/// Sets the instance
	/// </summary>
	/// <param name="value"></param>
	public static void SetInstance(AudioManager value) => instance = value;
	#endregion
}
