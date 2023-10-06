using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContextMenuItem : MonoBehaviour
{
    public CanvasGroup Group;
    public TMP_Text ContentLabel;
    public TMP_Text ShortcutLabel;
    public GameObject CheckedIndicator;
    public GameObject SubmenuIndicator;
    public ContextMenuButton Button;
    public Image Icon;
}
