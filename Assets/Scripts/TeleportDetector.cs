using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class TeleportDetector : MonoBehaviour
{
	[SerializeField] private bool left;
	[SerializeField] private XRRayInteractor rayInteractor;
	[SerializeField] private TeleportationProvider tpProvider;

	private string fingerTag;
	private string indexTag;
	private int nbFingersInZone;

	private bool teleportIntent;

	private bool indexInZone;

	private Vector3 tpPos;

	private void Awake()
	{
		string hand = left ? "Left" : "Right";
		fingerTag = hand + "Fingers";
		indexTag = hand + "Index";
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag(fingerTag)) ++nbFingersInZone;
		if (nbFingersInZone == 3) TeleportStart();

		if(other.CompareTag(indexTag))
		{
			indexInZone = true;
			if (teleportIntent) TeleportConfirm();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag(fingerTag)) --nbFingersInZone;
		if (nbFingersInZone != 3) TeleportCancel();

		if(other.CompareTag(indexTag)) indexInZone = false;
	}

	private void TeleportStart()
	{
		teleportIntent = true;
		rayInteractor.gameObject.SetActive(true);
	}

	private void TeleportConfirm()
	{
		indexInZone = true;
		if (!rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit)) return;
		tpProvider.QueueTeleportRequest(new() { destinationPosition = hit.point });
	}

	private void TeleportCancel()
	{
		teleportIntent = false;
		rayInteractor.gameObject.SetActive(false);
	}
}
