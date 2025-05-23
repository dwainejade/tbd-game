using UnityEngine;

public enum Emotion
{
    Curiosity,
    Frustration,
    Fear,
    Joy,
    Sadness,
    Skepticism,
    Anger,
    Guilt,
    Love,
    Loneliness
}

[System.Serializable]
public class ContextualActionSet
{
    public Emotion emotion;
    public string sceneId;

    public string perceptionAction;
    public string physicalAction;
    public string communicationAction;
    public string specialAction;
}