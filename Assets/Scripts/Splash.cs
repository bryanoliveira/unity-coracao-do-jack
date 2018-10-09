using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Splash : MonoBehaviour {

	[SerializeField]
	private float segundos = 2;

	void Start () {
		StartCoroutine (Vai ());
	}
	private IEnumerator Vai() {
		yield return new WaitForSeconds (segundos);
		SceneManager.LoadScene (1);
	}
}
