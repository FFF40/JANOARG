using System.Linq;
using JANOARG.Client.Data.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.Behaviors.Common
{
    public class PlayerManager: MonoBehaviour
    {
        public static PlayerManager sMain;
        public PlayerTitles PlayerTitles;
        public PlayerAvatars PlayerAvatars;

        public void Awake()
        {
          sMain = this;  
        }

        public static TMP_Text LoadPlayerTitle(TMP_Text titleText)
        {
            Title title = new Title(){ID = "Perfectly Generic Player", Name = "Perfectly Generic Player"};
            string playerTitle = CommonSys.sMain.Storage.Get("INFO:PlayerTitle", "Perfectly Generic Player");
            if (sMain.PlayerTitles.Titles.Any(t => t.ID == playerTitle)){
                title = sMain.PlayerTitles.Titles.First(t => t.ID == playerTitle); 
            } else
            {
                titleText.text = "Perfectly Generic Player";
                return titleText;
            }
            if (title.Rarity.Name == "Rainbow")
            {
                titleText.ForceMeshUpdate();

                TMP_TextInfo textInfo = titleText.textInfo;

                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    TMP_CharacterInfo character = textInfo.characterInfo[i];

                    if (!character.isVisible)
                        continue;

                    // Rainbow hue based on character position
                    float hue = (float)i / textInfo.characterCount;

                    // Lower saturation, high brightness
                    Color color = Color.HSVToRGB(hue, 0.45f, 1.0f);

                    int materialIndex = character.materialReferenceIndex;
                    int vertexIndex = character.vertexIndex;

                    Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;

                    vertexColors[vertexIndex + 0] = color;
                    vertexColors[vertexIndex + 1] = color;
                    vertexColors[vertexIndex + 2] = color;
                    vertexColors[vertexIndex + 3] = color;
                }

                titleText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            }
            else
            {
                titleText.color = title.Rarity.Color;
            }

            return titleText;
        }

        
        public static Image LoadPlayerAvatar(Image avatarImage)
        {
            string playerIcon = CommonSys.sMain.Storage.Get("INFO:PlayerIcon", "none");

            avatarImage.color = playerIcon == "none"
                ? Color.black
                : Color.white;

            if (sMain.PlayerAvatars.Avatars.Any(a => a.ID == playerIcon))
            {
                var avatar = sMain.PlayerAvatars.Avatars.First(a => a.ID == playerIcon);
                avatarImage.sprite = avatar.Image;
                return avatarImage;
            } else
            {
                avatarImage.sprite = null;
                return avatarImage;
            }
        }
    }

}