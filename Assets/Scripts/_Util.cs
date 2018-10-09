using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class _Util {

	public static IEnumerator Wait(float time) {
		float start = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup < start + time) {
			yield return null;
		}
	}

	public static string RemoveDiacritics(string input) {
		string stFormD = input.Normalize(System.Text.NormalizationForm.FormD);
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		for (int i = 0; i < stFormD.Length; i++) {
			System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(stFormD[i]);
			if (uc != System.Globalization.UnicodeCategory.NonSpacingMark) {
				sb.Append(stFormD[i]);
			}
		}
		return (sb.ToString().Normalize(System.Text.NormalizationForm.FormC));
	}

	public static object GetDataValueForKey(Dictionary<string, object> dict, string key) {
		object objectForKey;
		if (dict.TryGetValue(key, out objectForKey)) {
			return objectForKey;
		} else {
			return "";
		}
	}
}


public class Tabela {
	public string nome;
	public int score;

	public Tabela() {
	}

	public Tabela(string nome, int score) {
		this.nome = nome;
		this.score = score;
	}
}