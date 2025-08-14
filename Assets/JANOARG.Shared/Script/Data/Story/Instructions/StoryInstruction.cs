using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Text.RegularExpressions;

[Serializable]
public abstract class StoryInstruction
{
    

    #region Static
    /// <summary>
    /// This method is called when add the text to the objects
    /// </summary>
    public virtual void OnTextBuild(Storyteller teller) { }

    /// <summary>
    /// This method is called when the music in the background changes.
    /// </summary>

    public virtual void OnMusicChange(Storyteller teller) { }

    /// <summary>
    /// This method is called when adding choices to the story.
    /// </summary>
    public virtual void AddChoices(Storyteller teller) { }

    /// <summary>
    /// This method is called to check the story flags.
    /// It can be used to check if certain conditions are met before proceeding with the story.
    /// </summary>
    public virtual void CheckStoryFlags(Storyteller teller) { }

    #endregion

    #region Dynamic

    /// <summary>
    /// This method is used for revealing text in the story.
    /// </summary>
    public virtual IEnumerator OnTextReveal(Storyteller teller) { yield return null; }

    /// <summary>
    /// Changes the background of the story.
    /// </summary>
    public virtual IEnumerator OnBackgroundChange(Storyteller teller) { yield return null; }

    /// <summary>
    /// This method is called when an actor action is performed.
    /// Like changing the actor's sprite, position, etc.
    /// </summary>
    public virtual IEnumerator OnActorAction(Storyteller teller) { yield return null; }

    /// <summary>
    /// This method is called when the music is played.
    /// </summary>
    public virtual IEnumerator OnMusicPlay(Storyteller teller) { yield return null; }

    /// <summary>
    /// This method is called when a sound effect is played.
    /// </summary>
    public virtual IEnumerator OnSFXPlay(Storyteller teller) { yield return null; }

    /// <summary>
    /// This method is called when the interface is changed.
    /// It can be used to update the UI elements in the story.
    /// </summary>
    public virtual IEnumerator OnInterfaceChange(Storyteller teller) { yield return null; }

    /// <summary>
    /// This method is called when the text area is switched.
    /// It will be used to switch the text from the Dialogue Box to the Full Screen Narration.
    /// </summary>
    public virtual IEnumerator OnTextAreaSwitch(Storyteller teller) { yield return null; }

    #endregion

    // To reduce manual labor
    public void Log(string message, Storyteller teller, [CallerMemberName] string caller = "")
    {
        Debug.LogWarning($"Chunk {teller.CurrentChunkIndex}, Class:{GetType().Name}, {caller}(): {message}");
    }

    #region Parse Parameters
    public float ParseDuration(string duration)
    {
        var match = Regex.Match(duration, @"^(?<number>\d+(?:\.\d+)?)(?<unit>s|x|)$");
        if (!match.Success) throw new ArgumentException("Duration value is invalid");
        return Convert.ToSingle(match.Groups["number"].Value);
    }

    //Return true if this is scale
    public bool isDurationScaleable(string duration)
    {
        var match = Regex.Match(duration, @"^(?<number>\d+(?:\.\d+)?)(?<unit>s|x|)$");
        if (!match.Success) throw new ArgumentException("Duration value is invalid");
        return match.Groups["unit"].Value != "s";
    }

    public bool ParseBoolean(string param)
    {
        bool rt;
        if (bool.TryParse(param, out rt))
        {
            return rt; 
        }
        else
        {
            Debug.LogWarning("Failed to parse '" + param + "' into a boolean. Defaulting to false.");
            return false;
        }
    }
    #endregion
}