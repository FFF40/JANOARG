namespace JANOARG.Client.Behaviors.SongSelect.Shared
{
    public interface IHasGameConditional
    {
        bool isRevealed { get; }
        bool isUnlocked { get; }
    }
}