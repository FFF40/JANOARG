using System.Collections.Generic;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Shared.Data.ChartInfo;
using JANOARG.Shared.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace JANOARG.Client.Behaviors.Player
{
    public class HitPlayer : MonoBehaviour
    {
        public HitObject Original;
        public HitObject Current;


        public float       Time;
        public float       EndTime;
        public List<float> HoldTicks;
        public float       CurrentPosition;

        public MeshRenderer Center;
        [FormerlySerializedAs("Left")]
        public MeshRenderer LeftPoint;
        [FormerlySerializedAs("Right")]
        public MeshRenderer RightPoint;

        public MeshFilter   HoldMesh;
        public MeshRenderer HoldRenderer;

        public MeshFilter   FlickMesh;
        public MeshRenderer FlickRenderer;

        public LanePlayer     Lane;
        public HitScreenCoord HitCoord;

        public bool           IsSimultaneous;
        public MeshRenderer   SimultaneousHighlight;
        public SpriteRenderer SimultaneousGlow;
        

        public bool InDiscreteHitQueue;

        public bool PendingHoldQueue;
        public bool IsProcessed;
        public bool IsTapped;

        public void Init()
        {
            if (Current.StyleIndex >= 0 && Current.StyleIndex < PlayerScreen.sMain.HitStyles.Count)
            {
                HitStyleManager style = PlayerScreen.sMain.HitStyles[Current.StyleIndex];

                LeftPoint.sharedMaterial =
                    RightPoint.sharedMaterial =
                        style.NormalMaterial;

                Center.sharedMaterial =
                    Current.Type == HitObject.HitType.Catch
                        ? style.CatchMaterial : style.NormalMaterial;

                if (HoldRenderer)
                    HoldRenderer.sharedMaterial = style.HoldTailMaterial;

                IsSimultaneous = Original.IsSimultaneous;

                SimultaneousHighlight.gameObject.SetActive(IsSimultaneous);

                if (IsSimultaneous && SimultaneousHighlight.gameObject.activeSelf)
                {
                    SimultaneousHighlight.sharedMaterial = 
                        Current.Type == HitObject.HitType.Catch 
                            ? style.CatchHighlightMaterial : style.NormalHighlightMaterial;
                    SimultaneousGlow.sharedMaterial = 
                        Current.Type == HitObject.HitType.Catch 
                            ? style.CatchHighlightGlowMaterial : style.NormalHighlightGlowMaterial;
                }

                if (Current.Flickable)
                {
                    FlickMesh.gameObject.SetActive(true);

                    FlickMesh.sharedMesh = float.IsFinite(Current.FlickDirection)
                        ? PlayerScreen.sMain.ArrowFlickIndicator : PlayerScreen.sMain.FreeFlickIndicator;

                    FlickRenderer.sharedMaterial = Center.sharedMaterial;
                }
            }
            else
            {
                Center.enabled =
                    LeftPoint.enabled =
                        RightPoint.enabled = false;
            }

            UpdateMesh();
        }

        public void UpdateSelf(float time, float beat, bool forceDirty = false)
        {
            if (Current != null)
                Current.Advance(beat);
            else
                Current = (HitObject)Original.GetStoryboardableObject(beat);

            if (Current.IsDirty || forceDirty || IsProcessed)
            {
                UpdateMesh();
                Current.IsDirty = false;
            }

            if (FlickMesh.gameObject.activeSelf)
            {
                Quaternion rotation = CommonSys.sMain.MainCamera.transform.rotation;

                float angle = float.IsFinite(Current.FlickDirection)
                    ? Current.FlickDirection
                    : Vector2.SignedAngle(
                        Vector2.right,
                        CommonSys.sMain.MainCamera.WorldToScreenPoint(LeftPoint.transform.position) -
                        CommonSys.sMain.MainCamera.WorldToScreenPoint(RightPoint.transform.position));

                FlickMesh.transform.rotation = rotation * Quaternion.Euler(0, 0, angle);
            }
        }

        public void UpdateMesh()
        {
            float time = Mathf.Max(Time, PlayerScreen.sMain.CurrentTime + PlayerScreen.sMain.Settings.VisualOffset);
            float zPosition;

            try
            {
                zPosition = CurrentPosition = Lane.GetZPosition(time);
            }
            catch
            {
                return;
            }

            Lane.GetStartEndPosition(time, out Vector2 start, out Vector2 end);

            transform.localPosition = Vector3.LerpUnclamped(start, end, Current.Position + Current.Length / 2) +
                                      Vector3.forward * zPosition;

            transform.localEulerAngles = Vector3.forward * Vector2.SignedAngle(Vector2.right, end - start);


            float width = Vector2.Distance(start, end) * Current.Length;

            if (Current.Type == HitObject.HitType.Catch)
            {
                float scale = PlayerScreen.sMain.Settings.HitObjectScale[1];
                Center.transform.localScale = new Vector3(width, .2f * scale, .2f * scale);
                SimultaneousHighlight.transform.localScale = new Vector3(width + .2f * scale, .3f * scale, .3f * scale);
                SimultaneousGlow.transform.localScale *= new Vector3Frag(y: SimultaneousHighlight.transform.localScale.y * 12f);
                
                LeftPoint.transform.localScale = RightPoint.transform.localScale = new Vector3(.2f, .4f, .4f) * scale;
                RightPoint.transform.localPosition = Vector3.right * (width / 2);
                LeftPoint.transform.localPosition = -RightPoint.transform.localPosition;
            }
            else
            {
                float scale = PlayerScreen.sMain.Settings.HitObjectScale[0];
                Center.transform.localScale = new Vector3(width - .2f * scale, .4f * scale, .4f * scale);
                SimultaneousHighlight.transform.localScale = new Vector3(width + .2f * scale, .6f * scale, .6f * scale);
                SimultaneousGlow.transform.localScale *= new Vector3Frag(y: SimultaneousHighlight.transform.localScale.y * 6f);
                LeftPoint.transform.localScale = RightPoint.transform.localScale = new Vector3(.2f, .4f, .4f) * scale;
                RightPoint.transform.localPosition = Vector3.right * (width / 2 + .2f * scale);
                LeftPoint.transform.localPosition = -RightPoint.transform.localPosition;
            }
        }
    }
}
