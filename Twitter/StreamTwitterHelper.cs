using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StreamTwitterHelper : MonoBehaviour {
	[HideInInspector]public string twitterdata;
	public GameObject TwitterBox;
	//[HideInInspector]public string[,] lines;
	//[HideInInspector]public int line;

	[HideInInspector]public DateTime nowtime;
	string userid;
	string usertime;
	string usercontent;
	float lat;
	float longt;
	Vector2 Center;
	Vector2 Position;
	Rect Rect;

	void Awake () {
		twitterdata = "";
		nowtime =  DateTime.Now;
		Debug.Log (nowtime);
	}
	/*
	 * 
	 */
	// Update to check datetime is right 
	void Update () {
		nowtime =  DateTime.Now;
		if (twitterdata != "") {

            Debug.Log(twitterdata);

			//createTwitterBox(twitterdata);

			//reset twitterdata
			twitterdata = "";
		}


	}

	public void createTwitterBox (string s)
	{

		/*//position
		string[] parse_whole = twitterdata.Split(new string[]{"~|~"},System.StringSplitOptions.None);
		string[] parse_geo = parse_whole [0].Split (',');

		float xposition = float.Parse(parse_geo[1]) * 221220f + 18669724f;  
		//Debug.Log (parse_geo[1]);
		float yposition = float.Parse (parse_geo[0]) * 266844f -9013145f;  

		Vector3 newPosition = new Vector3 (xposition,1.5f,yposition);
		//Debug.Log (newPosition);

		GameObject newbox = Instantiate(TwitterBox,newPosition,Quaternion.identity);
		Text storage = newbox.gameObject.transform.GetChild (0).GetComponent<Text> ();
		storage.text = parse_geo[2] + "\n" + nowtime.ToString () + "\n" + parse_whole[1] + " "  ;

		//Debug.Log (storage.text);*/
	}

	Vector2 calcTile(float lat, float lng)
	{

		float n = Mathf.Pow(2, 16);
		float xtile = n * ((lng + 180) / 360);
		float ytile = n * (1 - (Mathf.Log(Mathf.Tan(Mathf.Deg2Rad * lat) + (1f / Mathf.Cos(Mathf.Deg2Rad * lat))) / Mathf.PI)) / 2f;
		return new Vector2((int)xtile, (int)ytile);
	}
}