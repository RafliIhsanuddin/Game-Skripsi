using UnityEngine;
using UnityEngine.Playables;
using Yarn.Unity;
using System;

public class TimelineDialogueControl : MonoBehaviour
{
    public DialogueRunner dialogueRunner;
    public PlayableDirector director;
    public string yarnNodeName;

    public void PlayDialogueNode()
    {
        if (!dialogueRunner.IsDialogueRunning)
        {
            dialogueRunner.StartDialogue(yarnNodeName);
        }
    }

    public void PauseTimelineUntilDialogueComplete()
    {
        director.Pause();
        dialogueRunner.onDialogueComplete.AddListener(ResumeTimeline);
    }

    private void ResumeTimeline()
    {
        dialogueRunner.onDialogueComplete.RemoveListener(ResumeTimeline);
        director.Resume();
    }
}
