using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepAudioPlayer : MonoBehaviour
{
	[SerializeField] private List<AudioClip> walkFootstepSounds;
	[SerializeField] private List<AudioClip> runFootstepSounds;
	[SerializeField] private AudioClip magneticBootsSound;  // This is the magnetic boots sound

	[SerializeField] private float pitchRange = 0.2f; // The range of pitch variation
	[SerializeField] private float volumeRange = 0.1f; // The range of volume variation

	public AudioSource footstepAudioSource;
	public AudioSource magneticBootsAudioSource;

	void Awake()
	{
	}

	public void PlayWalkFootstepSound()
	{
		PlayFootstepSound(walkFootstepSounds);
	}

	public void PlayRunFootstepSound()
	{
		PlayFootstepSound(runFootstepSounds);
	}

	public void PlayMagneticBootsSound()
	{
		PlaySound(magneticBootsSound, magneticBootsAudioSource);
	}

	private void PlayFootstepSound(List<AudioClip> footstepSounds)
	{
		// Select a random audio clip from the list
		AudioClip clipToPlay = footstepSounds[Random.Range(0, footstepSounds.Count)];

		PlaySound(clipToPlay, footstepAudioSource);
	}

	private void PlaySound(AudioClip clipToPlay, AudioSource audioSource)
	{
		// Save the original pitch and volume
		float originalPitch = audioSource.pitch;
		float originalVolume = audioSource.volume;

		// Set a random pitch and volume for variation
		audioSource.pitch = originalPitch + Random.Range(-pitchRange, pitchRange);
		audioSource.volume = originalVolume + Random.Range(-volumeRange, volumeRange);

		// Play the audio clip
		if (!audioSource.isPlaying) {
			audioSource.PlayOneShot(clipToPlay);
		}

		// Reset the pitch and volume back to original after playing
		audioSource.pitch = originalPitch;
		audioSource.volume = originalVolume;
	}
}
