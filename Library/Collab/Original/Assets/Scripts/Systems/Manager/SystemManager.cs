﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Linq;
using UnityEngine.SceneManagement;

//SystemManagerというよりゲーム部分のmanager
public class SystemManager : MonoBehaviour {

	public static SystemManager instance;

	/// <summary>
	/// 現在遊んでいるステージのナンバー
	/// </summary>
	public int stageNum { get; private set; }

	private struct Gimmick {
		public List<Trigger> triggers;
		public List<Parts> parts;
	}
	private Dictionary<int, Gimmick> gimmicks = new Dictionary<int, Gimmick>();

	private float width;
	private float height;

	private void Awake() {
		stageNum = PlayerPrefs.GetInt("stage");
		instance = this;
		DestroyStage();
		CreateStage();

		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	void Start() {
		//OptionManager.instance.ChangeScreenSize(800,450);
	}

	private void OnEnable() {

	}

	void Update() {


    }

	/// <summary>
	/// ステージナンバーを変更する
	/// </summary>
	public void ChangeStageNum(int num) {
		stageNum = num;
	}

	/// <summary>
	/// ポーズ時の処理
	/// </summary>
	private void Pause() {
	 
	}

	/// <summary>
	/// ステージを読み込んで生成する
	/// </summary>
	private void CreateStage() {
		TextAsset xmlTextAsset;
		XmlDocument xmlDoc = new XmlDocument();

		try {
			xmlTextAsset = Instantiate(Resources.Load("XMLs/Stages/"+stageNum.ToString())) as TextAsset;
			xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xmlTextAsset.text);
		} catch {
			Debug.LogError("XMLs/Stages/" + stageNum.ToString() + ".xmlが存在していないか、正しく読み込めません。");
			return;
		}

		try {
			width = int.Parse(xmlDoc.GetElementsByTagName("Width").Item(0).InnerText);
			height = int.Parse(xmlDoc.GetElementsByTagName("Height").Item(0).InnerText);
		} catch {
			Debug.LogError("Width,Heightタグ内に値を記入してください");
			return;
		}

		//トリガーの追加
		var triggers = xmlDoc.GetElementsByTagName("Trigger");
		for(int i = 0; i < triggers.Count; i++) {
			int connectNum = int.Parse(triggers.Item(i).ChildNodes.Item(0).InnerText);
			string name = triggers.Item(i).ChildNodes.Item(1).InnerText;
			float x, y;
			x = float.Parse(triggers.Item(i).ChildNodes.Item(2).InnerText);
			y = float.Parse(triggers.Item(i).ChildNodes.Item(3).InnerText);

			GameObject triggerObject = new GameObject();
			Trigger.TriggerType triggerType = Trigger.TriggerType.Default;
			switch (name) {
				default:
					Debug.LogError("設定されていないトリガーが選ばれました\n名前："+name);
					continue;

				case "RightGear":
					triggerObject = Resources.Load("Prefabs/Triggers/RightGear") as GameObject;
					triggerType = Trigger.TriggerType.RighrtGear;
					break;
				case "Button":
					triggerObject = Resources.Load("Prefabs/Triggers/ButtonTrigger") as GameObject;
					triggerType = Trigger.TriggerType.Button;
					break;
				case "Electrical":
					triggerObject = Resources.Load("Prefabs/Triggers/Electrical") as GameObject;
					triggerType = Trigger.TriggerType.Electrical;
					break;
			}
			var newTriggerObject = Instantiate(triggerObject, new Vector3(x, y, 0), Quaternion.identity, transform) as GameObject;
			Trigger trigger = newTriggerObject.GetComponent<Trigger>();
			trigger.thisType = triggerType;
			trigger.connectNum = connectNum;
			if (gimmicks.ContainsKey(connectNum)) {
				gimmicks[connectNum].triggers.Add(trigger);
			} else {
				Gimmick newGimmick = new Gimmick();
				newGimmick.triggers = new List<Trigger>();
				newGimmick.parts = new List<Parts>();
				newGimmick.triggers.Add(trigger);
				gimmicks.Add(connectNum, newGimmick);
			}
		}


		//パーツの追加
		var parts = xmlDoc.GetElementsByTagName("Parts");
		for(int i = 0; i < parts.Count; i++) {
			int connectNum = int.Parse(parts.Item(i).ChildNodes.Item(0).InnerText);
			if (gimmicks.ContainsKey(connectNum)) {
				string name = parts.Item(i).ChildNodes.Item(1).InnerText;
				float x, y;
				x = float.Parse(parts.Item(i).ChildNodes.Item(2).InnerText);
				y = float.Parse(parts.Item(i).ChildNodes.Item(3).InnerText);

				GameObject partsObject = new GameObject();
				Parts.PartsType partsType = Parts.PartsType.Default;
				switch (name) {
					default:
						Debug.LogError("設定されていないパーツが選ばれました\n名前："+name);
						continue;

					case "Door":
						partsObject = Resources.Load("Prefabs/Parts/Door") as GameObject;
						partsType = Parts.PartsType.Door;
						//他のパーツ
						//partsObject = hoge;
						//partsType = hoge;
						break;

                    case "Bridge":
                        partsObject = Resources.Load("Prefabs/Parts/Bridge") as GameObject;
                        partsType = Parts.PartsType.Bridge;
                        break;

                    case "Bomb":
                        partsObject = Resources.Load("Prefabs/Parts/Bomb") as GameObject;
                        partsType = Parts.PartsType.Bomb;
                        break;
                }
				var newPartsObject = Instantiate(partsObject, new Vector3(x, y, 0), Quaternion.identity, transform) as GameObject;
				var newParts = newPartsObject.GetComponent<Parts>();
				newParts.thisType = partsType;
				gimmicks[connectNum].parts.Add(newParts);

			} else {
				Debug.LogError(connectNum + "番のトリガーを登録してください");
				continue;
			}
		}

		var wall = Resources.Load("Prefabs/StageFrames/Wall") as GameObject;
		var ground = Resources.Load("Prefabs/StageFrames/Ground") as GameObject;
		var player = Instantiate(Resources.Load("Prefabs/Systems/Player") as GameObject, new Vector3(int.Parse(xmlDoc.GetElementsByTagName("StartX").Item(0).InnerText), int.Parse(xmlDoc.GetElementsByTagName("StartY").Item(0).InnerText), 0), Quaternion.identity, transform);

		var wallObject = Instantiate(wall, new Vector3(0, height / 2, 0), Quaternion.identity, transform);
		wallObject.transform.localScale = new Vector3(1, height+1, 1);
		wallObject = Instantiate(wall, new Vector3(width, height / 2, 0), Quaternion.identity, transform);
		wallObject.transform.localScale = new Vector3(1, height+1, 1);

		var groundObject = Instantiate(ground, new Vector3(width/2, 0, 0), Quaternion.identity, transform);
		groundObject.transform.localScale = new Vector3(width+1, 1, 1);
		groundObject = Instantiate(ground, new Vector3(width / 2, height, 0), Quaternion.identity, transform);
		groundObject.transform.localScale = new Vector3(width+1, 1, 1);
		//Todo 背景オブジェクトの追加
	}

	//Triggerが動作したときに全てのパーツを動かす関数
	public void ActionGimmick(int num) {
		foreach(var parts in gimmicks[num].parts) {
			parts.ActionParts();
		}
	}


	/// <summary>
	/// もしgimmicksに何かあった場合全て削除する関数
	/// </summary>
	private void DestroyStage() {
		//Destroy(Player.instance.gameObject);

		var keys = gimmicks.Keys;
		foreach(var key in keys) {
			foreach(Parts parts in gimmicks[key].parts) {
				Destroy(parts.gameObject);
				Debug.LogError("すでに生成されているステージの初期化ができていませんでした。報告してください");
			}
		}
		gimmicks.Clear();
	}
}