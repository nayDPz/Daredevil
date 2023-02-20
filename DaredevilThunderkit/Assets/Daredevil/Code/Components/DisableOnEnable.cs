using UnityEngine;

public class DisableOnEnable : MonoBehaviour
{
	public GameObject objectToDisable;

	private void OnEnable()
	{
		if (objectToDisable)
		{
			objectToDisable.SetActive(false);
		}
	}

	private void OnDisable()
	{
		if (objectToDisable)
		{
			objectToDisable.SetActive(true);
		}
	}
}
