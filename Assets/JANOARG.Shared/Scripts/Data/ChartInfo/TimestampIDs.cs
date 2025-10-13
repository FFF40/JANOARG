// ReSharper disable InconsistentNaming
namespace JANOARG.Shared.Data.ChartInfo
{
    public enum TimestampIDs
    {
        #region CameraController

        #region  Pivots
        CameraPivot_X,
        CameraPivot_Y,
        CameraPivot_Z,
        PivotDistance,
        #endregion

        #region Camera Rotation
        CameraRotation_X,
        CameraRotation_Y,
        CameraRotation_Z,
        #endregion

        #endregion


        #region Palette

        #region Background Color (RGB)
        BackgroundColor_R,
        BackgroundColor_G,
        BackgroundColor_B,
        #endregion

        #region Interface Color (RGBA)
        InterfaceColor_R,
        InterfaceColor_G,
        InterfaceColor_B,
        InterfaceColor_A,
        #endregion

        #endregion


        #region LaneStyle

        #region Lane Color (RGBA)
        LaneColor_R,
        LaneColor_G,
        LaneColor_B,
        LaneColor_A,
        #endregion

        #region Judgeline Color (RGBA)
        JudgeColor_R,
        JudgeColor_G,
        JudgeColor_B,
        JudgeColor_A,
        #endregion

        #endregion


        #region HitStyle

        #region Tap Note Color (RGBA)
        NormalColor_R,
        NormalColor_G,
        NormalColor_B,
        NormalColor_A,
        #endregion

        #region Catch Note Color (RGBA)
        CatchColor_R,
        CatchColor_G,
        CatchColor_B,
        CatchColor_A,
        #endregion

        #region Hold Note Color (Tail) (RGBA)
        HoldTailColor_R,
        HoldTailColor_G,
        HoldTailColor_B,
        HoldTailColor_A,
        #endregion

        #endregion


        #region LaneGroup

        #region Position (Vec3)
        Position_Y,
        Position_Z,
        Position_X,
        #endregion

        #region Rotation (Vec3)
        Rotation_X,
        Rotation_Y,
        Rotation_Z,
        #endregion

        #endregion


        #region Lane

        #region Position/Offset (Vec3)
        Offset_X,
        Offset_Y,
        Offset_Z,
        #endregion

        #region Rotation/Offset (Vec3)
        OffsetRotation_X,
        OffsetRotation_Y,
        OffsetRotation_Z,
        #endregion

        #endregion


        #region LaneStep

        #region Start Position (Vec3)
        StartPos_X,
        StartPos_Y,
        StartPos_Z,
        #endregion

        #region End Position (Vec3)
        EndPos_X,
        EndPos_Y,
        EndPos_Z,
        #endregion

        Speed,

        #endregion


        #region HitObject

        Position,
        Length,

        #endregion

        #region Text

            TextColor_R,
            TextColor_G,
            TextColor_B,
            TextColor_A,

            TextSize,

            TextFont,

        #endregion

        #region World Objects
            ObjectPosition_X,
            ObjectPosition_Y,
            ObjectPosition_Z,

            ObjectRotation_X,
            ObjectRotation_Y,
            ObjectRotation_Z,
        #endregion


    }
}