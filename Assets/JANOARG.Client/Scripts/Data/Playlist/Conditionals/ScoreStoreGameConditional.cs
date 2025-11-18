


using System;
using System.Linq;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Behaviors.SongSelect;
using JANOARG.Client.Behaviors.SongSelect.List;
using JANOARG.Client.Utils;
using JANOARG.Shared.Data.ChartInfo;

namespace JANOARG.Client.Data.Playlist.Conditionals
{
    [Serializable]
    public class ScoreStoreGameConditional : GameConditional
    {
        public string SongID;
        public AchievementReq Achievement = AchievementReq.Cleared;
        public float AchievementThreshold = 0;
        public DifficultyReq Difficulty = DifficultyReq.Any;
        public int DifficultyThreshold;

        public override bool Test()
        {
            return StorageManager.sMain.Scores.entries.Any(x =>
                x.Value.SongID == SongID && (Achievement switch
                {
                    AchievementReq.Cleared         => x.Value.Score >= Helper.PASSING_SCORE,
                    AchievementReq.FullCombo       => x.Value.BadCount == 0,
                    AchievementReq.AllPerfect      => x.Value.BadCount == 0 && x.Value.GoodCount == 0,
                    AchievementReq.ScoreThreshold  => x.Value.Score >= AchievementThreshold,
                    AchievementReq.ComboThreshold  => x.Value.MaxCombo >= AchievementThreshold,
                    AchievementReq.BadThreshold    => x.Value.BadCount <= AchievementThreshold,
                    _                              => true,
                }) && (Difficulty switch
                {
                    DifficultyReq.Any           => true,
                    DifficultyReq.SpecialOnly   => x.Value.ChartIndex < 0,
                    DifficultyReq.This          => x.Value.ChartIndex == DifficultyThreshold,
                    DifficultyReq.ThisOrHigher  => x.Value.ChartIndex >= DifficultyThreshold,
                    _                           => true,
                })
            );
        }

        public override string GetDisplayInstructionString()
        {

            PlayableSong info = SongSelectScreen.sMain.PlayableSongByID[SongID];

            string achievementString = Achievement switch
            {
                AchievementReq.Played         => "Complete",
                AchievementReq.Cleared        => $"Reach <b>{Helper.PASSING_SCORE}ppm</b> on",
                AchievementReq.FullCombo      => $"Achieve FULL STREAK on",
                AchievementReq.AllPerfect     => $"Achieve ALL FLAWLESS on",
                AchievementReq.ScoreThreshold => $"Reach {AchievementThreshold:F0}ppm on",
                AchievementReq.ComboThreshold => $"Reach {AchievementThreshold:F0} LONGEST STREAK on",
                AchievementReq.BadThreshold   => $"Achieve {AchievementThreshold:F0} or less BROKEN on",
                _                             => throw new Exception("Unknown achievement " + Achievement)
            };

            string difficultyString = Difficulty switch
            {
                DifficultyReq.Any          => "in any difficulty",
                DifficultyReq.SpecialOnly  => "in any pearl-colored diffculty",
                DifficultyReq.This         => $"in <b>{info.Charts.Single(x => x.DifficultyIndex == DifficultyThreshold).DifficultyName.ToUpper()}</b> difficulty",
                DifficultyReq.ThisOrHigher => $"in <b>{info.Charts.Single(x => x.DifficultyIndex == DifficultyThreshold).DifficultyName.ToUpper()}</b> difficulty or higher",
            };

            return $"{achievementString} <b>{info.SongName}</b> {difficultyString}";
        }

        public enum AchievementReq
        {
            Played          = 0,
            Cleared         = 1,
            FullCombo       = 2,
            AllPerfect      = 3,
            ScoreThreshold  = 0 | UseThreshold,
            ComboThreshold  = 1 | UseThreshold,
            BadThreshold    = 2 | UseThreshold,

            UseThreshold    = 1 << 31
        }

        public enum DifficultyReq
        {
            Any           = 0,
            SpecialOnly   = 1,
            This          = -1,
            ThisOrHigher  = -2,
        }
    }
}