using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour {

	static public EffectManager instance;
	public GameObject effectPrefab;
	private int MAX_EFFECT = 20;
	private int now_index = 0;
	private List<GameObject> effects;

	void Awake() {
		instance = this;
	}
	void Start () {
		Vector3 initPos = new Vector3(0,0,0);
		now_index = 0;
		effects = new List<GameObject>();
		for(int i=0; i<MAX_EFFECT; ++i) {
			GameObject effect = Instantiate(effectPrefab, initPos, Quaternion.identity, this.transform);
			effects.Add(effect);
			effects[i].SetActive(false);
		}
	}

	public void EnableEffect(float x, float y, float z) {
		if(now_index>=MAX_EFFECT) {
			for(int i=0; i<MAX_EFFECT; ++i) {
				if(!effects[i].activeSelf) {
					now_index = i;
				}
			}
			if(now_index>=MAX_EFFECT) {
				return;
			}
		}
		effects[now_index].transform.position = new Vector3(x,y,z);
		effects[now_index].SetActive(true);
		StartCoroutine("HideEffect", now_index);
		now_index+=1;
	}

	IEnumerator HideEffect(int index) {
		yield return new WaitForSeconds(0.5f);
		effects[index].SetActive(false);
	}
}
