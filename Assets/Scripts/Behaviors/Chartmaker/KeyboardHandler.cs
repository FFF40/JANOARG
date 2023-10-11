using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class KeyboardHandler : MonoBehaviour
{
    public static KeyboardHandler main;

    public KeybindActionList Keybindings = new KeybindActionList {
        { "FL:New", new KeybindAction {
            Category = "File",
            Name = "New Song",
            Keybind = new Keybind(KeyCode.N, EventModifiers.Command),
            Invoke = () => ModalHolder.main.Spawn<NewSongModal>(),
        }},
        { "FL:Open", new KeybindAction {
            Category = "File",
            Name = "Open Song",
            Keybind = new Keybind(KeyCode.O, EventModifiers.Command),
            Invoke = () => Chartmaker.main.OpenSongModal(),
        }},
        { "FL:Save", new KeybindAction {
            Category = "File",
            Name = "Save Song",
            Keybind = new Keybind(KeyCode.S, EventModifiers.Command),
            Invoke = () => Chartmaker.main.StartSaveRoutine(),
        }},
        { "ED:Cut", new KeybindAction {
            Category = "Edit",
            Name = "Cut",
            Keybind = new Keybind(KeyCode.X, EventModifiers.Command),
            Invoke = () => Chartmaker.main.Cut(),
        }},
        { "ED:Copy", new KeybindAction {
            Category = "Edit",
            Name = "Copy",
            Keybind = new Keybind(KeyCode.C, EventModifiers.Command),
            Invoke = () => Chartmaker.main.Copy(),
        }},
        { "ED:Paste", new KeybindAction {
            Category = "Edit",
            Name = "Paste",
            Keybind = new Keybind(KeyCode.V, EventModifiers.Command),
            Invoke = () => Chartmaker.main.Paste(),
        }},
        { "ED:Undo", new KeybindAction {
            Category = "Edit",
            Name = "Undo",
            Keybind = new Keybind(KeyCode.X, EventModifiers.Command),
            Invoke = () => Chartmaker.main.Undo(),
        }},
        { "ED:Redo", new KeybindAction {
            Category = "Edit",
            Name = "Redo",
            Keybind = new Keybind(KeyCode.Y, EventModifiers.Command),
            Invoke = () => Chartmaker.main.Redo(),
        }},
    };
    
    public void Awake() 
    {
        main = this;
    }

    public void Start() 
    {
        Keybindings.LoadKeys();
    }

    public void OnGUI()
    {
        if (EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>())
        {
            return;
        }
        if (Event.current.isKey)
        {
            Keybindings.HandleEvent(Event.current);
        }
    }
}
