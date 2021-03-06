﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parts : MonoBehaviour {

    private Transform thisTransform;

    public enum PartsType {
		Default,
		Door,
		Bridge
	}
	[HideInInspector]
	public PartsType thisType = PartsType.Default;

	// Start is called before the first frame update
	void Start() {
        thisTransform = gameObject.GetComponent<Transform>();
		if(thisType == PartsType.Default) {
			Debug.Log("エラー文");
		}
	}

	// Update is called once per frame
	void Update() {

	}


	/// <summary>
	/// 各パーツが動作する条件を満たしたときに呼び出される関数
	/// </summary>
	public void ActionParts() {
        switch (thisType) {
            case PartsType.Door:
                thisTransform.Translate(0, 0.8f * Time.deltaTime, 0);               
                break;

            case PartsType.Bridge:
                if (thisTransform.rotation.eulerAngles.z <= 85) {
                    var rot = new Vector3(0, 0, 20 * Time.deltaTime);
                    transform.Rotate(rot);
                }
                break;

            default:
               
                break;

        }
	}
}
