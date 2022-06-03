using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Text_Loader
{
    struct DialogueText
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

        public DialogueText(Job job, Gender gender, List<string> firstText)
        {
            Job = job;
            Gender = gender;
            Text = firstText;
        }
    }

    public class TextLoader : MonoBehaviour
    {
        // Assign in Inspector
        // Path to the folder with npc-npc dialogues.
        [SerializeField]
        private string path;

        private string currentFile;
        private static List<List<DialogueText>> dialogueTexts;

        // Start is called before the first frame update
        void Start()
        {
            dialogueTexts = new List<List<DialogueText>>();

            // Get pathes of all .txt files in directory
            var ext = new List<string> { "txt" };
            var names = Directory
                .EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                .Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));

            // Iterate through all the files and read dialogues to the list
            foreach (var name in names)
            {
                currentFile = name;
                dialogueTexts.Add(FillDialogueText());
            }
        }

        List<DialogueText> FillDialogueText()
        {
            var strings = ReadText(currentFile);

            return new List<DialogueText>() { Fill(strings), Fill(strings) };
        }

        /// <summary>
        /// Reads text from the given file and return a list of strings.
        /// </summary>
        /// <param name="path">
        /// The path to the file
        /// </param>
        /// <returns>
        /// Returns a List of strings
        /// </returns>
        public static List<string> ReadText(string path)
        {
            StreamReader reader = new StreamReader(path);
            string text = reader.ReadToEnd();

            return text.Split('\n').ToList();
        }

        DialogueText Fill(List<string> strings)
        {
            DialogueText text = new DialogueText(new List<string>());

            // Get tags
            ParseText(FindTagString(ref strings), out text.Gender);
            ParseText(FindTagString(ref strings), out text.Job);

            // Get text till a tag string is found or end of the file is reached
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

            return text;
        }

        // Erases strings without tags (!enum),
        // till a tag string is found, throws exception otherwise
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

        // Parse Enum from a string
        // String must have structure ( ! before enum)
        // "!TEnum"
        void ParseText<TEnum>(string tag, out TEnum enumerator) where TEnum : struct
        {
            if (!Enum.TryParse(tag.Remove(0, 1), out enumerator))
            {
                Debug.LogError("Wrong " + typeof(TEnum).ToString() + " type name in file: " + currentFile);
            }
        }

        /// <summary>
        /// Get a random dialogue with the given tags.
        /// </summary>
        /// <param name="gender">
        /// An array of Gender enums, where first element is used for first dialogue, second - for second.
        /// </param>
        /// <param name="job">
        /// An array of Job enums, where first element is used for first dialogue, second - for second.
        /// </param>
        /// <returns>
        /// Returns a Tuple with two dialogues.
        /// </returns>
        public static Tuple<List<string>, List<string>> GetDialgoue(Gender[] gender = null, Job[] job = null)
        {
            // Fill arrays with Default values, if arrays are null
            gender = gender ?? new Gender[] { Gender.Default, Gender.Default };
            job = job ?? new Job[] { Job.Default, Job.Default };

            List<List<DialogueText>> list = new List<List<DialogueText>>();
            List<List<DialogueText>> validTexts = new List<List<DialogueText>>();
            // Fills the list with dialogues with suitable tags
            foreach (var text in dialogueTexts)
            {
                bool isValid = true;
                for (int i = 0; i < 2; i++)
                {
                    // If dialogue doesn't have needed tag, it is not valid and not included into the list
                    if (!job[i].HasFlag(text[i].Job) || !gender[i].HasFlag(text[i].Gender))
                    {
                        isValid = false;
                    }
                }
                if (isValid)
                {
                    validTexts.Add(text);
                }
            }

            System.Random random = new System.Random();
            list.Add(validTexts[random.Next(0, validTexts.Count)]);

            if (list.Count < 1)
                return null;

            // Pick random dialogue from the list
            var chosenText = list[random.Next(0, list.Count)];

            return new Tuple<List<string>, List<string>>(chosenText[0].Text, chosenText[1].Text);
        }

        /// <summary>
        /// Reads 2 dialogues from the given path. Dialogues must be divided by a line "{}"
        /// </summary>
        /// <param name="path">
        /// The path to file to read text from.
        /// </param>
        /// <returns>
        /// Returns a tuple, where first element is first dialogue, second element - second dialogue.
        /// </returns>
        public static Tuple<List<string>, List<string>> DialogueWithoutTags(string path)
        {
            var text = ReadText(path);
            var firstList = new List<string>();
            var secondList = new List<string>();

            bool reached = false;

            // Fills first list with the text till "{}" line is found, then fills second list 
            foreach (var str in text)
            {
                if (str[0] == '{' && str[1] == '}')
                {
                    reached = true;
                    continue;
                }

                List<string> list;
                if (reached)
                    list = secondList;
                else
                    list = firstList;

                list.Add(str);
            }

            return new Tuple<List<string>, List<string>>(firstList, secondList);
        }
    }
}