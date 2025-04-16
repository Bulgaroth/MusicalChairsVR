using UnityEngine;

public class Chair : MonoBehaviour
{
	private Transform linkedTransform;

	public bool isUsed { get { return linkedTransform != null; } }

	public void SetUsedBy(Transform user)
	{
		if (isUsed) return;
		linkedTransform = user;
		print($"Utilisé par : {user.name}");
	}
}
