using UnityEngine;
using System.Collections;
using Facebook.Unity;
using System.Collections.Generic;

public class Main_Facebook : MonoBehaviour {
	[SerializeField]
	private Main_Jogo main;

	public static string playerId;

	[SerializeField]
	private UmRecorde_Referencias jogadorRef;

	private void Awake() {
		// inicia a API do Facebook
		if (!FB.IsInitialized) {
			FB.Init(InitCallback, main.Pausa);
		} else {
			FB.ActivateApp();
			if (FB.IsLoggedIn) {
				LoginCallback (null);
				if (PlayerPrefs.GetInt ("recordePendente") == 1) {
					PostaScore (0);
				}
			}
		}
	}

	private void InitCallback () {
		if (FB.IsInitialized) {
			FB.ActivateApp();
			LoginCallback (null);
			if (PlayerPrefs.GetInt ("recordePendente") == 1) {
				PostaScore (0);
			}
		} else {
			Debug.Log("Não deu pra inicializar a SDK do Facebook.");
		}
	}
	public void Loga() {
		FB.LogInWithReadPermissions(new List<string>() { "public_profile" }, LoginCallback); // , "email", "user_friends", "publish_actions"
	}
	private void LoginCallback(ILoginResult result) {
		if (FB.IsLoggedIn) {
			main.FBLogou ();

			FB.API("/me", HttpMethod.GET, PegaDados);
			FB.API("/app/scores?fields=score,user.limit(30)", HttpMethod.GET, PegaScores);
		} else {
			Debug.Log("Login cancelado.");
		}
	}

    public void BuscaScores() {
        FB.API("/app/scores?fields=score,user.limit(30)", HttpMethod.GET, PegaScores);
    }

	private void PegaScores(IGraphResult result) {
		// verifica erros
		if (!string.IsNullOrEmpty(result.Error) || result.Cancelled) {
			Debug.Log ("Não foi possível buscar amigos.");
			return;
		}

		// pega os resultados e itera sobre eles
		var friendList = result.ResultDictionary["data"] as List<object>;

		Tabela[] tabelaScores = new Tabela[friendList.Count];

		string posicaoTexto;

		for(int i = 0; i < friendList.Count; i++) {
			tabelaScores [i] = new Tabela ();
			posicaoTexto = (i + 1).ToString ();

			// separa as listas
			var item = (Dictionary<string,object>) friendList[i];
			var user = (Dictionary<string,object>) item["user"];

			if (user ["id"].ToString () == playerId) {
				if (Main_Jogo.estado == 0) {
					jogadorRef.recorde.text = item ["score"].ToString ();
					jogadorRef.posicao.text = (i + 1).ToString ();
					tabelaScores [i].nome = "voce";
					tabelaScores [i].score = int.Parse (jogadorRef.recorde.text);
					if (Main_Jogo.recorde > tabelaScores [i].score) {
						PostaScore (0);
					}
					continue;
				} else {
					posicaoTexto = " · " + (i + 1);
				}
			} else if (int.Parse(item["score"].ToString()) == 0)
				continue;

			string nome = _Util.RemoveDiacritics (user["name"].ToString()).Split(' ')[0];

			main.AdicionaRecordeView(nome, item["score"].ToString(), posicaoTexto, (i + 1));

			tabelaScores [i].nome = nome;
			tabelaScores [i].score = int.Parse(jogadorRef.recorde.text);
		}
        main.SetTabela(tabelaScores);
	}

	public void PostaScore(int pontos) {
		if (!FB.IsInitialized)
			return;
        if (pontos < Main_Jogo.recorde)
            pontos = Main_Jogo.recorde;

		PlayerPrefs.SetInt ("recorde", pontos);

		var scoreData = new Dictionary<string,string> ();
		scoreData ["score"] = pontos.ToString ();

		FB.API ("/me/scores", HttpMethod.POST, delegate(IGraphResult result) {
			if(result.RawResult.Contains("error")) {
				PlayerPrefs.SetInt("recordePendente", 1);
			}
		}, scoreData);
	}

	private void PegaDados(IGraphResult result) {
		if (!string.IsNullOrEmpty(result.Error) || result.Cancelled) {
			// Handle error
		} else {
			jogadorRef.nome.text = result.ResultDictionary["name"].ToString().Split(' ')[0];
			playerId = result.ResultDictionary ["id"].ToString ();
		}
	}

	public void Logout() {
		if (FB.IsLoggedIn) {
			FB.LogOut ();
		}
		StartCoroutine (EsperaDeslogar ());
	}

	private IEnumerator EsperaDeslogar() {
		while (FB.IsLoggedIn) {
			yield return new WaitForSeconds (0.2f);
		}
		main.FBDeslogou ();
	}

	public void ConvidaAmigos() {
		FB.AppRequest(
			"Te desafio a bater meu recorde do heX. Consegue?",
			null, null, null, null, null, null,
			delegate (IAppRequestResult result) {
				//Debug.Log(result.RawResult);
			}
		);
	}
}
