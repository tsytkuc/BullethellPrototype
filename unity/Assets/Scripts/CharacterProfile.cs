using System;

namespace BullethellPrototype.Unity
{
    [Serializable]
    public sealed class CharacterProfile
    {
        public string id;
        public string displayName;
        public string portraitImagePath;
        public string portraitAlt;
        public bool placeholderVisual;
        public string notes;
    }
}
