using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using MichaelsoftBinbowsWords;
using rnd = UnityEngine.Random;

public class MichaelsoftBinbowsScript : MonoBehaviour {

	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMBombModule Module;
	public KMColorblindMode Colorblind;

	public KMSelectable[] logoButtons;
	public KMSelectable backButton;
	public KMSelectable[] keyboard;
	public KMSelectable back, sub;

	public GameObject[] buttonObjects;
	public GameObject initScreen;
	public GameObject[] screens;
	public GameObject keyboardParent;

	public TextMesh[] displayTexts;
	public TextMesh[] buttonCBTexts;
	public TextMesh mainCBText;

	public Material blackMat, mainScreenColor;
	public Material[] screenColors;
	public MeshRenderer mainScreen;

	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;

	private int _michaelsoftBinbowsId;
	private static int _michaelsoftBinbowsIdCounter = 1;

	private bool cbActive;

	private bool isActivated;

	private bool inScreen;

	private bool inSubmission;

	private string[] colorNames = { "Green", "Red", "Yellow", "Blue" };

	private string alphaSetSpecial;
	private string alphaSetNoSpecial = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

	private string submissionText;

	private string encrypted, encrypted2;

	private string[] hexNumber = new string[5];

	private char initVector;

	private string word;

	int colorPosTP = 0;

	Words wordList = new Words();

	void Awake()
    {

		moduleId = moduleIdCounter++;
		_michaelsoftBinbowsId = _michaelsoftBinbowsIdCounter++;

		foreach (KMSelectable button in logoButtons)
		{
			button.OnInteract += delegate () { logoButtonPress(button); return false; };
		}

		foreach (KMSelectable key in keyboard)
		{
			key.OnInteract += delegate () { keyPress(key); return false; };
		}

		backButton.OnInteract += delegate () { backButtonPress(); return false; };

		back.OnInteract += delegate () { backPress(); return false; };

		sub.OnInteract += delegate () { subPress(); return false; };

		cbActive = Colorblind.ColorblindModeActive;

		Bomb.GetComponent<KMBombModule>().OnActivate += onActivate;

    }

	void OnDestroy()
	{
		_michaelsoftBinbowsIdCounter = 1;
	}

	
	void Start()
    {
		for (int i = 0; i < 4; i++)
		{
			buttonCBTexts[i].text = "";
		}
		foreach (GameObject obj in buttonObjects)
		{
			obj.SetActive(false);
		}
		initScreen.SetActive(true);
		mainScreen.material = blackMat;
		keyboardParent.SetActive(false);

		alphaSetSpecial = "" + alphaSetNoSpecial + "@$%&?=";

		wordSelect();
    }

	void wordSelect()
	{
		word = wordList.selectWord();

		encrypted = word;
		encrypted2 = encrypted;

		for (int i = 0; i < 5; i++)
		{
			hexNumber[i] += "0123456789ABCDEF"[rnd.Range(0, 16)];
		}
		logThings("The decrypted email is: " + word);
		logThings("Your 5-digit hexidecimal number is: " + hexNumber.Join(""));

		ebcEncryption();
		cbcEncryption();
	}

	void ebcEncryption()
	{
		encrypted = encryptionOne(encrypted);
		logThings(String.Format("After encrypting with EBC: {0}", encrypted));
	}

	void cbcEncryption()
	{
		initVector = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".PickRandom();
		logThings("Your initial vector is: " + initVector);
		encrypted2 = encryptionTwo(encrypted2);
		logThings(String.Format("Your re-encrypted word is: {0}", encrypted2));
	}

	void logThings(string log)
	{
		Debug.LogFormat("[Michaelsoft Binbows #{0}] {1}", moduleId, log);
	}

	private string encryptionOne(string word)
	{
		string output = "";

		for (int i = 0; i < word.Length; i++)
		{
			string letBin = letToBin(word[i]), encBin = "";

			var hex = hexNumber.ToArray();

			for (int j = 0; j < 5; j++)
			{
				string bin = hexToBin(hex[j]) + "" + letBin[j];
				encBin = encBin + "" + (bin.Replace("0", "").Length % 2);
				logThings(String.Format("{0} + {1} -> {2} -> {3}", hex[j], letBin[j], bin, encBin[j]));
			}

			char let = binToLet(encBin);

			if ("@$%&?=".Contains(let))
			{
				output += binToLet(encBin);
			}
			else
			{
				char temp = binToLet(encBin);

				if (!"@$%&?=".Contains(temp) && rnd.Range(0,3) == 0)
				{
					output += temp;
				}
				else
				{
					output += let;
				}

				if (i < word.Length - 1)
				{
					for (int j = 0; j < 5; j++)
					{
						hex[j] += "0123456789ABCDEF"[rnd.Range(0, 16)];
					}
				}
            }
		}

		return output;
	}

	string encryptionTwo(string word)
	{
		string output = "";

		var vector = initVector;

        for (int i = 0; i < word.Length; i++)
        {
            string letBin = letToBin(word[i]), encBin = "";

			string vectorBin = letToBin(vector);

            var hex = hexNumber.ToArray();

            for (int j = 0; j < 5; j++)
            {
                string bin = hexToBin(hex[j]) + "" + letBin[j];
                encBin = encBin + "" + (bin.Replace("0", "").Length % 2);
                logThings(String.Format("{0} + {1} -> {2} -> {3}", hex[j], letBin[j], bin, encBin[j]));
            }


            var xorOutput = xor(encBin, vectorBin);


            char let = binToLet(xorOutput);
         
            vector = let;

            if ("@$%&?=".Contains(let))
            {
				
                output += vector;
            }
            else
            {

                char temp = vector;



                if (!"@$%&?=".Contains(temp) && rnd.Range(0, 3) == 0)
                {
                    output += temp;
                }
                else
                {
                    output += let;
                }
            }
        }

        return output;
	}

	string xor(string b1, string b2)
	{
		string result = "";

		for (int i = 0; i < b1.Length && i < b2.Length; i++)
		{
			result = result + (b1[i] == b2[i] ? "0" : "1");
		}
		return result;
	}

	string letToBin(char c)
	{
		string[] bins = { "00000", "00001", "00010", "00011", "00100", "00101", "00110", "00111", "01000", "01001", "01010", "01011", "01100", "01101", "01110", "01111",
        "10000", "10001", "10010", "10011", "10100", "10101", "10110", "10111", "11000", "11001", "11010", "11011", "11100", "11101", "11110", "11111" };
		return bins[alphaSetSpecial.IndexOf(c)];
    }

	char binToChar(string bin)
	{
        string[] bins = {
            "00000", "00001", "00010", "00011", "00100", "00101", "00110", "00111", "01000", "01001", "01010", "01011", "01100", "01101", "01110", "01111",
            "10000", "10001", "10010", "10011", "10100", "10101", "10110", "10111", "11000", "11001", "11010", "11011", "11100", "11101", "11110", "11111"
        };
		return alphaSetSpecial[Array.IndexOf(bins, bin)];
    }

	string binToHex(string bin)
	{
		string[] bins = { "0000", "0001", "0010", "0011", "0100", "0101", "0110", "0111", "1000", "1001", "1010", "1011", "1100", "1101", "1110", "1111" };
		string hex = "", alpha = "0123456789ABCDEF";

		for (int i = 0; i < bin.Length; i+= 4)
		{
			hex = hex + "" + alpha[Array.IndexOf(bins, bin.Substring(i, 4))];
		}
		return hex;
	}

	string hexToBin(string hex)
	{
        string[] bins = { "0000", "0001", "0010", "0011", "0100", "0101", "0110", "0111", "1000", "1001", "1010", "1011", "1100", "1101", "1110", "1111" };
		string bin = "", alpha = "0123456789ABCDEF";

		for (int i = 0; i < hex.Length; i++)
		{
			bin += bins[alpha.IndexOf(hex[i])];
		}

		return bin;
    }

	private char binToLet(string bin)
	{
		string[] bins =
		{
			"00000", "00001", "00010", "00011", "00100", "00101", "00110", "00111", "01000", "01001", "01010", "01011", "01100", "01101", "01110", "01111",
			"10000", "10001", "10010", "10011", "10100", "10101", "10110", "10111", "11000", "11001", "11010", "11011", "11100", "11101", "11110", "11111"
		};

		return alphaSetSpecial[Array.IndexOf(bins, bin)];
	}

	void onActivate()
	{
		if (_michaelsoftBinbowsId == 1)
		{
			Audio.PlaySoundAtTransform("Startup", transform);
		}
		initScreen.SetActive(false);
		mainScreen.material = mainScreenColor;
		initializeMainButton();
		isActivated = true;
	}

	void initializeMainButton()
	{
		buttonObjects[0].SetActive(true);
		buttonObjects[1].SetActive(false);
		mainCBText.text = "";

		for (int i = 0; i < 4; i++)
		{
			buttonCBTexts[i].text = cbActive ? colorNames[i][0].ToString() : "";
		}
	}

	void logoButtonPress(KMSelectable button)
	{
		button.AddInteractionPunch(0.4f);
		Audio.PlaySoundAtTransform("Click", transform);

		if (inScreen || moduleSolved)
		{
			return;
		}

		for (int i = 0; i < 4; i++)
		{
			if (button == logoButtons[i])
			{
				if (!inScreen)
				{
					showColorScreen(i);
					colorPosTP = i;
				}
			}
		}
	}

	void keyPress(KMSelectable letter)
	{
		letter.AddInteractionPunch(0.4f);
		Audio.PlaySoundAtTransform("KeyPress", transform);

		if (!inSubmission || !inScreen || moduleSolved || !inSubmission)
		{
			return;
		}

		if (displayTexts[0].text.Length < 12)
		{
			submissionText += letter.GetComponentInChildren<TextMesh>().text;
			displayTexts[0].text = submissionText;
		}
	}

	void backPress()
	{
		back.AddInteractionPunch(0.4f);
		Audio.PlaySoundAtTransform("KeyPress", transform);

		if (!inSubmission || !inScreen || moduleSolved)
		{
			return;
		}

		if (displayTexts[0].text.Length > 0)
		{
			submissionText = submissionText.Remove(submissionText.Length - 1);
			displayTexts[0].text = submissionText;
		}
	}

	void subPress()
	{
		sub.AddInteractionPunch(0.4f);
		Audio.PlaySoundAtTransform("KeyPress", transform);

		if (!inSubmission || !inScreen || moduleSolved)
		{
			return;
		}

		if (submissionText == encrypted2)
		{
			moduleSolved = true;
			foreach (GameObject obj in screens)
			{
				obj.SetActive(false);
			}
            buttonObjects[1].SetActive(false);
            keyboardParent.SetActive(false);
            initScreen.SetActive(true);
			Audio.PlaySoundAtTransform("Solve", transform);
			Module.GetComponent<KMBombModule>().HandlePass();
			logThings("The answer matches the re-encrypted word. Solved!");
        }
		else
		{
			displayTexts[0].text = "";
			logThings(String.Format("The answer does not match the re-encrypted word. You submitted {0} instead of {1}. Strike!", submissionText.Length == 0 ? "nothing" : submissionText, encrypted2));
			Module.GetComponent<KMBombModule>().HandleStrike();
			submissionText = "";
		}
	}

	void showColorScreen(int colorPos)
	{
		inScreen = true;
		buttonObjects[0].SetActive(false);
		buttonObjects[1].SetActive(true);
		mainScreen.material = screenColors[colorPos];
		mainCBText.text = cbActive ? colorNames[colorPos].ToUpper() : "";
		switch (colorPos)
		{
			case 0:
				screens[0].SetActive(true);
				displayTexts[0].text = encrypted;
				break;
			case 1:
				screens[2].SetActive(true);
				displayTexts[2].text = hexNumber.Join("");
				break;
			case 2:
				screens[1].SetActive(true);
				displayTexts[1].text = initVector.ToString();
				break;
			case 3:
				Audio.PlaySoundAtTransform("SubmissionMode", transform);
				displayTexts[0].text = submissionText;
				inSubmission = true;
				screens[0].SetActive(true);
				keyboardParent.SetActive(true);
				break;
		}
	}

	void backButtonPress()
	{
		if (!inScreen || moduleSolved)
		{
			return;
		}

		backButton.AddInteractionPunch(0.4f);

		Audio.PlaySoundAtTransform("BackToMain", transform);

		foreach (GameObject obj in screens)
		{
			obj.SetActive(false);
		}
		keyboardParent.SetActive(false);
		mainScreen.material = mainScreenColor;
		initializeMainButton();
		inScreen = false;
	}
	
	
	void Update()
    {

    }

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"Use !{0} cb to enable colorblind mode. || !{0} G/GREEN/R/RED/Y/YELLOW/B/BLUE to go to any of the segments. || !{0} back to return to the main segment. || !{0} type [insert answer here] to type out your answer. || !{0} clear to clear your answer. || !{0} submit to submit your answer.";
#pragma warning restore 414

	private int getCharIndex(char c)
	{
		return "QWERTYUIOPASDFGHJKLZXCVBNM@$%&?=".IndexOf(c);
	}

	void tpCb()
	{
		if (!inScreen)
		{
            for (int i = 0; i < 4; i++)
            {
                buttonCBTexts[i].text = cbActive ? colorNames[i][0].ToString() : "";
            }
        }
		else
		{
			mainCBText.text = cbActive ? colorNames[colorPosTP] : "";
		}

	}

	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

		List<string> colors = new List<string> { "G", "GREEN", "R", "RED", "Y", "YELLOW", "B", "BLUE" };


        if (!isActivated)
        {
            yield return "sendtochaterror The module isn't fully activated yet!";
            yield break;
        }

        if (split[0].EqualsIgnoreCase("CB"))
		{
			cbActive = !cbActive;
			tpCb();
			yield break;
		}

		if (colors.Contains(split[0].ToUpper()))
		{
			if (inScreen)
			{
				yield return "sendtochaterror Go back to the main screen before selecting a color section!";
				yield break;
			}
			switch (split[0].ToUpper())
			{
				case "G":
				case "GREEN":
					logoButtons[0].OnInteract();
					break;
				case "R":
				case "RED":
					logoButtons[1].OnInteract();
					break;
				case "Y":
				case "YELLOW":
					logoButtons[2].OnInteract();
					break;
				case "B":
				case "BLUE":
					logoButtons[3].OnInteract();
					break;
			}
			yield break;
		}
		if (split[0].EqualsIgnoreCase("BACK"))
		{
			if (!inScreen)
			{
				yield return "sendtochaterror You are not in a colored section!";
			}
			else
			{
				backButton.OnInteract();
			}
			yield break;
		}
		if (split[0].EqualsIgnoreCase("CLEAR"))
		{
			if (!inScreen && !inSubmission)
			{
				yield return "sendtochaterror You are not in the blue section!";
				yield break;
			}
			while (displayTexts[0].text.Length != 0)
			{
				back.OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
			yield break;
		}

		if (split[0].EqualsIgnoreCase("TYPE"))
		{
			if (!inScreen || !inSubmission)
			{
				yield return "sendtochaterror You are not in the blue section!";
				yield break;
			}
			if (split.Length != 2)
			{
				yield return "sendtochaterror Please specify what you want to type!";
				yield break;
			}
			if (split[1].Length > 12)
			{
				yield return "sendtochaterror You can't submit over 12 characters!";
				yield break;
			}
			int[] buttons = split[1].Select(getCharIndex).ToArray();
			if (buttons.Any(x => x < 0))
			{
				yield break;
			}
			foreach (char character in split[1])
			{
				keyboard[getCharIndex(character)].OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
			yield break;
		}
		if (split[0].EqualsIgnoreCase("SUBMIT"))
		{
			if (!inScreen || !inSubmission)
			{
				yield return "sendtochaterror You are not in the blue section!";
				yield break;
			}
			sub.OnInteract();
		}
    }

	IEnumerator TwitchHandleForcedSolve()
    {

		while (!isActivated)
		{
			yield return true;
		}

		if (colorPosTP != 3)
		{
			backButton.OnInteract();
		}
		if (!inScreen && !inSubmission)
		{
			logoButtons[3].OnInteract();
		}
		if (inSubmission && !encrypted2.StartsWith(displayTexts[0].text))
		{
			while (displayTexts[0].text.Length != 0)
			{
				back.OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
		}

		int start = inSubmission ? displayTexts[0].text.Length : 0;

		for (int i = start; i < 12; i++)
		{
			keyboard[getCharIndex(encrypted2[i])].OnInteract();
			yield return new WaitForSeconds(0.1f);
		}

		sub.OnInteract();

		yield return null;
    }


}





