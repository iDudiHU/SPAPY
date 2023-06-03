using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class CameraSwitcherUIButton : MonoBehaviour
{
	//public Button button; // reference to your UI button
	public CinemachineMixingCamera mixingCamera; // reference to your CinemachineMixingCamera

	private int currentCameraIndex = 0;

	private void Start()
	{
		// Attach our SwitchCamera function to the button click event
		//button.onClick.AddListener(SwitchCamera);
	}

	public void SwitchCamera()
	{
		// Increment the current camera index, and wrap back to 0 if it exceeds the count of cameras
		currentCameraIndex = (currentCameraIndex + 1) % mixingCamera.ChildCameras.Length;

		// Set the weight of all child cameras to 0
		for (int i = 0; i < mixingCamera.ChildCameras.Length; i++) {
			mixingCamera.SetWeight(i, 0f);
		}

		// Set the weight of the current camera to 1
		mixingCamera.SetWeight(currentCameraIndex, 1f);
	}
}
