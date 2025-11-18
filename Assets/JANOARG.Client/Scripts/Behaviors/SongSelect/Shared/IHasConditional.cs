namespace JANOARG.Client.Behaviors.SongSelect.Shared
{
    public interface IHasConditional
    {
        bool isRevealed { get; }
        bool isUnlocked { get; }
    }
}