using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Text.RegularExpressions;

[Serializable]
public abstract class StoryInstruction
{
    #region Static
    public virtual void OnTextBuild(Storyteller teller) { }
    public virtual void OnMusicChange(Storyteller teller) { }
    #endregion

    #region Dynamic
    public virtual IEnumerator OnTextReveal(Storyteller teller) { yield return null; }
    public virtual IEnumerator OnBackgroundChange(Storyteller teller) { yield return null; }
    public virtual IEnumerator OnActorAction(Storyteller teller) { yield return null; }
    public virtual IEnumerator OnMusicPlay(Storyteller teller) { yield return null; }
    public virtual IEnumerator OnSFXPlay(Storyteller teller) { yield return null; }
    public virtual IEnumerator OnInterfaceChange(Storyteller teller) { yield return null; }

    #endregion

    //To reduce manual labor
    public void Log(string message, Storyteller teller, [CallerMemberName] string caller = "")
    {
        Debug.LogWarning($"Chunk {teller.CurrentChunkIndex}, Class:{GetType().Name}, Function:{message} in {caller}()");
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