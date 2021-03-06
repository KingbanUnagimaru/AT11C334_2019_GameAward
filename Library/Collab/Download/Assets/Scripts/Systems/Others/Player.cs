﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public static Player instance;

	private bool isRight = true;

	[SerializeField]
    private float moveSpeed = MOVE_SPEED;
	private const float MOVE_SPEED = 10f;
	private float rotateSpeed = 225f;

	/// <summary>
	/// transformを使う際にはこれを使用すること
	/// </summary>
	private Transform thisTransform;
	public Transform visualTransform;
	public Transform underTransform;

	[HideInInspector]
	public bool isGimmickMode = false;
	private Rigidbody rigidbody;
	private Collider thisCollider;

	//[SerializeField]
	private bool isJumping;
	private float jumpRotateSpeed;
	private float jumpRange;
	private const float JUMP_RANGE_ADD_VALUE = 10f;
	private const float ROTATE_ADD_SPEED = 20f;

	private bool canRightMove = true;
	private bool canLeftMove = true;

    private void Awake() {
        instance = this;
		Destroy(Camera.main.gameObject);
    }

    // Start is called before the first frame update
    void Start() {
		thisTransform = GetComponent<Transform>();
		rigidbody = gameObject.GetComponent<Rigidbody>();
		thisCollider = gameObject.GetComponent<Collider>();
	}

	// Update is called once per frame
	void Update() {
		MovePlayer();

		if (Input.GetKeyDown(KeyCode.W) && isJumping != true) {
			jumpRange += 2f;
			jumpRotateSpeed = 0;
		}

		if (Input.GetKey(KeyCode.W) && isJumping != true) {
			if(jumpRange < 10) {
				jumpRange += JUMP_RANGE_ADD_VALUE * Time.deltaTime;
				thisTransform.localScale -= new Vector3(0,0.005f,0f);
				visualTransform.position -= (visualTransform.position - underTransform.position) * 0.015f;
				jumpRotateSpeed += ROTATE_ADD_SPEED * Time.deltaTime;
			}

		}
		if (Input.GetKeyUp(KeyCode.W) && isJumping != true) {
			rigidbody.velocity = new Vector3(0, jumpRange, 0);
			thisTransform.localScale = new Vector3(1, 1, 1);
			visualTransform.localPosition = new Vector3(0, 0, 0);
			isJumping = true;
			jumpRange = 0;
			if(isRight == false) {
				jumpRotateSpeed *= -1;
			}
		}

		if(isJumping == true) {
			visualTransform.Rotate(0, -jumpRotateSpeed,0);
			jumpRotateSpeed *= 0.98f;
		}
	}

	/// <summary>
	/// プレイヤーが死んだときの処理
	/// </summary>
	/// <param name="reason">死因</param>
	private void PlayerDead(string reason) {
		switch (reason) {
			default:
				//reasonをエラー文に書く
				Debug.LogError("エラー文");
				break;

				//他死因によって処理や描画を変える
		}

	}

	/// <summary>
	/// コントローラーとキーボード押したら移動
	/// </summary>
	private void MovePlayer() {
		if (Input.GetKey(KeyCode.W)) {
			moveSpeed = MOVE_SPEED / 5;

		} else {
			moveSpeed = MOVE_SPEED;
		}

		if (Input.GetKey(KeyCode.D)) {
			if(isGimmickMode == false && canRightMove) {
				transform.position += Vector3.right * moveSpeed * Time.deltaTime;
			}
			var rot = new Vector3(0, -rotateSpeed * Time.deltaTime,0);
			visualTransform.Rotate(rot);
			isRight = true;
		}

		if (Input.GetKey(KeyCode.A) && isGimmickMode == false) {
			if (isGimmickMode == false && canLeftMove) {
				transform.position -= Vector3.right * moveSpeed * Time.deltaTime;
			}
			var rot = new Vector3(0, rotateSpeed * Time.deltaTime,0);
			visualTransform.Rotate(rot);
			isRight = false;
		}

	}

	/// <summary>
	/// コントローラーとキーボード押したらジャンプ
	/// </summary>
	private void JumpPlayer() {

		//jumpSpeedを使用
	}


	private void ShiftGimmickMode() {
		//判定は当たり判定側などで取ること
		isGimmickMode = !isGimmickMode;
	}


	/// <summary>
	/// ゴールギミックを動作させたときに使う
	/// </summary>
	private void StageClear() {	
		
	}

	private void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.CompareTag("Ground")) {
			rigidbody.AddForce(new Vector3(0, 1, 0));

			if (collision.gameObject.transform.position.y <= thisTransform.position.y) {
				isJumping = false;
			}
		}

		if (collision.gameObject.CompareTag("Wall")) {
			if(collision.gameObject.transform.position.x >= thisTransform.position.x) {
				//右側に行けない
				canRightMove = false;
			} else {
				canLeftMove = false;
				//左側
			}
		}
	}

	private void OnCollisionExit(Collision collision) {
		if (collision.gameObject.CompareTag("Wall")) {
			canLeftMove = true;
			canRightMove = true;
		}
	}

	private void OnCollisionStay(Collision collision) {
	}

	private void OnTriggerStay(Collider other) {
        if (other.gameObject.CompareTag("Trigger")) {
			Trigger trigger = other.gameObject.transform.parent.gameObject.GetComponent<Trigger>();
			if(trigger.thisType == Trigger.TriggerType.Button|| trigger.thisType == Trigger.TriggerType.MinusButton) {
				trigger.isThisGimmick = true;
            }

			if (Input.GetKeyDown(KeyCode.Space)) {

				if (trigger.thisType == Trigger.TriggerType.Electrical ||
					trigger.thisType == Trigger.TriggerType.LeftGear ||
					trigger.thisType == Trigger.TriggerType.RighrtGear) {
				isGimmickMode = !isGimmickMode;
				trigger.isThisGimmick = true;
				trigger.mesh.enabled = !isGimmickMode;
					thisTransform.position = other.gameObject.transform.position;
				}
			}

		}

		if (other.gameObject.CompareTag("MissGround")) {
			Instantiate(Resources.Load("Prefabs/Systems/GameOver") as GameObject, transform.position, Quaternion.identity);
			Destroy(this);
		}
    }

	private void OnTriggerExit(Collider other) {
		if (other.gameObject.CompareTag("Trigger")) {
			isGimmickMode = false;
            other.gameObject.transform.parent.gameObject.GetComponent<Trigger>().isThisGimmick = false;

		}
    }

   
}
