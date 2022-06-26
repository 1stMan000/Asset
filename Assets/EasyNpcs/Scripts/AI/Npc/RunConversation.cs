using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Npc_AI;
using Text_Loader;
using Rotation;

public class RunConversation : MonoBehaviour
{
    bool first;
    NpcAI me;
    NpcAI partner;
    Tuple<List<string>,List<string>> conversation = null;

    Rotate rotate;

    private void Start()
    {
        me.ChangeState(NpcState.Talking);
    }

    public void Set(bool order, NpcAI npcMe, NpcAI npcPartner, Tuple<List<string>, List<string>> conver = null)
    {
        first = order;
        me = npcMe;
        partner = npcPartner;
        conversation = conver;
    }

    public void StartConversation()
    {
        rotate = gameObject.AddComponent<Rotate>();
        rotate.RotateTo(partner.gameObject);

        if (first)
        {
            If_First();
        }
    }

    void If_First()
    {
        Tuple<List<string>, List<string>> chosenConv = Choose_Conversation();
        if (chosenConv != null)
        {
            StartCoroutine(Speak_First_Lines(chosenConv));
        }
    }

    Tuple<List<String>, List<String>> Choose_Conversation()
    {
        if (conversation == null)
        {
            return ChooseConversation();
        }
        else
        {
            return conversation;
        }
    }

    Tuple<List<string>, List<string>> ChooseConversation()
    {
        Job[] jobs = { me.job, partner.job };
        Gender[] genders = { me.gender, partner.gender };

        return TextLoader.GetDialgoue(genders, jobs);
    }

    IEnumerator Speak_First_Lines(Tuple<List<string>, List<string>> chosenConv)
    {
        StartCoroutine(Talk(chosenConv.Item1));
        yield return new WaitForSeconds(4);

        RunConversation partnerConv = partner.gameObject.AddComponent<RunConversation>();
        partnerConv.Set(false, partner, me);
        partnerConv.StartConversation();
        partnerConv.RecieveRequest(chosenConv);
    }

    public IEnumerator Talk(List<string> text)
    {
        for (int i = 0; i < text.Count; i++)
        {
            if (!text[i].StartsWith(" "))
            {
                me.Text.text = text[i];
                yield return new WaitForSeconds(4);
                if (i != text.Count - 1)
                {
                    me.Text.text = null;
                    yield return new WaitForSeconds(4);
                }
            }
        }
        
        me.ChangeState(NpcState.Idle);
    }

    void RecieveRequest(Tuple<List<string>, List<string>> chosenConv)
    {
        StartCoroutine(Talk(chosenConv.Item2));
    }

    private void OnDestroy()
    {
        Destroy(rotate);
    }
}
