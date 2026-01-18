using UnityEditor;
using static JANOARG.Client.Editor.Debugging.AutoplayToggle;

namespace JANOARG.Client.Editor.Debugging
{
    using UnityEditor;

    [InitializeOnLoad]
    public static class AutoplayToggle {

        private const string _MENU_NAME = "JANOARG/Enable Autoplay";

        public static bool sAutoplayEditorEnabled;

        /// Called on load thanks to the InitializeOnLoad attribute
        static AutoplayToggle() {

            sAutoplayEditorEnabled = EditorPrefs.GetBool(_MENU_NAME, false);

            // Delaying until first editor tick so that the menu
            // will be populated before setting check state, and
            // re-apply correct action
            EditorApplication.delayCall += () => {
                PerformAction(sAutoplayEditorEnabled);
            };
        }

        [MenuItem(_MENU_NAME, priority = 1000)]
        private static void ToggleAction() {

            // Toggling action
            PerformAction( !sAutoplayEditorEnabled);
        }

        public static void PerformAction(bool enabled) {

            // Set checkmark on menu item
            Menu.SetChecked(_MENU_NAME, enabled); 
            
            // Saving editor state
            EditorPrefs.SetBool(_MENU_NAME, enabled);

            sAutoplayEditorEnabled = enabled;

        }
    }
}