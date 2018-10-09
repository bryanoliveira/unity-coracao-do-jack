using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hexagono_Controlador : MonoBehaviour {

	// ---- CONSTS
	private const int fatorFacilidade = 5;
	private const float alphaOff = 0.2f;
	private const float alphaOn = 0.7f;
	private const float delayPadrao = 0.5f; // se alterar aqui, favor alterar a inicialização do delay
	// ---- CONSTS

	private static float delay = 0.5f; // se alterar aqui, favor alterar o delayPadrao
	private float finalA;
	private float finalS;

	private bool coringa;
	private bool eraCoringa = true; // para o tutorial
    private bool ligado = false;
	private bool subindoOpacidade;
 	private bool subindoEscala;
	private static bool primeiro = true;
	private static bool direcao = true; // true: direita; false:esquerda
    #if UNITY_ANDROID
    public static bool vibracao = false;
    #endif

    private static List<Color> cores;
	private static Color corChain;
	private Color minhaCor;

	[SerializeField]
    private SpriteRenderer sprite;

	[SerializeField]
	private Sprite[] skins;

    [SerializeField]
    private GameObject hexPrefab;

    [SerializeField]
    private Transform[] lados;

	private Hexagono_Controlador[] meus;

	public Main_Jogo main;

	private static List<Hexagono_Controlador> ligados;

	[SerializeField]
	private Animator anim;

    private IEnumerator alphaRotinaAtual;

    private void Start() {
		// aplica skin salva no hexagono
		sprite.sprite = skins [PlayerPrefs.GetInt ("skin")];

		// só pode ser coringa quem não for o primeiro hexagono
		// escolhe aleatoriamente dado o fator de facilidade
		if (!primeiro && Random.Range (0, 10) > 12 - fatorFacilidade) {
			coringa = true;
			// não precisa mais mudar a cor do hexágono pra branco, agora é um arco-iris
			// ao ativar o animador a animação principal é executada e já faz isso
			anim.enabled = true;

			// seta o alpha em 0
			sprite.color = new Color (sprite.color.r, sprite.color.g, sprite.color.b, 0);
			// aparece suavemente e sobe opacidade de 0 até alphaOff
			FadeIn (alphaOff, 10);
		} else {
			coringa = false;
			// se não for coringa, escolhe uma cor para esse hexagono
			minhaCor = sprite.color = cores[Random.Range(0, cores.Count)];

			// seta o alpha em 0
			sprite.color = new Color (sprite.color.r, sprite.color.g, sprite.color.b, 0);
			// aparece suavemente e sobe opacidade da original até alphaOff
			FadeIn (alphaOff, 10);

			if (primeiro) {
				corChain = minhaCor;
				Liga();
				main.ResetaPontos (); // reseta de novo pois o Ligar() pontua automaticamente. se resetar pra 0 caga o tutorial.
				StartCoroutine (GiraEmVolta ());
			}
		}

		// se esse for o primeiro hexagono, o proximo não será
		if (primeiro)
			primeiro = false;
    }

	private static void AddLigados(Hexagono_Controlador hex) {
		if(ligados == null)
			ligados = new List<Hexagono_Controlador> ();
		ligados.Add (hex);
	}

	public void Liga() {
		if (!ligado) {
			if (corChain == minhaCor || coringa) {// pontua se o hexagono não está ligado e é da mesma cor do anterior
				// se for coringa, pontua e muda sua cor temporariamente
				if (coringa) {
					anim.enabled = false;
					sprite.color = new Color (corChain.r, corChain.g, corChain.b, alphaOff);
				}
				main.BGFLash (corChain);
				main.AddPontos ();
				#if UNITY_ANDROID
                if(vibracao)
				    Vibration.Vibrate (40);
				#endif
				// aumenta a dificuldade aumentando a velocidade até um certo ponto
				if(delay > 0.2f)
					delay -= Time.deltaTime * (7 / fatorFacilidade);
			} else { // perde
				Perde();
			}
			AddLigados (this);
		}

		// liga o hexagono de forma animada
        this.ligado = true;
        StopAllCoroutines();
        FadeIn(alphaOn);
		// spawna os hexagonos faltantes em volta
		SpawnaEmVolta();
		// da uma piscada pra não deixar a opacidade bugar
		Pisca ();
    }

	private void Perde() {
		// agora o novo chain será da cor deste hexagono 
		corChain = minhaCor;
		// reseta o delay
		delay = delayPadrao;
		// desliga os hexagonos do chain anterior
		DesligaTudo ();
		// reseta os pontos e salva o recorde
		IniciaCores();
		main.Perde ();
		#if UNITY_ANDROID
		if(vibracao)
			Vibration.Vibrate (200);
		#endif
	}

    public void Desliga() {
		// só desliga se estiver ligado (verificação por 'DesligaTudo')
		if (ligado) {
			ligado = false;
			// se for um coringa e estiver ligado, sua cor não é a branca. resolve isso.
			if (coringa) {
				sprite.color = new Color (1, 1, 1, alphaOn); // alphaOn para fazer o fadeOut
				anim.enabled = true;
			}
			// desliga suavemente
			FadeOut(alphaOff);
		}
    }
	public void DesligaTudo() {
		foreach (Hexagono_Controlador hex in ligados) {
			hex.Desliga ();
		}
		ligados.Clear ();
	}

    private void SpawnaEmVolta() {
		// aloca o vetor de hexagonos
		meus = new Hexagono_Controlador[6]; // lados do hexagono sempre = 6
		// variavel pra receber o hexagono vizinho
		RaycastHit hit;
		// conserta a escala para a posição ficar correta
		transform.localScale = new Vector3(1, 1, 1);
		// para todos os lados
		for (int i = 0; i < 6; i++) {
			// verifica se no lado atual já existe um hexagono
			// distancia máxima entre hexagonos sempre < 2
			if (Physics.Raycast (this.transform.position, lados[i].transform.forward, out hit, 2)) {
				// se sim, guardo que este é meu vizinho
				meus [i] = hit.transform.GetComponent<Hexagono_Controlador> (); 
			} else {
				// se não, crio um vizinho
				GameObject hx = Instantiate (hexPrefab, lados[i].position, Quaternion.identity) as GameObject;
				meus [i] = hx.GetComponent<Hexagono_Controlador> ();
				// conto pra ele quem é minha mãe
				meus [i].main = main;
			}
		}
    }

	private IEnumerator Pisca() {
		// sobe suavemente a opacidade para a maxima
		FadeIn(1); // opacidade máxima sempre = 1
		ScaleUp(1.05f);
		// espero o delay global
		yield return new WaitForSeconds (delay);
		ScaleDown(1f);
		// fadeOut para a opacidade adequada
		if(ligado)
			FadeOut(alphaOn);
		else
			FadeOut(alphaOff);
	}

	public IEnumerator GiraEmVolta() {
		// inverte a direção do giro para maior dinamicidade
		direcao = !direcao;

		// deixa a escala alta enquanto esse for o foco
		ScaleUp(1.05f);

		int giros = 0;

		// lados do hexagono sempre = 6
		// sai do laço quando eu deixar de ser o foco
		if (direcao) { // gira pra direita
			for (int i = 0; main.foco == this; i++) {
				// dá aquela piscada no vizinho
				StartCoroutine (meus [i].Pisca ());
				// fala pra minha mae quem é que tá piscando
				main.atual = meus [i];
				// espera e repete
				yield return new WaitForSeconds (delay);
				// faz o loop repetir adequadamente
				if (i == 5) {
					i = -1; // é incrementado pra 0 quando volta no for
					giros++;
					if (giros > 2 && Main_Jogo.pontos > 2) {
						Perde ();
						giros = 0;
						this.ligado = true;
						StopAllCoroutines();
						FadeIn(alphaOn);
						Pisca ();
					}
				}
			}
		} else { // gira pra esquerda
			for (int i = 5; main.foco == this; i--) {
				// dá aquela piscada no vizinho
				StartCoroutine (meus [i].Pisca ());
				// fala pra minha mae quem é que tá piscando
				main.atual = meus [i];
				// espera pra repetir
				yield return new WaitForSeconds (delay);
				// faz o loop repetir adequadamente
				if (i == 0) {
					i = 6; // é decrementado pra 5 quando volta no for
					giros++;
					if (giros > 2) {
						Perde ();
						giros = 0;
						this.ligado = true;
						StopAllCoroutines();
						FadeIn(alphaOn);
						Pisca ();
					}
				}
			}
		}

		// volta a escala ao normal, esse deixou de ser o foco
		ScaleDown(1f);
	}

	private void FadeIn(float finalA, float maciez) {
		// evita conflitos com fadeOut e outras co-rotinas em execução
		subindoOpacidade = true;
		this.finalA = finalA;
		// inicia rotina paralela
		StartCoroutine (FadeInRoutine (maciez));
	}

	private void FadeIn(float finalA) {
		// evita conflitos com fadeOut e outras co-rotinas em execução
		subindoOpacidade = true;
		this.finalA = finalA;
		// inicia rotina paralela
		StartCoroutine (this.FadeInRoutine ());
	}
	private void FadeOut(float finalA) {
		// evita conflitos com fadeIn e outras co-rotinas em execução
		subindoOpacidade = false;
		this.finalA = finalA;
		// inicia rotina paralela
		StartCoroutine (this.FadeOutRoutine ());
	}

	// muito orgulhoso dessa solução, sério
	// se voce mudar só o valor do delay, a velocidade do jogo inteiro é alterada
	private IEnumerator FadeInRoutine() {
		// enquanto minha opacidade for menor que a final, incrementa proporcionalmente ao delay
		while (subindoOpacidade && sprite.color.a < finalA) {
			// atualiza a cor
			sprite.color = new Color (sprite.color.r, sprite.color.g, sprite.color.b, sprite.color.a + delay / 10);
			// espera pra repetir
			yield return new WaitForSeconds (delay / 5000);
		}
	}
	private IEnumerator FadeOutRoutine() {
		// enquanto minha opacidade for maior que a final, decrementa proporcionalmente ao delay
		while (!subindoOpacidade && sprite.color.a > finalA) {
			// atualiza a cor
			sprite.color = new Color (sprite.color.r, sprite.color.g, sprite.color.b, sprite.color.a - delay / 10);
			// espera pra repetir
			yield return new WaitForSeconds (delay / 5000);
		}
	}

	private IEnumerator FadeInRoutine(float maciez) {
		// enquanto minha opacidade for menor que a final, incrementa proporcionalmente ao delay
		while (subindoOpacidade && sprite.color.a < finalA) {
			// atualiza a cor
			sprite.color = new Color (sprite.color.r, sprite.color.g, sprite.color.b, sprite.color.a + delay / maciez);
			// espera pra repetir
			yield return new WaitForSeconds (delay / 5000);
		}
	}


	private void ScaleUp(float finalS) {
		this.finalS = finalS;
		subindoEscala = true;
		StartCoroutine(ScaleUpRoutine());
	}
	private void ScaleDown(float finalS) {
		if (main.foco == this)
			return;
		this.finalS = finalS;
		subindoEscala = false;
		StartCoroutine(ScaleDownRoutine());
	}

	private IEnumerator ScaleUpRoutine() {
		// enquanto minha escala for menor que a final, incrementa proporcionalmente ao delay
		while (subindoEscala && transform.localScale.x < finalS) {
			transform.localScale = new Vector3 (transform.localScale.x + delay / 10, transform.localScale.y + delay / 10, 1);
			// espera pra repetir
			yield return new WaitForSeconds (delay / 1000);
		}
	}
	private IEnumerator ScaleDownRoutine() {
		// enquanto minha escala for maior que a final, decrementa proporcionalmente ao delay
		while (!subindoEscala && transform.localScale.x > finalS) {
			transform.localScale = new Vector3 (transform.localScale.x - delay / 100, transform.localScale.y - delay / 100, 1);
			// espera pra repetir
			yield return new WaitForSeconds (delay / 1000);
		}
	}

	public static void Reiniciar() {
		delay = delayPadrao;
		primeiro = true;
		ligados.Clear ();
		cores.Clear ();
	}

	public static void IniciaCores() {
		// preenche vetor de cores com cores primarias
		cores = new List<Color>();
		cores.Add (Color.red);
		cores.Add (Color.green);
		cores.Add (Color.blue);
	}
	public static void AddCor() {
		cores.Add(cores[Random.Range(0, cores.Count / 2)] + cores[Random.Range(cores.Count / 2, cores.Count)]);
	}

	public static void TodosSaoCoringas(bool sim) {
		GameObject[] hexes = GameObject.FindGameObjectsWithTag ("Hexagono");
		foreach (GameObject hex in hexes) {
			hex.GetComponent<Hexagono_Controlador> ().SetCoringa (sim);
		}
	}

	private void SetCoringa(bool sim) {
		if (!ligado) {
			if (sim && !coringa) {
				sprite.color = new Color (1, 1, 1, 0);
				anim.enabled = true;
				eraCoringa = coringa;
				coringa = true;
				FadeIn (alphaOff, 100);
			}
			else if (!eraCoringa) {
				// se não for coringa, volta pra cor anterior e para animação
				coringa = false;
				anim.enabled = false;
				sprite.color = minhaCor;
				// seta o alpha em 0
				sprite.color = new Color (sprite.color.r, sprite.color.g, sprite.color.b, 0);
				// aparece suavemente e sobe opacidade da original até alphaOff
				FadeIn (alphaOff, 100);
			}
		}
	}
}