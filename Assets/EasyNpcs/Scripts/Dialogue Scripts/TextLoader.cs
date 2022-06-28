using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Text_Loader
{
    public struct DialogueText
    {
        public Job Job;
        public Gender Gender;
        public List<string> Text;

        public DialogueText(List<string> list)
        {
            Text = list;
            Job = Job.Default;
            Gender = Gender.Default;
        }
    }

    public class TextLoader : MonoBehaviour
    {
        [SerializeField]
        private string path;

        private string currentFile;
        public static List<List<DialogueText>> dialogueTexts {get; private set;}

        void Start()
        {
            dialogueTexts = new List<List<DialogueText>>();
            var textFiles = FindAllTextFiles();

            foreach (var textFile in textFiles)
            {
                currentFile = textFile;
                dialogueTexts.Add(FillDialogueText());
            }
        }
        
        IEnumerable<string> FindAllTextFiles()
        {
            var ext = new List<string> { "txt" };
            var texts = Directory
                .EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                .Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));

            return texts;
        }

        List<DialogueText> FillDialogueText()
        {
            var strings = Split_Text_By_Lines(currentFile);

            return new List<DialogueText>() { Fill(strings), Fill(strings) };
        }

        static List<string> Split_Text_By_Lines(string path)
        {
            StreamReader reader = new StreamReader(path);
            string text = reader.ReadToEnd();

            return text.Split('\n').ToList();
        }

        DialogueText Fill(List<string> strings)
        {
            DialogueText text = new DialogueText(new List<string>());

            Parse_Gender_Job(FindTagString(ref strings), out text.Gender);
            Parse_Gender_Job(FindTagString(ref strings), out text.Job);

            Check_End_Of_Npc_Section(text, strings);

            return text;
        }

        void Check_End_Of_Npc_Section(DialogueText text, List<string> strings)
        {
            for (int i = 0; i < strings.Count; i++)
            {
                if (strings[i][0] == '!')
                    break;
                else
                {
                    text.Text.Add(strings[i]);
                    strings.RemoveAt(i);
                    i--;
                }
            }
        }

        void Parse_Gender_Job<TEnum>(string tag, out TEnum enumerator) where TEnum : struct
        {
            if (!Enum.TryParse(tag.Remove(0, 1), out enumerator))
            {
                Debug.LogError("Wrong " + typeof(TEnum).ToString() + " type name in file: " + currentFile);
            }
        }

        string FindTagString(ref List<string> strings)
        {
            for (int i = 0; i < strings.Count; i++)
            {
                var result = strings[i];
                strings.RemoveAt(i);

                if (result[0] == '!')
                {
                    return result;
                }
            }

            string error = "Wrong dialogue file format in file: " + path;
            Debug.LogError(error);
            throw new Exception(error);
        }

        public static Tuple<List<string>, List<string>> GetDialgoue(Gender[] genders = null, Job[] jobs = null)
        {
            genders = genders ?? new Gender[] { Gender.Default, Gender.Default };
            jobs = jobs ?? new Job[] { Job.Default, Job.Default };

            List<List<DialogueText>> validTexts = Parse_DialogueTexts(genders, jobs);
            
            if (validTexts.Count < 1)
                return null;

            System.Random random = new System.Random();
            var chosenText = validTexts[random.Next(0, validTexts.Count - 1)];

            return new Tuple<List<string>, List<string>>(chosenText[0].Text, chosenText[1].Text);
        }

        static List<List<DialogueText>> Parse_DialogueTexts(Gender[] genders, Job[] jobs)
        {
            List<List<DialogueText>> validTexts = new List<List<DialogueText>>();
            foreach (var text in dialogueTexts)
            {
                bool isValid = true;
                for (int i = 0; i < 2; i++)
                {
                    if (!jobs[i].HasFlag(text[i].Job) || !genders[i].HasFlag(text[i].Gender))
                    {
                        isValid = false;
                    }
                }
                if (isValid)
                {
                    validTexts.Add(text);
                }
            }

            return validTexts;
        }
    }

    public class FindDialogue
    {
        public static Tuple<List<string>, List<string>> GetDialgoue(Gender[] genders = null, Job[] jobs = null)
        {
            genders = genders ?? new Gender[] { Gender.Default, Gender.Default };
            jobs = jobs ?? new Job[] { Job.Default, Job.Default };

            List<List<DialogueText>> validTexts = Parse_DialogueTexts(genders, jobs);
            
            if (validTexts.Count < 1)
                return null;

            System.Random random = new System.Random();
            var chosenText = validTexts[random.Next(0, validTexts.Count - 1)];

            return new Tuple<List<string>, List<string>>(chosenText[0].Text, chosenText[1].Text);
        }

        static List<List<DialogueText>> Parse_DialogueTexts(Gender[] genders, Job[] jobs)
        {
            List<List<DialogueText>> validTexts = new List<List<DialogueText>>();
            foreach (var text in TextLoader.dialogueTexts)
            {
                bool isValid = true;
                for (int i = 0; i < 2; i++)
                {
                    if (!jobs[i].HasFlag(text[i].Job) || !genders[i].HasFlag(text[i].Gender))
                    {
                        isValid = false;
                    }
                }
                if (isValid)
                {
                    validTexts.Add(text);
                }
            }

            return validTexts;
        }
    }
}