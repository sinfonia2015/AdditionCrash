using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Fresvii.AppSteroid;
using Fresvii.AppSteroid.Models;

public class GameControler : MonoBehaviour {
	
	//-------------- public変数　--------------------------------
	
	public GameObject stageNumberText;
	public GameObject targetNumberText;
	public GameObject limitTimeText;
	public GameObject timeBar;
	public GameObject Face;
	public GameObject endCancas;
	public GameObject endStageText;
	public GameObject endScoreText;

	public GameObject clearCancas;
	public GameObject clearStageText;
	public GameObject stageBonusText;
	public GameObject timeBonusText;
	public GameObject thisTotalText;
	public GameObject allScoreText;

	public Sprite[] Faces;

	public int stageBonus = 100;
	
	public enum GameState {
		PLAYING,
		CLEAR,
		END
	}
	
	public List<GameObject> allButtons = new List<GameObject>();
	
	
	// 一個目選択のGameObject
	[HideInInspector]
	public GameObject firstObj;
	
	// 一個目選択の数値
	[HideInInspector]
	public int firstNum;
	

	//-------------- private変数　--------------------------------

	// AppSteroidのリーダーボードID
	string leaderboardId = "d935734f4c894c25a61b8aa8d54d32c8";

	int stageNumber = 1;
	int targetNumber;

	GameObject[] FirstAllBtns;

	private GameState state;
	
	// ステージごとの制限時間
	private float limitTime;
	
	// タイマーの静止状態
	bool paused = true;
	
	// 各ボタンの数字を保管する連想配列
	Dictionary<GameObject, int> buttonNumSet = new Dictionary<GameObject, int>();
	
	// 累計ポイント
	int totalScore;
	// タイムボーナス
	int timeBonus;

	// クリアフラグ
	bool isClear;
	
	
	
	//-------------- 変数ここまで　--------------------------------
	
	// ターゲット数字を出力
	void GetTargetNumber() {
		if(stageNumber <= 3) {
			int i;
			i = Random.Range(1,6);
			targetNumber = i * 10;
		} else if(stageNumber >= 4 && stageNumber <= 6) {
			int i;
			i = Random.Range(6,11);
			targetNumber = i * 10;
		} else if(stageNumber >= 7 && stageNumber <= 9) {
			targetNumber = Random.Range (11,31);
		} else if(stageNumber >= 10 && stageNumber <= 12) {
			targetNumber = Random.Range (31,51);
		} else if(stageNumber >= 13 && stageNumber <= 15) {
			targetNumber = Random.Range (51,71);
		} else if(stageNumber >= 16 && stageNumber <= 18) {
			targetNumber = Random.Range (71,101);
		} else {
			targetNumber = Random.Range (51,101);
		}

		Debug.Log ("ターゲット数字は" + targetNumber.ToString());
	}
	
	// 制限時間を決定
	void GetLimitTime() {
		if(stageNumber <= 5) {
			limitTime = 50.0f;
		} else if(stageNumber >= 6 && stageNumber <= 10) {
			limitTime = 45.0f;
		} else if(stageNumber >= 11 && stageNumber <= 15) {
			limitTime = 40.0f;
		} else if(stageNumber >= 16 && stageNumber <= 20) {
			limitTime = 35.0f;
		} else if(stageNumber >= 21 && stageNumber <= 25) {
			limitTime = 30.0f;
		} else if(stageNumber >= 26 && stageNumber <= 30) {
			limitTime = 25.0f;
		} else if(stageNumber >= 31 && stageNumber <= 35) {
			limitTime = 20.0f;
		} else {
			limitTime = 15.0f;
		}
	}
	
	//ボタンと数字のセットを作成
	void ButtonNumSets() {
		int j = 0;
		for(int i = 0; i < allButtons.Count; i++) {
			if(i == 0 || i % 2 == 0) {
				j = Random.Range (1, targetNumber);
				buttonNumSet.Add(allButtons[i], j);
			} else {
				buttonNumSet.Add(allButtons[i], targetNumber - j);
			}
			allButtons[i].transform.FindChild("Text").GetComponent<Text>().text = buttonNumSet[allButtons[i]].ToString();
		}
	}
	
	
	// 正解かどうかをジャッジ。正解なら２つのタイルを消す + アイコン変化。不正解なら無反応　+　アイコン変化
	public void Judge(GameObject secondObj) {
		int secondNum;
		// Debug.Log("２つめのオブジェクトは" + secondObj.name);
		secondNum = int.Parse(secondObj.transform.FindChild("Text").GetComponent<Text>().text);
		// Debug.Log("２つめの数字は" + secondNum);
		
		if(firstNum + secondNum == targetNumber) {
			Debug.Log ("正解！");

			StartCoroutine("FaceChange", Faces[1]);
			firstObj.GetComponent<BtnControl>().OtherToggleOn();
			
			DestroyTile(secondObj);
			DestroyTile(firstObj);

			allButtons.Remove(firstObj);
			allButtons.Remove(secondObj);

			if(allButtons.Count == 0) {
				Debug.Log("オールクリア！");

				isClear = true;
			}
		} else {
			StartCoroutine("FaceChange", Faces[2]);
			Debug.Log ("ハズレ");
		}
	}
	
	// タイルを消す
	void DestroyTile(GameObject i) {
		i.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f);
		i.GetComponent<Image>().enabled = false;
		i.transform.FindChild("Text").GetComponent<Text>().enabled = false;
	}

	// すべてのタイルを表示する
	void AllTileShow() {
		foreach(GameObject g in allButtons) {
			g.GetComponent<Image>().enabled = true;
			g.transform.FindChild("Text").GetComponent<Text>().enabled = true;
		}
	}

	// 顔変化のコルーチン
	IEnumerator FaceChange(Sprite i) {
		Face.GetComponent<Image>().sprite = i;
		if(i == Faces[1] || i == Faces[2]) {
			yield return new WaitForSeconds(1.0f);
			Face.GetComponent<Image>().sprite = Faces[0];
		} 
	}

	public void GoTitle() {
		Application.LoadLevel("start");
	}

	public void ReStart() {
		isClear = false;
		timeBonus = 0;
		clearCancas.GetComponent<Canvas>().enabled = false;
	
		stageNumber += 1;
		stageNumberText.GetComponent<Text>().text = "stage <size=42>" + stageNumber + "</size>";

		//ターゲット数字を決定
		GetTargetNumber();
		targetNumberText.GetComponent<Text>().text = targetNumber.ToString();
		// 制限時間を決定
		GetLimitTime();
		limitTimeText.GetComponent<Text>().text = Mathf.FloorToInt(limitTime) + " sec";

		//すべてのボタンをクリア
		buttonNumSet.Clear();

		//ボタン配列を復活
		allButtons.AddRange(FirstAllBtns);

		// ボタンに数字をセット
		ButtonNumSets();

		//すべてのボタンを表示
		AllTileShow();

		timeBar.GetComponent<Image>().fillAmount = 1.0f;
		paused = false;
		state = GameState.PLAYING;
	}

	// リーダーボードを表示
	public void ShowLeaderBoard() {
		FASGui.ShowGUI(FASGui.Mode.All, "start", FASGui.Mode.Leaderboards);
	}


	void Start () {
		
		stageNumberText.GetComponent<Text>().text = "stage <size=42>" + stageNumber + "</size>";
		
		//ターゲット数字を決定
		GetTargetNumber();
		targetNumberText.GetComponent<Text>().text = targetNumber.ToString();
		
		// 制限時間を決定
		GetLimitTime();
		limitTimeText.GetComponent<Text>().text = Mathf.FloorToInt(limitTime) + " sec";
		
		paused = false;
		
		// すべてのボタンを配列化
		FirstAllBtns = GameObject.FindGameObjectsWithTag("tile");
		allButtons.AddRange(FirstAllBtns);
		
		// ボタンに数字をセット
		ButtonNumSets();
		
		state = GameState.PLAYING;
	}
	
	
	void Update () {
		switch(state) {
		case GameState.PLAYING:
			if(!paused){
				timeBar.GetComponent<Image>().fillAmount -= Time.deltaTime / limitTime;
			}

			if(timeBar.GetComponent<Image>().fillAmount == 0.0f) {

				Debug.Log("時間終了");
				StartCoroutine("FaceChange", Faces[4]);
				endCancas.GetComponent<Canvas>().enabled = true;
				endStageText.GetComponent<Text>().text = "Stage Clear  " + (stageNumber - 1) ;
				endScoreText.GetComponent<Text>().text = "累計ポイント  " + totalScore;

				//リーダーボードにスコアを送信後、リーダーボードIDをセット
				FASLeaderboard.ReportScore(leaderboardId, totalScore, delegate(Score score, Error error)
				{
					if (error == null)
					{
						Debug.Log("Report score success : " + score.User.Name + " : " + score.Value);
						FASGui.SetLeaderboardId(leaderboardId);
					}
					else
					{
						Debug.LogError("Report score error : " + error.ToString());
					}
				});

				state = GameState.END;
			}

			if(isClear) {
				StartCoroutine("FaceChange", Faces[3]);
				paused = true;
				
				float i = timeBar.GetComponent<Image>().fillAmount * limitTime ;
				timeBonus = Mathf.CeilToInt(i);
				
				clearCancas.GetComponent<Canvas>().enabled = true;
				clearStageText.GetComponent<Text>().text = "Stage " + stageNumber;
				stageBonusText.GetComponent<Text>().text = "Stage Bonus  " + stageBonus;
				timeBonusText.GetComponent<Text>().text = "Time Bonus     " + timeBonus;
				thisTotalText.GetComponent<Text>().text = "Total               " + (stageBonus + timeBonus);
				totalScore += (stageBonus + timeBonus);
				allScoreText.GetComponent<Text>().text = totalScore.ToString();

				state = GameState.CLEAR;
			}

			break;
		case GameState.CLEAR:
			break;
		case GameState.END:
			break;
		}
	}
}



// 2015.06.06
// Copyrightⓒ Sinfonia LLC. All Rights Reserved.
