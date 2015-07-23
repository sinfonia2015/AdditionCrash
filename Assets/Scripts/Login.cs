using UnityEngine;
using System.Collections;
using Fresvii.AppSteroid;
using Fresvii.AppSteroid.Models;
using System.Collections.Generic;

public class Login : MonoBehaviour {

	public void StartCommunity() {
		FASGui.ShowGUI(FASGui.Mode.All,"start"); // Step 3
	}

	IEnumerator Start() {
		while (!FAS.Initialized) // 初期化処理完了まで待機
		{
			yield return 1;
		}

		//  Get signed up users list
		List<User> users = FASUser.LoadSignedUpUsers();
		
		//  If signed up user already exists
		if (users.Count > 0) // Step 2 - Case 2
		{
			User user = users[users.Count - 1]; //  In this case, we use latest signed up user account.
			
			FASUser.LogIn(user.Id, user.Token, delegate(Error error)
			              {
				if (error == null)
				{
					Debug.Log("ログイン成功");
				}
				else
				{
					Debug.LogError(error.ToString());
				}
			});
			yield return null;
		}
		//  If signed up user does not exist
		else // Step 2 - Case 1
		{
			FASUser.SignUp(delegate(User user, Error error)
			               {
				if (error == null)
				{
					FASUser.LogIn(user.Id, user.Token, delegate(Error error2)
					              {
						if (error2 == null)
						{
							Debug.Log("ログイン成功");
						}
						else
						{
							Debug.LogError(error2.ToString()); // Log in error
						}
					});
				}
				else
				{
					Debug.LogError(error.ToString()); // Sign up error
				}
			});
		}


	}

}
