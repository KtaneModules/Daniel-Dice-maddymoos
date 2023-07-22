using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using KModkit;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;

public class DanielDice : MonoBehaviour {

	static readonly float Beat = .4f;
	static private int _moduleIdCounter = 1;
	private int _moduleId;
	public KMAudio Audio;
	public KMBombModule Module;
	public KMBombInfo Bomb;
	private int NumCorrect;
	public MeshRenderer[] Dice;
	public TextMesh Text;
	public KMSelectable[] Buttons;
	public KMSelectable MuteButton;
	private bool ShutUp;
	public MeshRenderer[] FakeHLs;
	public Material[] colorizer;
	private bool RDRTS, solved;
	public bool TestRD;
	private int[] OldDice = new int[2];
	private int[] NewDice = new int[2];
	private int[] DieCols = new int[2];
	private int OldSum, NewSum, MusicLocation, input;
	private int[] MusicNums = { 1, 2, 1, 2, 3, 4, 3, 5 };
	private bool Stop, badbad, intro;

	void Awake()
    {
		_moduleId = _moduleIdCounter++;
		RDRTS = (ModSettingsJSON.Get() || TestRD);
		for (int i = 0; i < Buttons.Length; i++)
		{
			int j = i;
			Buttons[j].OnInteract += delegate
			{
				if (!Stop)
					HandlePress(j);
				return false;
			};
			Buttons[j].OnHighlight += delegate
			{
				if (!Stop)
					HL(true, j);
			};
			Buttons[j].OnHighlightEnded += delegate
			{
				if (!Stop)
					HL(false, j);
			};
		}
		MuteButton.OnInteract += delegate { ShutUp = !ShutUp; return false;};
	}
	void HandlePress(int j)
    {
		Buttons[j].AddInteractionPunch();
        switch (j)
        {
			case 0: input = 1; break;
			case 1: input = -1; break;
        }
    }
	void HL(bool b, int j)
	{
		FakeHLs[j].enabled = b;
	}
	// Use this for initialization
	void Start () {
		Text.text = RDRTS ? "00/30" : "0 / 5";
		Debug.Log(RDRTS);
		NewDice[0] = Rnd.Range(1, 7);
		NewDice[1] = Rnd.Range(1, 7);
		StartCoroutine(IntroMusic());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void DanielTheDice(int Button)
    {
		badbad = false;
		if (RDRTS)
		{
			NewDice[0] = Rnd.Range(1, 7);
			NewDice[1] = Rnd.Range(1, 7);

		}
        else
        {
			if (!intro)
			{
				NewDice[0] = Operations(DieCols[0], OldDice[0], OldDice[1], true);
				NewDice[1] = Operations(DieCols[1], OldDice[1], OldDice[0], true);
			}
        }
		DieCols[0] = Rnd.Range(0, 4);
		DieCols[1] = Rnd.Range(0, 4);
		NewSum = NewDice.Sum();
		StartCoroutine(Animate(Button));
	}

	int Operations(int color, int a, int b, bool log)
    {
		int c = 0;
		switch (color)
		{
			//red blue yellow greenm
			case 0: c = a + b; c %= 6; break;
			case 1: c = a - b; c = ((c % 6) + 6) % 6; break;
			case 2: c = a * b; c = c % 6; break;
			case 3:
				while (a % b != 0) a += 7;
				c = a / b;
				c %= 7;
				break;

		}
		if (log)
			Debug.Log("Color:" + color + " a=" + a + " b=" + b + " c=" + c);
		if (c == 0) c = 6;
		return c;
    }
	IEnumerator Animate(int Button)
	{
		Stop = true;
		input = 0;
		if ((OldSum == NewSum || (Button == 1 && OldSum < NewSum) || (Button == -1 && OldSum > NewSum)) && !intro)
		{
			if(!ShutUp) Audio.PlaySoundAtTransform("DanDice_Win", Module.transform);
			if (OldSum != NewSum || (OldSum == 12 && Button == 1) || (OldSum == 2 && Button == -1))
			{
				Debug.LogFormat("[Daniel Dice #{0}]: You said {1}. I rolled {2}. Compared to the last roll ({3}), you were correct!", _moduleId, Button == 1 ? "Higher" : "Lower", NewSum, OldSum);
				NumCorrect++;
				if (RDRTS) {
					if (Button == 1)
					{
						switch (OldSum)
						{
							case 9: NumCorrect += 1; Debug.LogFormat("[Daniel Dice #{0}]: For your risky roll, you earned 1 bonus point!", _moduleId); break;
							case 10: NumCorrect += 2; Debug.LogFormat("[Daniel Dice #{0}]: For your risky roll, you earned 2 bonus points!", _moduleId); break;
							case 11: NumCorrect += 4; Debug.LogFormat("[Daniel Dice #{0}]: For your risky roll, you earned 4 bonus points!", _moduleId); break;
							case 12: NumCorrect += 6; Debug.LogFormat("[Daniel Dice #{0}]: For your risky roll, you earned 6 bonus points!", _moduleId); break;
						}
					}
					if (Button == -1)
					{
						switch (OldSum)
						{
							case 5: NumCorrect += 1; Debug.LogFormat("[Daniel Dice #{0}]: For your risky roll, you earned 1 bonus point!", _moduleId); break;
							case 4: NumCorrect += 2; Debug.LogFormat("[Daniel Dice #{0}]: For your risky roll, you earned 2 bonus points!", _moduleId); break;
							case 3: NumCorrect += 4; Debug.LogFormat("[Daniel Dice #{0}]: For your risky roll, you earned 4 bonus points!", _moduleId); break;
							case 2: NumCorrect += 6; Debug.LogFormat("[Daniel Dice #{0}]: For your risky roll, you earned 6 bonus points!", _moduleId); break;
						}
					}
				}
			}
			else
				Debug.LogFormat("[Daniel Dice #{0}]: Tie... no points.", _moduleId);

		}
		else if (((Button == 1 && OldSum > NewSum) || (Button == -1 && OldSum < NewSum)) && !intro)
		{
			if(!ShutUp) Audio.PlaySoundAtTransform("DanDice_Loss", Module.transform);
			Debug.LogFormat("[Daniel Dice #{0}]: Oof. You picked {1}, and I rolled {2}, compared to {3}.", _moduleId, Button == 1 ? "Higher" : "Lower", NewSum, OldSum);
			badbad = true;
			intro = true;
		}
		yield return new WaitForSecondsRealtime(2f * Beat);
		Audio.PlaySoundAtTransform("diceshake", Module.transform);
		yield return new WaitForSecondsRealtime(3.5f * Beat);
		Audio.PlaySoundAtTransform("roll1", Module.transform);
		yield return new WaitForSecondsRealtime(.75f * Beat);
		Audio.PlaySoundAtTransform("roll3-1", Module.transform);
		Audio.PlaySoundAtTransform("roll3-2", Module.transform);
		if (!RDRTS)
		{
			Dice[0].material = colorizer[DieCols[0]];
			Dice[1].material = colorizer[DieCols[1]];
		}
		Dice[0].material.mainTextureOffset = new Vector2(.1f * NewDice[0], 0f);
		Dice[1].material.mainTextureOffset = new Vector2(.1f * NewDice[1], 0f);

		yield return new WaitForSecondsRealtime(5.75f * Beat);
		yield return null;
		OldDice[0] = NewDice[0];
		OldDice[1] = NewDice[1];
		OldSum = NewSum;
		intro = false;

		if (badbad)
        {
			if(!RDRTS) Module.HandleStrike();
			NumCorrect = 0;
			StartCoroutine(IntroMusic());
			Dice[0].material.mainTextureOffset = new Vector2(0f, 0f);
			Dice[1].material.mainTextureOffset = new Vector2(0f, 0f);
		}
		if (RDRTS)
		{
			Text.text = NumCorrect.ToString().PadLeft(2, '0') + "/30";

			if (NumCorrect == 30) { Module.HandlePass(); solved = true; Debug.LogFormat("[Daniel Dice #{0}]: You are insane. RDRTS mode solved. Congrats. Please DM me!", _moduleId); }

			}
		else { Text.text = NumCorrect.ToString() + " / 5"; if (NumCorrect == 5) { Module.HandlePass(); solved = true; } }
		Stop = false;
    }

	IEnumerator IntroMusic()
    {
		yield return null;
		StartCoroutine(MusicLoop());
		intro = true;
		input = 0;
		if(!ShutUp) Audio.PlaySoundAtTransform("DanDice_D", Module.transform);
		DanielTheDice(0);
	}
	IEnumerator MusicLoop()
    {

		yield return new WaitForSecondsRealtime(32f * Beat);


		while (!solved) 
		{
            while (Stop)
            {
				yield return null;
                if (badbad)
                {
					break;
                }
            }
			if (badbad || solved) break;
			while (input == 0 && !solved)
			{
				if(!ShutUp) Audio.PlaySoundAtTransform("DanDice_D" + MusicNums[MusicLocation], Module.transform);
				yield return new WaitForSecondsRealtime(8f * Beat);
				MusicLocation++;
				MusicLocation %= 8;
			}
			FakeHLs[0].enabled = false;
			FakeHLs[1].enabled = false;
			DanielTheDice(input);
			yield return null;
		}
    }

	//twitch plays
	private bool firstGuess = true;
	#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} higher/lower [Guesses higher or lower] | !{0} music [Toggles the music] | !{0} rdrts [Enables RDRTS (must be done before 1st guess and cannot be undone)]";
	#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
    {
		if (command.ToLower().Equals("rdrts"))
        {
			if (RDRTS)
            {
				yield return "sendtochaterror RDRTS is already enabled!";
				yield break;
            }
			if (!firstGuess)
			{
				yield return "sendtochaterror You have already guessed at least once!";
				yield break;
			}
			yield return null;
			RDRTS = true;
			Text.text = "00/30";
			Dice[0].material = colorizer[0];
			Dice[1].material = colorizer[0];
			DanielTheDice(0);
			yield break;
        }
		if (command.ToLower().Equals("music")||command.ToLower().Equals("mute"))
        {
			yield return null;
			MuteButton.OnInteract();
			yield break;
		}
		if (command.ToLower().Equals("higher"))
		{
			if (Stop)
            {
				yield return "sendtochaterror You cannot guess while a bet is being processed!";
				yield break;
            }
			yield return null;
			Buttons[0].OnInteract();
			firstGuess = false;
			yield return "solve";
			yield return "strike";
			yield break;
		}
		if (command.ToLower().Equals("lower"))
		{
			if (Stop)
			{
				yield return "sendtochaterror You cannot guess while a bet is being processed!";
				yield break;
			}
			yield return null;
			Buttons[1].OnInteract();
			firstGuess = false;
			yield return "solve";
			yield return "strike";
		}
	}

	IEnumerator TwitchHandleForcedSolve()
    {
		if (!RDRTS && badbad)
        {
			StopAllCoroutines();
			Module.HandlePass();
			solved = true;
			yield break;
        }
		while (!solved)
        {
			if (!Stop && input == 0)
            {
				int newSum = Operations(DieCols[0], OldDice[0], OldDice[1], false) + Operations(DieCols[1], OldDice[1], OldDice[0], false);
				if (!RDRTS && (OldSum > newSum || (OldSum == 2 && OldSum == newSum)))
					Buttons[1].OnInteract();
				else if (!RDRTS && (OldSum < newSum || (OldSum == 12 && OldSum == newSum)))
					Buttons[0].OnInteract();
				else
					Buttons[Rnd.Range(0, 2)].OnInteract();
			}
			yield return true;
        }
    }
}
