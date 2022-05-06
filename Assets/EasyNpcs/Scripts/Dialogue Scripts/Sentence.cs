using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sentence : MonoBehaviour
{
    public delegate void GoalCompletedHandler();
    GoalCompletedHandler goalHandlers;

    public string text;
    public string answer;

    public Sentence nextSentence;
    public List<Sentence> choices; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
