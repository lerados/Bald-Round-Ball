using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelData{

	public LevelData(string levelName){
		string data = PlayerPrefs.GetString (levelName);
		if (data == "") {
			return;
		} else {
			string[] allData = data.Split ('&');
			BestTime = float.Parse (allData [0]);
			SilverTime = float.Parse (allData [1]);
			GoldTIme = float.Parse (allData [2]);
		}
	}

	public float BestTime { set; get; }
	public float SilverTime { set; get; }
	public float GoldTIme { set; get; }

}

public class MainMenu : MonoBehaviour {

	private const float Camera_Transition_Speed = 3.0f;

	public GameObject levelButtonPrefab;
	public GameObject levelButtonContainer;
	public GameObject shopButtonPrefab;
	public GameObject shopButtonContainer;
	public Text currencytext;

	public Material playerMaterial;

	private Transform cameraTransform;
	private Transform cameraDesiredLookkAt;

	private bool nextLevelLocked = false;

	private int[] costs = 	{0,150,150,150,
							300,300,300,300,
							500,500,500,500,
							1000,1250,1500,2000};

	private void Start(){
		ChangePlayerSkin(GameManager.Instance.currentSkinIndex);
		currencytext.text = "Currency : " + GameManager.Instance.currency.ToString();
		cameraTransform = Camera.main.transform;

		Sprite[] thumbnails = Resources.LoadAll<Sprite>("Levels");
		foreach(Sprite thumbnail in thumbnails){
			GameObject container = Instantiate (levelButtonPrefab) as GameObject;
			container.GetComponent<Image>().sprite = thumbnail;
			container.transform.SetParent(levelButtonContainer.transform, false);
			LevelData level =  new LevelData (thumbnail.name);
			container.transform.GetChild (0).GetChild (0).GetComponent<Text> ().text = (level.BestTime != 0.0f) ? level.BestTime.ToString ("f") : "" ;

			container.transform.GetChild (1).GetComponent<Image> ().enabled = nextLevelLocked;
			container.GetComponent<Button>().interactable = !nextLevelLocked;

			if(level.BestTime == 0.0f){
				nextLevelLocked = true;
			}

			string sceneName = thumbnail.name;
			container.GetComponent<Button>().onClick.AddListener(() => LoadLevel(sceneName));
		}

		int textureIndex = 0;
		Sprite[] textures = Resources.LoadAll<Sprite>("Player");
		foreach(Sprite texture in textures){
			GameObject container = Instantiate (shopButtonPrefab) as GameObject;
			container.GetComponent<Image>().sprite = texture;
			container.transform.SetParent(shopButtonContainer.transform, false);

			int index = textureIndex;
			container.GetComponent<Button>().onClick.AddListener(() => ChangePlayerSkin(index));
			container.transform.GetChild (0).GetChild (0).GetComponent<Text> ().text = costs [index].ToString ();
			if((GameManager.Instance.skinAvailability & 1 << index) == 1 << index){
				container.transform.GetChild(0).gameObject.SetActive(false);
			}
			textureIndex++;
		}
	}

	private void Update(){
		if(cameraDesiredLookkAt != null){
			cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, cameraDesiredLookkAt.rotation, Camera_Transition_Speed * Time.deltaTime);
		}
	}

	private void LoadLevel(string sceneName){
		SceneManager.LoadScene(sceneName);
	}

	public void LookAtMenu(Transform menuTransform){
		cameraDesiredLookkAt = menuTransform;
	}

	public void Close(){
		Application.Quit ();
	}

	private void ChangePlayerSkin(int index){
		if((GameManager.Instance.skinAvailability & 1 << index) == 1 << index){
			float x = (index % 4) * 0.25f;
			float y = ((int)index / 4) * 0.25f;

			if (y == 0.0f) {
				y = 0.75f;
			} else if (y == 0.25f) {
				y = 0.5f;
			} else if (y == 0.50f) {
				y = 0.25f;
			} else if (y == 0.75f) {
				y = 0f;
			}

			playerMaterial.SetTextureOffset ("_MainTex", new Vector2 (x, y));
			GameManager.Instance.currentSkinIndex = index;
			GameManager.Instance.Save();
		}
		else{
			// you do not have the skin, do you want to buy it ?
			int cost = costs[index];
			if(GameManager.Instance.currency >= cost){
				GameManager.Instance.currency -= cost;
				GameManager.Instance.skinAvailability += 1 << index;
				GameManager.Instance.Save();
				currencytext.text = "Currency : " + GameManager.Instance.currency.ToString();
				shopButtonContainer.transform.GetChild(index).GetChild(0).gameObject.SetActive(false);
				ChangePlayerSkin(index);
			}
		}
	}

}
