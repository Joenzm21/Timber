using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrunkController : MonoBehaviour {

	private bool flyOut = false;
	private int trunkType; 

	private Vector3 flyDirection;
	private Vector3 rotationDirection;

	public int TrunkType { set { trunkType = value; } get { return trunkType; } }

	void Update ()
	{
		if(flyOut)
		{
			transform.position += flyDirection * 7 * Time.deltaTime;
			transform.Rotate(rotationDirection * 150 * Time.deltaTime);
		}
	}

	public void FlyOut(int direction)
	{ 
		gameObject.GetComponent<SpriteRenderer>().sortingOrder = 1;
		flyOut = true;
		if(direction == -1)
		{
			flyDirection = new Vector3(2, -0.1f, 0);
			rotationDirection = Vector3.back;
		}
		else 
		{
			flyDirection = new Vector3(-2, -0.1f, 0);
			rotationDirection = Vector3.forward;
		}
		StartCoroutine(Wait());
	}
	IEnumerator Wait()
	{
		yield return new WaitForSeconds(1f);
		Destroy(gameObject);
	}
}
