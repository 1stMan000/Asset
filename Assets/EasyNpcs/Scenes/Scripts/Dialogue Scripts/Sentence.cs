using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sentence.asset", menuName = "Sentence")]
public class Sentence : ScriptableObject
{
    public delegate void GoalCompletedHandler();
    GoalCompletedHandler goalHandlers;

    public string playerText;
    public string npcText;

    public Sentence nextSentence;
    public List<Sentence> choices; 
}
