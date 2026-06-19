using UnityEngine;
using System.Collections;

public class DemonstrationShattering : MonoBehaviour {

	public GameObject[] tBombes ;
	public GameObject[] tObjectsToShatter ;


	// Use this for initialization
	void Start () {
		Animator anim;
		anim = tObjectsToShatter[0].GetComponent<Animator>() ;
		anim.SetInteger("State", 1);
		anim = tObjectsToShatter[1].GetComponent<Animator>() ;
		anim.SetInteger("State", 2);
		anim = tObjectsToShatter[2].GetComponent<Animator>() ;
		anim.SetInteger("State", 4);
		StartCoroutine (ApparitionBombes (7.5f));
	}

	IEnumerator ApparitionBombes(float nDelai)
	{
		float nStart = Time.time;
		while((Time.time - nStart) < nDelai )
		{
			yield return null;
		}
		for (int i = 0; i < tBombes.Length; i++)
		{
			tBombes[i].SetActive(true);
			tBombes[i].transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
		}
		nStart = Time.time;
		while((Time.time - nStart) < 2 )
		{
			yield return null;
		}
		for (int i = 0; i < tBombes.Length; i++)
		{
			Bombe bb = tBombes[i].GetComponent<Bombe>();
			tBombes[i].transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(true);
			bb.enabled = true ;
		}

		Animator anim = tObjectsToShatter[0].GetComponent<Animator>() ;
		anim.SetInteger("State", 3);
		anim = tObjectsToShatter[1].GetComponent<Animator>() ;
		anim.SetInteger("State", 3);
		anim = tObjectsToShatter[2].GetComponent<Animator>() ;
		anim.SetInteger("State", 3);
	}

}
