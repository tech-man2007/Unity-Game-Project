using UnityEngine;
using System.Collections;

public class Bombe : MonoBehaviour {

	public float nTimer = 3f ;
	public bool bExploseAnticipe = false ;
	public float nVitesseClignotte = 10f;
	private float nMomentActivation ;
	private bool bExplose = false ;
	private bool bActiveTime = true;
	private Material hBombeMat ;
	private int EmissionShader;
	private float nTimeCustom = 0 ;
	private Transform hBombeModel ;

	// Use this for initialization
	void Start () {
		nMomentActivation = Time.time;
		hBombeModel = transform.GetChild (0);
		hBombeMat = hBombeModel.GetComponent<Renderer>().material; // sharedMaterial se met à jour mais pas material.
		EmissionShader = Shader.PropertyToID ("_EmissionColor");

	}
	
	// Update is called once per frame
	void Update () {
		if (bActiveTime )
		{
			float nProgression = (Time.time - nMomentActivation) / nTimer ;
			if (bExploseAnticipe || nProgression >= 1)
			{
				bActiveTime = false;
				Explosion ();
			}
			else // timer en cours, pas encore explosé
			{
				nTimeCustom += Time.deltaTime *(0.8f+(nProgression*6.2f)) ;

				Color rouge = new Color(Mathf.PingPong(nTimeCustom,0.6f),0,0);
				hBombeMat.SetColor(EmissionShader, rouge );

				hBombeModel.localScale = Vector3.one * (1+Mathf.PingPong(nTimeCustom*0.3f, 0.1f)) ; // entre 1 et 1.1

			}
		}
		if (bExplose)
		{
			bExplose = false ;

			SphereCollider[] CollidsBomb = gameObject.GetComponents<SphereCollider> ();
			float nRadius = CollidsBomb[0].radius ;
			for (int i = 0; i < CollidsBomb.Length; i++)
			{
				if (CollidsBomb[i].radius > nRadius)
					nRadius = CollidsBomb[i].radius;
			}
			nRadius *=3;
			Collider[] tColliders = Physics.OverlapSphere (transform.position, nRadius);
			float nSize = transform.lossyScale.magnitude ;
			
			foreach (Collider hit in tColliders) 
			{
				
				if (hit.GetComponent<Rigidbody>())
				{

					hit.GetComponent<Rigidbody>().AddExplosionForce( (80 * nSize), transform.position, (nRadius * nSize), 0.1f*nSize);


				}
			}

		}

	}


	void Explosion()
	{
		Transform hBombe = transform.GetChild (0);
		hBombe.gameObject.SetActive (false);
		SphereCollider[] tSpherCol = GetComponents<SphereCollider> ();
		for (int i = 0; i < tSpherCol.Length; i++)
		{
			if (!tSpherCol[i].isTrigger)
				tSpherCol[i].enabled = false;
		}
		SphereCollider[] CollidsBomb = gameObject.GetComponents<SphereCollider> ();
		float nRadius = CollidsBomb[0].radius ;
		for (int i = 0; i < CollidsBomb.Length; i++)
		{
			if (CollidsBomb[i].radius > nRadius)
				nRadius = CollidsBomb[i].radius;
		}
		Collider[] others = Physics.OverlapSphere(transform.position, nRadius);
		InteractionColliders (others);
	}

	private void InteractionColliders(Collider[] others)
	{
		int nbContacts = others.Length;
		Collider other;
		for (int i = 0; i < nbContacts; i++)
		{
			other = others[i];

			if (other.gameObject.tag == "Explosif")
			{
				Bombe hBb = other.GetComponent<Bombe>() ;
				if (hBb != null)
					hBb.bExploseAnticipe = true ;
			}
			if (other.gameObject.tag == "Player")
			{
				Shattering hEclat = other.GetComponent<Shattering>();
				if (hEclat)
				{
					hEclat.Explode();
					// scene demo shattering, hide Unity-chan's mesh elements
					other.transform.GetChild(0).gameObject.SetActive(false);
					other.transform.GetChild(1).gameObject.SetActive(false);
					other.transform.GetChild(2).gameObject.SetActive(false);
				}
			}
		}
		bExplose = true ;
	}

	public void Suppression() // appel dans script Detonator quand le sfx se termine
	{
		Debug.Log ("Suppression de la bombe");
		Destroy (gameObject);
	}
}
