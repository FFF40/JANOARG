using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class VersionCheckerModal : Modal
{
    public static VersionCheckerModal main;
    public static Coroutine Checker;

    public static ReleaseData data;
    public ReleaseItem LatestRelease;
    public ReleaseAsset LatestAsset;

    public TMP_Text TitleText;
    public TMP_Text SummaryText;
    public TMP_Text VersionText;

    public GameObject DescriptionBox;
    public TMP_Text DescriptionText;

    public Button OkButton;
    public Button RemindLaterButton;
    public Button DownloadButton;
    public TMP_Text DownloadText;

    public void Awake()
    {
        if (main) Close();
        else main = this;
    }

    public new void Start() 
    {
        base.Start();

        TitleText.text = SummaryText.text = VersionText.text = "";
        DescriptionBox.SetActive(false);
        OkButton.gameObject.SetActive(true);
        RemindLaterButton.gameObject.SetActive(false);
        DownloadButton.gameObject.SetActive(false);

        LatestRelease = data?.Data?.Find(x => x.name.StartsWith("chartmaker-v"));

        if (LatestRelease == null)
        {
            TitleText.text = "Error";
            SummaryText.text = "Server responded with empty or invalid release data.";
            return;
        }

        VersionText.text = 
              "Current version number: v" + Application.version +
            "\nLatest version number:  v" + LatestRelease.name[12..];
        
        Version.TryParse(Application.version, out Version currentVer);
        Version.TryParse(LatestRelease.name[12..], out Version remoteVer);

        if (currentVer == remoteVer) 
        {
            TitleText.text = "Version Checker";
            SummaryText.text = "You're using the latest version of the JANOARG Chartmaker!";
        }
        else if (currentVer < remoteVer) 
        {
            TitleText.text = "New version available!";
            SummaryText.text = 
                "A new version of the JANOARG Chartmaker is available!" +
              "\nWould you like to download it now?";
              
            DescriptionBox.SetActive(true);
            DescriptionText.text = LatestRelease.body;
            DescriptionText.text = new Regex(@"\*\*(.+)\*\*", RegexOptions.Multiline).Replace(DescriptionText.text, "<b>$1</b>");
            DescriptionText.text = new Regex(@"\*(.+)\*", RegexOptions.Multiline).Replace(DescriptionText.text, "<i>$1</i>");

            string match = Application.platform switch {
                RuntimePlatform.WindowsPlayer => "Chartmaker-win64.zip",
                _ => "Chartmaker-win64.zip",
            };

            LatestAsset = LatestRelease.assets.Find(x => x.name == match);
            LatestAsset ??= LatestRelease.assets[0];

            OkButton.gameObject.SetActive(false);
            RemindLaterButton.gameObject.SetActive(true);
            DownloadButton.gameObject.SetActive(true);
            DownloadText.text = "Download  <alpha=#80>" + (LatestAsset.size / 1048576f).ToString("#.00") + "MiB";
        }
        else 
        {
            TitleText.text = "Version Checker";
            SummaryText.text = "You're having the very super secret dev build of the JANOARG Chartmaker!";
        }
    }

    public static void InitFetch(bool silent = false)
    {
        Checker ??= Chartmaker.main.StartCoroutine(Fetch(silent));
    }

    public static IEnumerator Fetch(bool silent = false)
    {
        UnityWebRequest request = UnityWebRequest.Get("https://api.github.com/repos/FFF40/JANOARG/releases");
        yield return request.SendWebRequest();

        if (request.responseCode == 200) 
        {
            try 
            {
#if UNITY_EDITOR
                Debug.Log(request.downloadHandler.text);
#endif
                data = JsonUtility.FromJson<ReleaseData>("{\"Data\":" + request.downloadHandler.text + "}");
                if (silent) 
                {
                    ReleaseItem latestRelease = data?.Data?.Find(x => x.name.StartsWith("chartmaker-v"));
                    
                    Version.TryParse(Application.version, out Version currentVer);
                    Version.TryParse(latestRelease.name[12..], out Version remoteVer);

                    if (currentVer >= remoteVer) goto silentSkip;
                }
                if (main) main.Close();
                ModalHolder.main.Spawn<VersionCheckerModal>();
            }
            catch (Exception e)
            {
                if (silent) goto silentSkip;
                DialogModal modal = ModalHolder.main.Spawn<DialogModal>();
                modal.SetDialog("Error", 
                    "Couldn't parse response from server:\n" + e.ToString(), 
                    new string[] {"Ok"}, _ => {});
            }
        }
        else
        {
            if (silent) goto silentSkip;
            DialogModal modal = ModalHolder.main.Spawn<DialogModal>();
            modal.SetDialog("Error", 
                "Server responded with a " + request.responseCode + " status code.", 
                new string[] {"Ok"}, _ => {});
        }

        // goto statements in c#, let's gooooooooo
        silentSkip:
        Checker = null;
    }
}

[Serializable]
public class ReleaseData
{
    public List<ReleaseItem> Data = new();
}

[Serializable]
public class ReleaseItem
{
    public string name = "";
    public string body = "";
    public List<ReleaseAsset> assets = new();
}

[Serializable]
public class ReleaseAsset
{
    public string name = "";
    public ulong size = 0;
    public string browser_download_url = "";
}

