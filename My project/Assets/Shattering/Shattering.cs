using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shattering : MonoBehaviour {

	public GameObject[] Models ;
	public bool DoubleFace = true; // comme je rédéfini les triangles sans faire attention à la face visible par défaut je ne laisse pas ce choix pour l'instant de désactiver le double face
	[Range(1,100)]
	public int DensityPhysX = 10 ;
	[Range(0.5f,10f)]
	public float FadeOutDuration = 2 ;
	public AnimationCurve ScaleVariation = AnimationCurve.Linear(0,1,1,0); // 1 --> 0 , use at destruction
	public bool ApproximateTrianglesMoves = true;
	public bool UseToCreate = false ;
	public float CreationDuration = 3f;
	public AnimationCurve Creation_SpeedRotationVariation = AnimationCurve.EaseInOut(0,0,1,1); 	// 0 --> 1
	public AnimationCurve Creation_ScaleVariation = AnimationCurve.Linear(0,0,1,1); 			// 0 --> 1
	public AnimationCurve Creation_DistanceVariation = AnimationCurve.EaseInOut(0,1,1,0); 		// 1 --> 0
	public float Creation_DistanceStart = 2f;
	public GameObject MeshCenter ; // si initialisé il faudra l'utiliser comme centre autour duquel mener les opérations de scale rotation etc
	[Range(1,20)]
	public int nCreationComplexitéRotation = 4 ;

	private int MaxPhysXElements = 0 ;
	private tri2faces[] tris ;
	private List<tri2faces>[] trisSubMeshes ;
	private int nbSommets ;
	private List<GameObject> tPhysX_elts ;
	private GameObject hSupport ; 
	private bool bPhysX_Active = false ;
	private GameObject hModelPhysX ;
	private Material hInvisibleMat ;
	private Material[] tMatsOriginels ;
	private bool bRenduInvisible = false; // au départ il est visible
	private float nProgressionDisparition = 0;
	private float nProgressionApparition = 0 ;
	private bool[] tbSkinnedMesh ;
	private bool bCreationEnCours = false ;
	private bool bExplosionEnCours = false;
	private Vector3[] tCreation_MatricesRot ;


	// -----------------------------------------------------------

	public void CreationAnimationEnded()
	{

		
	}
	
	public void DestructionAnimationEnded()
	{
		//Destroy (hSupport);   		// to destroy the cloned and shattered object
		//Destroy (this.gameObject);	// to destroy the original object !! IF !! this script has been added to the original object
	}



	// -----------------------------------------------------------




	// Use this for initialization
	void Start () {
		Init ();
	}

	void Init()
	{
		// si tModels initialisé on fait les éclats pour chaque modèle sinon on le fait pour le mesh de l'objet sur lequel est attaché le script
		hModelPhysX = Resources.Load ("PhysXMesh") as GameObject;
		hInvisibleMat = Resources.Load ("Invisible") as Material;
		tPhysX_elts = new List<GameObject>();
		List<Mesh>  lMesh = new List<Mesh>();
		List<Transform> lTrans = new List<Transform>();
		List<Matrix4x4> lMatr = new	List<Matrix4x4> ();
		if (Models.Length == 0)
		{
			Models = new GameObject[1];
			Models[0] = gameObject;
		}
		
		tbSkinnedMesh = new bool[Models.Length];
		for (int i = 0; i < Models.Length; i++)
			tbSkinnedMesh[i] = false;
			
		int nIndiceItem = 0;
		if (Models.Length > 0)
		{
			foreach (GameObject item in Models) {
				if (item)
				{
//					Debug.Log ("Object "+item.name);

					MeshFilter hFilt = item.GetComponent<MeshFilter>();
					SkinnedMeshRenderer hSkinnedMesh = item.GetComponent<SkinnedMeshRenderer>();
					Mesh hMesh = new Mesh();
					if (hFilt)
						hMesh = hFilt.mesh ;
					else if (hSkinnedMesh)
					{
						hSkinnedMesh.BakeMesh(hMesh);
						tbSkinnedMesh[nIndiceItem] = true;
					}
					Matrix4x4 mat = new Matrix4x4 (); 
					if (tbSkinnedMesh[nIndiceItem])
						mat.SetTRS (item.transform.localPosition, item.transform.localRotation, Vector3.one);
					else
					{
						Vector3 vPositionTemp = item.transform.localPosition ;
						if (item.transform.parent)
						{
							vPositionTemp.x *= item.transform.parent.localScale.x ;
							vPositionTemp.y *= item.transform.parent.localScale.y ;
							vPositionTemp.z *= item.transform.parent.localScale.z ;
						}
						mat.SetTRS (vPositionTemp, item.transform.localRotation, item.transform.lossyScale);

					}
					if (hMesh)
					{
						lMesh.Add(hMesh);
						lTrans.Add(item.transform);
						lMatr.Add(mat);
					}
					nIndiceItem++ ;
				}
			}
			MaxPhysXElements = lMesh.Count*2*DensityPhysX ;
			if (lMesh.Count>0)
				CreateEclats(lMesh.ToArray(), lTrans.ToArray(), lMatr.ToArray() );
		}

		if (UseToCreate)
		{
			if (MeshCenter == null)
				MeshCenter = gameObject;
			bRenduInvisible = false;
			ShowOriginal(false);
			bCreationEnCours = true ;
			tCreation_MatricesRot = new	Vector3[nCreationComplexitéRotation] ;
			StartCoroutine(Apparition(CreationDuration));
			for (int i = 0; i < nCreationComplexitéRotation; i++) {
				tCreation_MatricesRot[i] = new Vector3(Random.Range(-1600,1600),Random.Range(-1600,1600),Random.Range(-1600,1600)) ;
			}

		}
	}

	// Update is called once per frame
	void Update () {
		
		

		if (bCreationEnCours)
		{
			hSupport.transform.position = transform.position;
			hSupport.transform.rotation = transform.rotation;
			UpdateT2fApparition();
			UpdateMesh_fromT2f();
		}

		if (bExplosionEnCours)
		if (hSupport.activeSelf)
		{
			UpdatePhysXElts (); // et fais majMeshToTris
			UpdateMesh_fromT2f ();
		}








	}


	public void Explode()
	{
		Explode (0);
		ShowOriginal (false); 
	}
	
	public void Explode(int nType)
	{
		bExplosionEnCours = true;
		hSupport.transform.position = transform.position;
		hSupport.transform.rotation = transform.rotation;
		UpdatePhysXElts (); // et fais majMeshToTris
		UpdateMesh_fromT2f ();
		hSupport.SetActive (true);
		StartCoroutine (Disparition (FadeOutDuration));
		// default case 0, gravity
		if (nType == 0)
		{
			bPhysX_Active = true;
			for (int i = 0; i < tPhysX_elts.Count; i++)
			{
				tPhysX_elts[i].SetActive(true);
			}
		}
		
	}
	
	private void ResetExplosion()
	{
		bExplosionEnCours = false;
		hSupport.transform.position = transform.position;
		hSupport.transform.rotation = transform.rotation;
		bPhysX_Active = false;
		for (int i = 0; i < tPhysX_elts.Count; i++)
		{
			tPhysX_elts[i].SetActive(false);
		}
		int nItem = 0;
		foreach (GameObject item in Models)
		{	if (item)
			{
				int nbTris = trisSubMeshes[nItem].Count ;
				for (int i = 0; i < nbTris; i++)
				{
					trisSubMeshes [nItem] [i].SetScale (Vector3.one);
				}
				nItem++;
			}
		}
		hSupport.SetActive (false);
		UpdatePhysXElts ();
		UpdateMesh_fromT2f ();
	}
	
	void ShowOriginal(bool visible)
	{
		int nbModels = Models.Length;
		int nbMats = 0; //au moins égal à nbmodels à moins de ne pas avoir de hrend ou hskinned sur un model ou pas de material associé.

		if (Models.Length > 0)
		{
			foreach (GameObject item in Models)
			{
				if (item)
				{
					MeshRenderer hRend = item.GetComponent<MeshRenderer>();
					SkinnedMeshRenderer hSkinnedRend = item.GetComponent<SkinnedMeshRenderer>();

					if (hRend)
						nbMats += hRend.materials.Length;
					else if (hSkinnedRend)
						nbMats += hSkinnedRend.materials.Length ;
				}
			}
		
			if (!visible && !bRenduInvisible)
			{
				tMatsOriginels = new Material[nbMats]; // attention un double appel à l'invisibilité sans le rendre visible avant efface le tableau 
				bRenduInvisible = true;
			}
			if (visible) // pour rendre visible
				bRenduInvisible = false ; // à mettre à la fin normalement

			int indice = 0 ;
			int nbMatstmp = 0;
			Material[] tmpMats;
			foreach (GameObject item in Models) {
				if (item)
				{
					MeshRenderer hRend = item.GetComponent<MeshRenderer>();
					SkinnedMeshRenderer hSkinnedRend = item.GetComponent<SkinnedMeshRenderer>();

					if (hRend)
					{
						nbMatstmp = hRend.materials.Length;
						tmpMats = hRend.materials;
						for (int i = 0; i < nbMatstmp; i++)
						{
							if (!visible) {
								tMatsOriginels [indice] = hRend.materials[i];
								tmpMats[i] = hInvisibleMat;
							} else
								tmpMats[i] = tMatsOriginels [indice];
							indice++;
						}
						hRend.materials = tmpMats;
					}
					else if (hSkinnedRend)
					{
						nbMatstmp = hSkinnedRend.materials.Length;
						tmpMats = hSkinnedRend.materials;
						for (int i = 0; i < nbMatstmp; i++)
						{
							if (!visible) {
								tMatsOriginels [indice] = hSkinnedRend.materials[i];
								tmpMats[i] = hInvisibleMat;
							} else
								tmpMats[i] = tMatsOriginels [indice];
							indice++;
						}
						hSkinnedRend.materials = tmpMats;
					}

				}
			}
		}
	}
	
	IEnumerator Disparition(float nDelai)
	{
		float nDebut = Time.time;
		while (Time.time-nDebut < nDelai)
		{
			nProgressionDisparition = Mathf.Clamp01( (Time.time-nDebut) / nDelai);
			yield return null;
		}
		nProgressionDisparition = 1;
		//remet en place les physX sur le corps et désativés
		ResetExplosion ();
		DestructionAnimationEnded ();
	}

	IEnumerator Apparition(float nDelai)
	{
		float nDebut = Time.time;
		while (Time.time-nDebut < nDelai)
		{
			nProgressionApparition = Mathf.Clamp01( (Time.time-nDebut) / nDelai);
			yield return null;
		}
		nProgressionApparition = 1;
		bCreationEnCours = false; 
		hSupport.SetActive (false);
		//réactiver le renderer de chaque tmodel
		ShowOriginal (true);
		//fonction vide appelée, peut servir pour l'utilisateur final
		CreationAnimationEnded ();
	}







	void CreateEclats(Mesh[] tMesh, Transform[] tTransf, Matrix4x4[] tMatr) // autant de submeshes que de models
	{
		Debug.Log ("Shattering Init Start");
		tri2faces tri;
		nbSommets = 0;
		int[] nSommets3 = new int[3];
		int nbTrisOrig = 0;
		int nbMeshes = tMesh.Length;
		trisSubMeshes = new List<tri2faces>[nbMeshes];
		for (int i = 0; i < nbMeshes; i++)
		{
			nbTrisOrig += (tMesh[i].triangles.Length/3) ;
		}
		int nbEltsPhysx = Mathf.Min (nbTrisOrig / 8, MaxPhysXElements); // pour l'ensemble des meshs
		List<int> tTrisWithPhysX = new List<int>(nbEltsPhysx); 
		FillRandomList (tTrisWithPhysX, 0, nbTrisOrig, nbEltsPhysx);

		Mesh hMesh;
		int nIndiceTri = 0;

		hSupport = new GameObject ("Support_"+gameObject.name);
		hSupport.transform.SetParent (transform, false);
		hSupport.transform.parent = null;
		hSupport.transform.localScale = Vector3.one;
		// rotation à 0 du parent afin d'appliquer correctement les normales, quelle que soit la rotation du Support
		Quaternion RotationOriginale = transform.rotation;
		transform.rotation = Quaternion.identity;

		int nbPhysXParSubMesh = 0;

		for (int nMesh = 0; nMesh < nbMeshes; nMesh++)
		{
			trisSubMeshes[nMesh] = new List<tri2faces>() ;
			hMesh = tMesh[nMesh];
			int[] triangles = hMesh.triangles;
			Vector3[] vertices = hMesh.vertices ;
			Vector2[] uv = hMesh.uv ;
			Vector3[] norms = hMesh.normals ;
			Vector4[] tangs = hMesh.tangents ;
			int nbNorms = norms.Length ;
			nbNorms = Mathf.Min(nbNorms, tangs.Length);
			int nbTrisThisMesh = triangles.Length/3 ;

			nbPhysXParSubMesh = 0;
			for (int i = 0; i < nbTrisThisMesh; i++)
			{
				tri = new tri2faces();
				// intermédiaires pas utiles, juste plus certain en cas de bug pour détecter l'origine du problème.
				nSommets3[0] = triangles[i*3+0] ;
				nSommets3[1] = triangles[i*3+1] ;
				nSommets3[2] = triangles[i*3+2] ;
				tri.SetVertices( new Vector3[]{tMatr[nMesh].MultiplyPoint3x4(vertices[nSommets3[0]]), tMatr[nMesh].MultiplyPoint3x4(vertices[nSommets3[1]]), tMatr[nMesh].MultiplyPoint3x4(vertices[nSommets3[2]])} );
				tri.SetUV( new Vector2[]{uv[nSommets3[0]], uv[nSommets3[1]], uv[nSommets3[2]]} );
				// avec uv et normal adéquats
				if (nSommets3[0] < nbNorms && nSommets3[1] < nbNorms && nSommets3[2] < nbNorms )
				{
					//apparemment la lecture des normals/direction dépend de la position de l'objet au moment de sa lecture
					tri.SetNormals( new Vector3[]{tTransf[nMesh].TransformDirection(norms[nSommets3[0]]), tTransf[nMesh].TransformDirection(norms[nSommets3[1]]), tTransf[nMesh].TransformDirection(norms[nSommets3[2]])} ) ;
					tri.SetTangents(new Vector4[]{tangs[nSommets3[0]], tangs[nSommets3[1]], tangs[nSommets3[2]]}) ;
				}
				tri.ComputeTris(nIndiceTri, DoubleFace); //meme en submeshes les sommets ne sont pas en submeshes, seuls les triangles donc je n'ai pas plusieurs sommets 0 1 etc
				tri.nGroupeRotation = Random.Range(0,nCreationComplexitéRotation);

				trisSubMeshes[nMesh].Add(tri) ;

				if (tTrisWithPhysX.Contains(nIndiceTri)) // je fais un elt PhysX / indice des tris total de tout les meshs
				{
					AddPhysXElt(tri, nMesh);
					nbPhysXParSubMesh++;
				}

				nIndiceTri++;

			}
			nbSommets += vertices.Length ;
		}


		transform.rotation = RotationOriginale;
		hSupport.AddComponent<MeshFilter> ();
		MeshFilter hMf = hSupport.GetComponent<MeshFilter> ();
		hSupport.AddComponent<MeshRenderer> ();
		MeshRenderer hMr = hSupport.GetComponent<MeshRenderer> ();
		hMr.materials = new Material[nbMeshes] ; // submeshes
		hMesh = new Mesh ();
		hMesh.name = "MeshEclatable";
		hMesh.MarkDynamic ();
		UpdatePhysXElts ();
		AttribueMesh (ref hMesh); // et uv
		hMf.mesh = hMesh;
		ApplyMaterial (hMr);
		AttribuePhysXElts_T2f ();
		hSupport.SetActive (false);

	}

	void ApplyMaterial(MeshRenderer hMr)
	{
		int nbMats = hMr.materials.Length;
		Material[] mats = hMr.materials;
		int nbCount=0;
		if (Models.Length > 0)
		{
			foreach (GameObject item in Models) {
				if (item)
				{
					MeshRenderer hMrIt = item.GetComponent<MeshRenderer>();
					SkinnedMeshRenderer hSkinnedMesh = item.GetComponent<SkinnedMeshRenderer>();
					if (hMrIt)
					{
						if (nbCount< nbMats)
							mats[nbCount] = hMrIt.material ;
					}else if (hSkinnedMesh)
					{
						if (nbCount< nbMats)
							mats[nbCount] = hSkinnedMesh.material ;
					}
					nbCount++ ;
				}
			}
		}
		hMr.materials = mats;

	}







	void AttribueMesh (ref Mesh hMesh)
	{
		int nbTrisType;
		Vector3[] vertices;
		Vector3[] normals ;
		Vector4[] tangents ;
		Vector2[] uvs;
		int nbVertices = 0; 
		int nTailleTris;
		int nbVertTri = 3;
		if (DoubleFace)
			nbVertTri *= 2 ;
		int[] triangles;
		int[] tmpTris;
		Vector3[] sommets ;
		Vector3[] norms ;
		Vector4[] tangs ;
		Vector2[] uvLoc;
		int nSubCount = trisSubMeshes.Length;
		hMesh.subMeshCount = nSubCount;
		for (int nSubset = 0; nSubset < nSubCount; nSubset++)
			nbVertices += trisSubMeshes[nSubset].Count*3 ; // au total, les vertices sont indépendants des subsets
		int nIndiceTriGlobal = 0;
		// fin init tabs pour sommets et triangles
		vertices = new Vector3[nbVertices] ;
		normals = new Vector3[nbVertices] ;
		tangents = new Vector4[nbVertices] ;
		uvs = new Vector2[nbVertices] ;

		for (int nSubset = 0; nSubset < nSubCount; nSubset++)
		{
			nbTrisType = trisSubMeshes[nSubset].Count;
			nTailleTris = nbTrisType*3;
//			vertices = new Vector3[nTailleTris] ; // puisqu'exactement 3 sommets uniques par tri2face
			if (DoubleFace)
				nTailleTris *= 2;
			triangles = new int[nTailleTris];

			for (int i = 0; i < nbTrisType; i++)
			{
				sommets = trisSubMeshes[nSubset][i].sommets ;
				norms = trisSubMeshes[nSubset][i].norms ;
				tangs = trisSubMeshes[nSubset][i].tangents ;
				tmpTris = trisSubMeshes[nSubset][i].tris ;
				uvLoc = trisSubMeshes[nSubset][i].uvs ;
				for (int j = 0; j < 3; j++) {
					vertices[nIndiceTriGlobal*3+j] = sommets[j] ; // on a toujours 3 nouveaux sommets sur chaque tri2face
					uvs[nIndiceTriGlobal*3+j] = uvLoc[j];
					triangles[i*nbVertTri+j] = tmpTris[j] ;
					normals[nIndiceTriGlobal*3+j] = norms[j];
					tangents[nIndiceTriGlobal*3+j] = tangs[j];
				}
				for (int j = 3; j < nbVertTri; j++) {
					triangles[i*nbVertTri+j] = tmpTris[j] ;
				}
				nIndiceTriGlobal++ ;
			}
			hMesh.vertices = vertices ; // il ne sera plein qu'après tout avoir lu donc je ne devrais le faire qu'une fois à la fin mais c'est pour éviter les out of array avec settriangles avant sommets
			hMesh.SetTriangles(triangles, nSubset);
		}
		hMesh.uv = uvs;
		hMesh.normals = normals;
		hMesh.tangents = tangents;
		;
		hMesh.RecalculateBounds();



		
	}



	//à ne faire dans l'update que si on a un skinned mesh renderer pusiqu'autrement je n'ai pas de changement de mesh.
	void UpdateMesh_fromT2f()
	{
		Matrix4x4 matCalc = Matrix4x4.identity;
		Vector3 vtmp = new Vector3();
		if (hSupport)
		{
			MeshFilter hMf = hSupport.GetComponent<MeshFilter>();
			if (hMf)
			{
				Mesh hMesh = hMf.mesh ;
				int nIndiceTriGlobal = 0 ;
				if (hMesh)
				{
					Vector3[] vertices = hMesh.vertices; // je peux aussi en créer un vide de meme taille, peu importe vu que j'écrase
					int nSubCount = hMesh.subMeshCount ;
					for (int nSubset = 0; nSubset < nSubCount; nSubset++)
					{
						int nbTrisType = trisSubMeshes[nSubset].Count;

						for (int i = 0; i < nbTrisType; i++)
						{
							//autre version, pareil que getvertice mais pas d'appel de fction, plus rapide
							if (i == 0) { // version de base, tous les tris ont la même échelle en même temps pour un subset donné voir objet donné
								vtmp = trisSubMeshes [nSubset] [i].vScale;
								matCalc [0, 0] = vtmp.x;
								matCalc [1, 1] = vtmp.y;
								matCalc [2, 2] = vtmp.z;
							}
							vtmp= trisSubMeshes[nSubset][i].vCentre ;
							for (int b = 0; b < 3; b++)
								vertices[nIndiceTriGlobal*3+b] = matCalc.MultiplyPoint3x4(trisSubMeshes[nSubset][i].sommets[b]-vtmp) + vtmp;


							nIndiceTriGlobal++ ;
						}
					}
					hMesh.vertices = vertices;
					;
					hMesh.RecalculateBounds();
				}
			}
		}
	}

	void AddPhysXElt(tri2faces hTriangle, int nMesh)
	{
		hTriangle.bRacinePhysX = true;
		hTriangle.nReperePhysX_A = tPhysX_elts.Count;
		GameObject hPhysX = GameObject.Instantiate (hModelPhysX); // charge importante CPU si on crée tout lors d'une frame (possible de "proposer" la création du hphysx étalée sur plusieurs frames, en échange on ne doit pas avoir d'intéraction dessus tant que tout n'est pas chargé)
		hPhysX.transform.SetParent (hSupport.transform);
		hPhysX.transform.position = hTriangle.vCentre + hSupport.transform.position;
		tPhysX_elts.Add (hPhysX);
	}

	void AttribuePhysXElts_T2f()
	{
		// on cherche pour chaque triangle les 2 physX les plus proches et on les lui lie // to improve with more physX referenced
		int nbTris, nbMeshes;
		int nbPhysX = tPhysX_elts.Count;
		Vector3 vPhysXelt;
		int nMin1, nMin2;
		float nDist1, nDist2, nTmpDist;
		float nClampMin, nClampMax ;
		nbMeshes = trisSubMeshes.Length;

		nClampMax = 1.5f ;
		nClampMin = -nClampMax;
		//debug
		for (int k = 0; k < nbPhysX; k++)
			vPhysXelt = tPhysX_elts [k].transform.position;
		
		if (nbPhysX>1) // au moins 2
		for (int i = 0; i < nbMeshes; i++)
		{
			nbTris = trisSubMeshes[i].Count;
			for (int j = 0; j < nbTris; j++) {
				if (! trisSubMeshes[i][j].bRacinePhysX)
				{
					Vector3 vPt = trisSubMeshes[i][j].vCentre + hSupport.transform.position ;

					nDist1 = Vector3.Distance( tPhysX_elts[0].transform.position, vPt) ;
					nDist2 = Vector3.Distance( tPhysX_elts[1].transform.position, vPt) ;
					nMin1 = 0 ;
					nMin2 = 1 ;
					for (int nK = 0; nK < nbPhysX; nK++) { // on regarde chaque physX pour déterminer les 2 plus proches du triangle en cours
						vPhysXelt = tPhysX_elts[nK].transform.position;
						nTmpDist = Vector3.Distance(vPhysXelt,vPt);
						if (nTmpDist< nDist1)
						{
							nDist2 = nDist1;
							nMin2 = nMin1 ;
							nDist1 = nTmpDist;
							nMin1 = nK ;
						}
						else if (nTmpDist < nDist2)
						{
							nDist2 = nTmpDist;
							nMin2 = nK;
						}

					}
					Vector3 vA = tPhysX_elts[nMin1].transform.position ;
					Vector3 vB = tPhysX_elts[nMin2].transform.position ;

					if (vA == vB)
					{
						trisSubMeshes[i][j].nReperePhysX_A = nMin1 ;
						trisSubMeshes[i][j].nReperePhysX_B = nMin1 ;
						trisSubMeshes[i][j].bRacinePhysX = true ;
					}
					else
					{
						// ceci n'est pas la meilleure méthode, il faudrait déterminer le splus proches à la fois par distance et par nbr de tris min les séparant en suivant le mesh
						// ensuite faire la moyenne des 2 classements pour déterminer les 2 plus proches à la fois en distance et selon le mesh., par ex le long des bras je peux avoir un tri de la cuissse très proche donc lié par exemple.
						Vector3 distRef = vB-vA ;
						Vector3 dAP = vPt - vA ;
						Vector3 Offset = new Vector3(Mathf.Clamp(dAP.x/distRef.x,nClampMin,nClampMax), Mathf.Clamp(dAP.y/distRef.y,nClampMin,nClampMax), Mathf.Clamp(dAP.z/distRef.z,nClampMin,nClampMax));
						trisSubMeshes[i][j].nReperePhysX_A = nMin1 ;
						trisSubMeshes[i][j].nReperePhysX_B = nMin2 ;
						trisSubMeshes[i][j].vOffsetPhysX = Offset ;
					}
				}
			}
		}

		//other method, amélioration en calculalnt la distance en suivant le mesh. Dijkstra
		// je fais un chemin entre chaque tris proches, je pourrais noter une distance de 1 pour simplifier à chaque fois, mais j'aurai une meilleure précision en notant la distance séparant les centres.
		// par contre algo irrégulier pour les mesh non liés ou tris non accolés et si je prend uniquement la distance en compte j'ai le cas par exemple de la main proche de la jambe ou tete ou etc...



	}

	void UpdateT2fApparition()
	{
		int nCptVert = 0;
		int iPhysx = 0;
		int nItemEnCours = 0;
		tri2faces T2fa = new tri2faces ();
		if (!hSupport.activeSelf && nProgressionApparition<1)
			hSupport.SetActive (true);

		if (Models.Length > 0)
		{
			foreach (GameObject item in Models) {
				if (item)
				{
					MeshFilter hFilt = item.GetComponent<MeshFilter>();
					SkinnedMeshRenderer hSkinnedMesh = item.GetComponent<SkinnedMeshRenderer>();
					Matrix4x4 mat = new Matrix4x4 ();

					if (tbSkinnedMesh[nItemEnCours])
					{
						mat = Matrix4x4.TRS(item.transform.position, item.transform.rotation, Vector3.one);
						if (hSkinnedMesh)
							if (hSkinnedMesh.sharedMesh.blendShapeCount >0)
								mat = mat*Matrix4x4.Scale(item.transform.lossyScale);
						mat = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse * mat ;
					}
					else
					{
						mat = item.transform.localToWorldMatrix ;
						mat = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse * mat ;
					}

					// application des tris selon le mesh original
					if (hFilt)
					{
						Mesh hMesh = hFilt.mesh ;
						if (hMesh)
						{
							iPhysx = majMeshToTris(hMesh, mat, nCptVert, hMesh.triangles.Length/3, iPhysx, nItemEnCours);
							nCptVert+= hMesh.triangles.Length/3;
						}
					}else if (hSkinnedMesh)
					{
						Mesh hMesh = new Mesh();
						hSkinnedMesh.BakeMesh(hMesh); // le bake ne prend pas de temps, juste comme ça j'ai <1ms
						if (hMesh)
						{
							iPhysx = majMeshToTris(hMesh, mat, nCptVert, hMesh.triangles.Length/3, iPhysx, nItemEnCours);
							nCptVert+= hMesh.triangles.Length/3;
						}
					}

					//maintenant on écarte les tris selon nProgressionApparition entre 0et1 et sachant qu'ils sont en position initiale sur le mesh source
					//il faudrait plusieurs "groupes" de tris, choisis aléatoirement, chacun avec sa matrice de rotation, on aura la meme position finale, mais des initiales différentes
					//variable int nGroupe pour chaque tri, on lit et choisit la matrice conséquente.




					float nProgRot = Creation_SpeedRotationVariation.Evaluate(nProgressionApparition);
					float nProgDist = Creation_DistanceVariation.Evaluate(nProgressionApparition) ; // 1 vers 0
					float nScaleIndiv = Creation_ScaleVariation.Evaluate(nProgressionApparition);
					float nDistanceSource = Creation_DistanceStart * nProgDist ;
					Vector3 vScaleGlob = Vector3.one*(1+nDistanceSource) ;
					Vector3 vOffSetCentre = MeshCenter.transform.position-transform.position ;
					Quaternion qRot = Quaternion.identity;
					Matrix4x4[] tMatr = new Matrix4x4[nCreationComplexitéRotation] ;
					for (int nMatr = 0; nMatr < nCreationComplexitéRotation; nMatr++) {
						qRot.eulerAngles = Vector3.zero + tCreation_MatricesRot[nMatr]* (1-nProgRot);
						tMatr[nMatr] = Matrix4x4.TRS(vOffSetCentre, qRot, vScaleGlob );
						tMatr[nMatr] = tMatr[nMatr] * Matrix4x4.TRS(-vOffSetCentre,Quaternion.identity,Vector3.one ) ; 
					}


					int nbTris = trisSubMeshes[nItemEnCours].Count ;
					int nGroupe ;
					for (int i = 0; i < nbTris; i++)
					{
						T2fa = trisSubMeshes [nItemEnCours] [i];
						nGroupe = T2fa.nGroupeRotation ;
						T2fa.MoveTo( tMatr[nGroupe].MultiplyPoint3x4(T2fa.vCentre));
						T2fa.SetScale(Vector3.one* nScaleIndiv);
						trisSubMeshes [nItemEnCours] [i] = T2fa;
					}
					
					nItemEnCours++;
				}
			}
			
		}
	}

	void UpdatePhysXElts()
	{
		int nCptVert = 0;
		int iPhysx = 0;
		int nItemEnCours = 0;
		if (Models.Length > 0)
		{
			foreach (GameObject item in Models) {
				if (item)
				{
					MeshFilter hFilt = item.GetComponent<MeshFilter>();
					SkinnedMeshRenderer hSkinnedMesh = item.GetComponent<SkinnedMeshRenderer>();
					Matrix4x4 mat = new Matrix4x4 ();
					if (tbSkinnedMesh[nItemEnCours])
					{
						mat = Matrix4x4.TRS(item.transform.position, item.transform.rotation, Vector3.one);
						if (hSkinnedMesh)
							if (hSkinnedMesh.sharedMesh.blendShapeCount >0)
								mat = mat*Matrix4x4.Scale(item.transform.lossyScale);
						mat = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse * mat ;
						
					}
					else
					{
						
						mat = item.transform.localToWorldMatrix ;
						mat = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse * mat ;
						
					}

					if (! bPhysX_Active)
					{

						if (hFilt) // (!tbSkinnedMesh[nItemEnCours])
						{
							Mesh hMesh = hFilt.mesh ;
							if (hMesh)
							{
								iPhysx = majMeshToTris(hMesh, mat, nCptVert, hMesh.triangles.Length/3, iPhysx, nItemEnCours);
								nCptVert+= hMesh.triangles.Length/3;
							}
						}else if (hSkinnedMesh)
						{
							Mesh hMesh = new Mesh();
							hSkinnedMesh.BakeMesh(hMesh); // le bake ne prend pas de temps, juste comme ça j'ai <1ms
							if (hMesh)
							{
								iPhysx = majMeshToTris(hMesh, mat, nCptVert, hMesh.triangles.Length/3, iPhysx, nItemEnCours);
								nCptVert+= hMesh.triangles.Length/3;
							}
						}

					
					}// à faire seulement une fois les physX activés et donc le suivi des physx le long du mesh désactivé
					else //(bPhysX_Active)
					{
						Matrix4x4 MatSup = new Matrix4x4();
						int nbPhysX = tPhysX_elts.Count;
						Vector3[] tPhysXNewPos = new Vector3[nbPhysX] ;
						MatSup.SetTRS (hSupport.transform.position, hSupport.transform.rotation, Vector3.one);
						MatSup = MatSup.inverse ;
						float nScale = ScaleVariation.Evaluate(nProgressionDisparition);
						Vector3 vScale = Vector3.one*nScale;
						int nbTris = trisSubMeshes[nItemEnCours].Count ;
						for (int i = 0; i < nbPhysX; i++) {
							tPhysXNewPos[i] = MatSup.MultiplyPoint3x4(tPhysX_elts[i].transform.position) ;
						}

						if (ApproximateTrianglesMoves)
						{
							for (int i = 0; i < nbTris; i++)
							{
								trisSubMeshes[nItemEnCours][i].SetScale(vScale);
								if (trisSubMeshes[nItemEnCours][i].bRacinePhysX )
								{	if ( trisSubMeshes[nItemEnCours][i].nReperePhysX_A < tPhysX_elts.Count )
										trisSubMeshes[nItemEnCours][i].MoveTo( tPhysXNewPos[ trisSubMeshes[nItemEnCours][i].nReperePhysX_A ] );
								}else
								{
									//méthode A très imparfaite, avec conservation de la distance relative de départ vis à vis d'un A et B pour chaque tri.
									Vector3 vA= tPhysXNewPos[trisSubMeshes[nItemEnCours][i].nReperePhysX_A];
									Vector3 vB= tPhysXNewPos[trisSubMeshes[nItemEnCours][i].nReperePhysX_B];
									Vector3 vPt, vOffSet, dRefMod;
									dRefMod = vB-vA ;
									vOffSet = trisSubMeshes[nItemEnCours][i].vOffsetPhysX;
									vOffSet.y = nProgressionDisparition*0.5f + (vOffSet.y*(1-nProgressionDisparition)); // doit etre à 0.5 lorsque nProgressionDisparition arrive à 1, départ à 0
									vPt.x = vA.x + vOffSet.x * dRefMod.x ;
									vPt.y = vA.y + vOffSet.y * dRefMod.y ;
									vPt.z = vA.z + vOffSet.z * dRefMod.z ;
									trisSubMeshes[nItemEnCours][i].MoveTo( vPt );
								}
							}
						}
						else // pas approximation déplacement, on met donc direct tri sur physX
						{
							for (int i = 0; i < nbTris; i++)
							{
								trisSubMeshes[nItemEnCours][i].SetScale(vScale);
								if ( trisSubMeshes[nItemEnCours][i].nReperePhysX_A < tPhysX_elts.Count )
									trisSubMeshes[nItemEnCours][i].MoveTo( tPhysXNewPos[ trisSubMeshes[nItemEnCours][i].nReperePhysX_A ] );
							}
						}
					}

					nItemEnCours++;
				}
			}

		}


	}






	int majMeshToTris(Mesh hMesh, Matrix4x4 mat ,int nDebut, int nCount, int iPhysx, int nMesh)
	{
		
		Vector3[] vertArrayCopy = hMesh.vertices;
		tri2faces T2fa = new tri2faces();
		int[] triangles = hMesh.triangles;
		Matrix4x4 MatSup = new Matrix4x4 ();
		MatSup.SetTRS (hSupport.transform.position, hSupport.transform.rotation, Vector3.one);
		for (int i = 0; i < nCount; i++)
		{
			//màj de la position pour chaque tri.
			T2fa = trisSubMeshes[nMesh][i] ; // original lu après pour test sur bRacinePhysx
			T2fa.sommets[0] = mat.MultiplyPoint3x4(vertArrayCopy[triangles[i*3+0]]) ;
			T2fa.sommets[1] = mat.MultiplyPoint3x4(vertArrayCopy[triangles[i*3+1]]) ;
			T2fa.sommets[2] = mat.MultiplyPoint3x4(vertArrayCopy[triangles[i*3+2]]) ;
			T2fa.vCentre = (T2fa.sommets[0] + T2fa.sommets[1] + T2fa.sommets[2])/3;
			// màj de la position de ceux voulu dans le tableau
			if (!bPhysX_Active)
			{
				if (T2fa.bRacinePhysX && iPhysx < tPhysX_elts.Count)
				{
					tPhysX_elts[iPhysx].transform.position = MatSup.MultiplyPoint3x4(T2fa.vCentre);
					iPhysx++;
				}
			}
			trisSubMeshes [nMesh] [i] = T2fa;
		}
		return iPhysx;
	}

	void FillRandomList(List<int> tListe, int nMin, int nMaxExc, int nQuantite)
	{
		if (nQuantite > nMaxExc - nMin)
			Debug.LogError ("FillRandomListe a besoin de trop d'éléments.");
		else
		{
			int i = 0;
			int nCptLim = 0;
			int nVal;
			while (i< nQuantite)
			{
				do
				{
					nVal = Random.Range (nMin, nMaxExc);
					nCptLim++;
				} while (tListe.Contains(nVal) && nCptLim < 100000);
				if (nCptLim<100000)
					tListe.Add(nVal);
				i++;
			}
		}
	}

	public AnimationCurve scaleVariation {
		get {
			return ScaleVariation;
		}
		set {
			ScaleVariation = value;
		}
	}
}

public class tri2faces 
{
	public Vector3[] sommets ;
	public Vector4[] tangents ;
	public Vector3[] norms ;
	public Vector2[] uvs ;
	public Vector3 vCentre ;
	public int[] tris ;
	public bool bRacinePhysX = false;
	public int nReperePhysX_A = -1 ; // indice dans le tableau de tphysx
	public int nReperePhysX_B = -1 ;
	public Vector3 vOffsetPhysX = Vector3.zero;
	public Vector3 vScale = Vector3.one ;
	public int nGroupeRotation = 0; // numéro de matrice de creation pour la rotation aléatoire
	private Matrix4x4 matCalcul ;

	public tri2faces()
	{
		matCalcul = new	Matrix4x4();
	}

	public void AddVertex(Vector3 v3)
	{

	}

	public void SetVertices(Vector3[] v3s)
	{
		if (v3s.Length != 3)
			Debug.LogError ("Attribution de " + v3s.Length + " sommets pour ce tri au lieu de 3");
		else
		{
			sommets = v3s;
			//on calcule le barycentre
			vCentre = Vector3.zero;
			for (int i = 0; i < 3; i++)
			{
				vCentre += v3s[i];
			}
			vCentre /= 3;
		}
	}

	public void SetVertices(Vector3[] v3s, Vector2[] uvs_in)
	{
		if (v3s.Length != 3)
			Debug.LogError ("Attribution de " + v3s.Length + " sommets pour ce tri au lieu de 3");
		else
		{
			sommets = v3s;
			uvs = uvs_in ;
			//on calcule le barycentre
			vCentre = Vector3.zero;
			for (int i = 0; i < 3; i++)
			{
				vCentre += v3s[i];
			}
			vCentre /= 3;
		}
	}

	public void SetVertices(Vector3 v1, Vector3 v2, Vector3 v3)
	{
		sommets = new Vector3[3];
		sommets[0] = v1;
		sommets[1] = v2;
		sommets[2] = v3;
		//on calcule le barycentre
		vCentre = Vector3.zero;
		for (int i = 0; i < 3; i++)
		{
			vCentre += sommets[i];
		}
		vCentre /= 3;
	}

	public void SetVertices(Vector3 v1, Vector3 v2, Vector3 v3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
	{
		sommets = new Vector3[3];
		sommets[0] = v1;
		sommets[1] = v2;
		sommets[2] = v3;
		SetUV (uv1, uv2, uv3);
		//on calcule le barycentre
		vCentre = Vector3.zero;
		for (int i = 0; i < 3; i++)
		{
			vCentre += sommets[i];
		}
		vCentre /= 3;
	}

	public Vector3[] GetVertices() // avec application de scale et rotation.
	{
//		Matrix4x4 mat = new Matrix4x4 ();
		matCalcul.SetTRS (Vector3.zero, Quaternion.identity, vScale);
		Vector3[] sommetsMods = new	Vector3[3];
		for (int i = 0; i < 3; i++)
			sommetsMods[i] = matCalcul.MultiplyPoint3x4(sommets[i]-vCentre) + vCentre;
		return sommetsMods;
	}

	public void SetUV(Vector2[] uvs_in)
	{
		uvs = uvs_in;
	}

	public void SetUV(Vector2 v1, Vector2 v2, Vector2 v3)
	{
		uvs = new Vector2[3];
		uvs[0] = v1;
		uvs[1] = v2;
		uvs[2] = v3;
	}

	public void SetNormals(Vector3[] norms_in)
	{
		if (norms_in != null)
		{
			if (norms_in.Length == 3)
				norms = norms_in;
			else
				norms = new Vector3[3];
		}
		else
			norms = new Vector3[3];
	}

	public void SetTangents(Vector4[] tangs)
	{
		tangents = tangs;
	}

	public void UpdateBarycentre()
	{
		vCentre = Vector3.zero;
		for (int i = 0; i < 3; i++)
		{
			vCentre += sommets[i];
		}
		vCentre /= 3;
	}

	public void SetScale( Vector3 vScale_in)
	{
		vScale = vScale_in;
	}

	public void Rotate(Vector3 vRot)
	{
		// je peux créer un gameobject temporaire, lui assigner pour chacun des 3 sommets la position du sommet et utiliser le transform du gameobject pour 
		// tourner autour du transform de ce tri2faces
		// transform.RotateAround ();
	}

	public void MoveTo(Vector3 vPos)//nouveau vCentre
	{
		Vector3 vDif = vPos - vCentre;
		for (int i = 0; i < 3; i++)
		{
			sommets[i] += vDif ;
		}
		vCentre = vPos;
	}


	public void ComputeTris(int numTri, bool bDoubleFace)
	{
		// je ne dois appeler ça que si j'ai déjà mes 3 sommets
		if (sommets.Length == 3)
		{
			// chaque tri a 3 nouveaux sommets, le num de tri nous aide à savoir combien de sommets ont été créés depuis le début et donc connaitre l'indice de ces derniers.
			if (bDoubleFace)
				tris = new int[6];
			else
				tris = new int[3];
			for (int i = 0; i < 3; i++) {
				tris[i] = numTri*3 + i ;
			}
			if (bDoubleFace) // recto verso sur 3 sommets qui n'ont que ces 2 triangles donc on peut les déplacer librement sans risque.
			{
				tris[3] = tris[0] ;
				tris[4] = tris[2] ;
				tris[5] = tris[1] ;
			}


		}
	}



}
