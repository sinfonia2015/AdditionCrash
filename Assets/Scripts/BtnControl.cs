using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class BtnControl : MonoBehaviour {
	GameControler gameControler;

	public void Clicked() {
		if(GetComponent<Toggle>().enabled == false) {
			//Debug.Log (this.name + "がクリックされた");

			gameControler.Judge(this.gameObject);

		}
	}

	public void Change() {
		if(this.GetComponent<Toggle>().isOn) {
			this.GetComponent<Image>().color = new Color(0.3f, 1.0f, 1.0f);
			gameControler.firstObj = this.gameObject;
			gameControler.firstNum = int.Parse(this.transform.FindChild("Text").GetComponent<Text>().text);

			// 自分以外のボタンのトグルを非アクティブにする
			OtherToggleOff();

		} else {
			this.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f);
			// 自分以外のボタンのトグルをアクティブにする
			OtherToggleOn();
		}
	}

	// 自分以外のボタンのトグルを非アクティブにする
	void OtherToggleOff() {
		foreach(GameObject g in gameControler.allButtons) {
			if(g != this.gameObject) {
				g.GetComponent<Toggle>().enabled = false;
			}
		}
	}

	// 自分以外のボタンのトグルをアクティブにする
	public void OtherToggleOn() {
		foreach(GameObject g in gameControler.allButtons) {
			if(g != this.gameObject) {
				g.GetComponent<Toggle>().enabled = true;
			}
		}
	}

	void Start () {
		gameControler = GameObject.Find ("GameControler").GetComponent<GameControler>();
	}
	
	// Update is called once per frame
	void Update () {
	}
}



// 2015.06.06
// Copyrightⓒ Sinfonia LLC. All Rights Reserved.
