using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Player
{
    public class LaneGroupPlayer : MonoBehaviour
    {
        public LaneGroup Original;
        public LaneGroup Current;

        public LaneGroupPlayer Parent;

        public void UpdateSelf(float time, float beat)
        {
            if (Current != null)
                Current.Advance(beat);
            else
                Current = (LaneGroup)Original.GetStoryboardableObject(beat);

            transform.localPosition = Current.Position;
            transform.localEulerAngles = Current.Rotation;
        }
    }
}
