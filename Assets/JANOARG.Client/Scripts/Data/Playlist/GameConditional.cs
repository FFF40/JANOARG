


using System;
using System.Collections.Generic;
using System.Linq;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Utils;
using UnityEditor;

namespace JANOARG.Client.Data.Playlist
{
    [Serializable]
    public abstract class GameConditional
    {
        public abstract bool Test();

        public static bool TestAll(IEnumerable<GameConditional> conditionals)
        {
            return conditionals.All(x => x.Test());
        }
    }

    [Serializable]
    public class ScoreStoreGameConditional : GameConditional
    {
        public string SongID;
        public AchievementReq Achievement = AchievementReq.Cleared;
        public DifficultyReq Difficulty = DifficultyReq.Any;

        public override bool Test()
        {
            return StorageManager.sMain.Scores.entries.Any(x =>
                x.Value.SongID == SongID && (Achievement switch
                {
                    AchievementReq.Cleared => x.Value.Score >= Helper.PASSING_SCORE,
                    AchievementReq.FullCombo => x.Value.BadCount == 0,
                    AchievementReq.AllPerfect => x.Value.BadCount == 0 && x.Value.GoodCount == 0,
                    _ => true,
                }) && (Difficulty switch
                {
                    _ => true,
                })
            );
        }

        public enum AchievementReq
        {
            Played,
            Cleared,
            FullCombo,
            AllPerfect
        }

        public enum DifficultyReq
        {
            Any,
        }
    }

    [Serializable]
    public class FlagStoreGameConditional : GameConditional
    {
        public string Flag;

        public override bool Test()
        {
            return StorageManager.sMain.Flags.Test(Flag);
        }
    }
}