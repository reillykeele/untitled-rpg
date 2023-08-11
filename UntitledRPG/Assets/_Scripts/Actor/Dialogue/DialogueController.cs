using System;
using ReiBrary.Extensions;
using ReiBrary.Singleton;
using UnityEngine;
using Yarn.Unity;

namespace UntitledRPG.Actor.Dialogue
{
    public class DialogueController : Singleton<DialogueController>
    {
        private DialogueRunner _runner;

        void Start()
        {
            _runner = FindObjectOfType<DialogueRunner>();

            // _runner.Di
        }

        public void StartConversation(string startNode)
        {
            if (startNode.IsNullOrEmpty())
            {
                Debug.LogWarning($"Trying to start an empty Yarn node: \"{startNode}\"");
                return;
            }

            _runner.StartDialogue(startNode);
        }
    }
}
