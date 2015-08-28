using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using Fresvii.AppSteroid;
using Fresvii.AppSteroid.Gui;
using Fresvii.AppSteroid.Services;


public class Chat : MonoBehaviour {

	// チャットキャンバスのコンポーネント
	[HideInInspector]
	public Canvas chatCanvas;

	// グループ内のメンバーを表示するキャンバス
	[HideInInspector]
	public Canvas groupCanvas;

	// メンバーの名前ボタン
	public GameObject nameButton;

	// 画面で表示するチャットログ
	public static string logMessage = "";

	// チャット入力メッセージ
	private string chat = "";

	// メッセージグループの配列
	private IList<Fresvii.AppSteroid.Models.Group> groups = new List<Fresvii.AppSteroid.Models.Group>();

	// チャットを行うグループ
	private Fresvii.AppSteroid.Models.Group inGameChatGroup;


// ----------------- 変数ここまで -----------------------------------------

	// グループメンバーパネルを表示・非表示
	public void ShowGroup() {
		if(!groupCanvas.enabled) {
			groupCanvas.enabled = true;

			StartCoroutine("MakeGroupList");

		} else {
			groupCanvas.enabled = false;

			// COntentの子要素（＝ボタン）を全削除
			foreach(Transform n in GameObject.Find("Content").transform) {
				GameObject.Destroy(n.gameObject);
			}
		}
	}

	// グループリストを表示するコルーチン
	IEnumerator MakeGroupList() {

		yield return new WaitForSeconds(1.0f);

		// グループボタンを作成、並べる
		if(this.groups != null)
		{
			foreach(Fresvii.AppSteroid.Models.Group group in this.groups){
				
				// ボタンテキスト
				string members = "";
				
				// ボタンテキストにメンバー名を追加していく
				if(group.Members != null)
				{
					foreach(Fresvii.AppSteroid.Models.Member member in group.Members)
					{
						members += member.Name + ", ";
					}
				}
				
				// ボタンPrefabを生成、contentパネルの子供にする
				GameObject clone = Instantiate(nameButton) as GameObject;
				clone.transform.SetParent(GameObject.Find("Content").transform);
				
				// ボタンテキストを変更
				clone.GetComponentInChildren<Text>().text = members;
				
				Button button = clone.GetComponent<Button>();
				
				button.onClick.AddListener (() => {
					ShowGroup();
					ShowChat(members, group);
				});
			}
		}
	}


	// チャットパネルを表示・非表示
	public void ShowChat(string title, Fresvii.AppSteroid.Models.Group group) {
		if(!chatCanvas.enabled) {
			chatCanvas.enabled = true;
			inGameChatGroup = group;
			GameObject.Find("Chat_Title").GetComponent<Text>().text = title;

			if(inGameChatGroup != null) {
				//logMessage += "グループIDは" + inGameChatGroup.Id + "\n";
				logMessage += "チャット開始" + "\n";
			} else {
				logMessage += "グループIDが取得できていません" + "\n";
			}
		}
	}

	// チャットパネルを閉じる
	public void CloseChat() {
		chatCanvas.enabled = false;
	}

	// チャット送信
	public void SendChat() {
		FASGroup.SendGroupMessageInGames(inGameChatGroup.Id, chat, delegate(Fresvii.AppSteroid.Models.GroupMessage groupMessage, Fresvii.AppSteroid.Models.Error error)
		                                 {
			if (error != null)
			{
				if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error) 
					Debug.LogError(error.ToString());
				logMessage += error.ToString();
			}
			else
			{
				if (FASConfig.Instance.logLevel <= FAS.LogLevels.Verbose)
					Debug.Log(groupMessage.Text);
				logMessage += groupMessage.User.Name + " : " + groupMessage.Text + "\n";
			}
		});


		// 入力フィールドを空にする
		GameObject.Find("InputField").GetComponent<InputField>().text = "";
	}


	// グループメッセージ作成イベントを登録
	void OnEnable()
	{
		FASEvent.OnGroupMessageInGameCreated += OnGroupMessageInGameCreated;
	}

	// グループメッセージ作成イベントを解除
	void OnDisable()
	{
		FASEvent.OnGroupMessageInGameCreated -= OnGroupMessageInGameCreated;
	}

	// メッセージ受信
	void OnGroupMessageInGameCreated(Fresvii.AppSteroid.Models.GroupMessage groupMessage)
	{
		Debug.Log("In Game chat : " + groupMessage.User.Name + " : " + groupMessage.Text);
		logMessage += groupMessage.User.Name + " : " + groupMessage.Text + "\n";
	}

	IEnumerator Start () {
		chatCanvas = GameObject.Find("ChatCanvas").GetComponent<Canvas>();
		groupCanvas = GameObject.Find("GroupCanvas").GetComponent<Canvas>();
		groupCanvas.enabled = false;

		// 初期化処理完了まで待機
		while (!FAS.Initialized) 
		{
			yield return 1;
		}

		// グループリストを取得
		FASGroup.GetGroupList(delegate(IList<Fresvii.AppSteroid.Models.Group> groups, Fresvii.AppSteroid.Models.Error error)
		                      {
			if(error == null){
				this.groups = groups;
				foreach(Fresvii.AppSteroid.Models.Group group in this.groups)
				{
					group.FetchMembers(delegate {});
				}
			}
			else
			{
				Debug.LogError(error.ToString());
			}
		});
	}
	

	void Update () {
		// チャットログエリアにログを表示
		GameObject.Find ("ChatLog").GetComponent<Text>().text = logMessage;

		// 入力メッセージをchat変数に格納
		chat = GameObject.Find("InputField").GetComponent<InputField>().text;
	}
}
