namespace LapisPlayer
{
    interface IDanceManager
    {
        public bool IsARMode { get; }
        public CharacterActor GetCharacter(int position);
        public CharacterActor[] GetActiveCharacters();
    }
}
