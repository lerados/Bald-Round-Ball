using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableObject : MonoBehaviour {

	public float forceRequired = 10.0f;

	private void OnCollisionEnter(Collision col){
		if(col.impulse.magnitude > forceRequired){
			Destroy (gameObject);
		}
	}

}
