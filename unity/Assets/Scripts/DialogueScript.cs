using System;

namespace BullethellPrototype.Unity
{
    [Serializable]
    public sealed class DialogueScript
    {
        public string placeholderSpeaker;
        public DialogueLine[] preBattle;
        public DialogueLine[] postBattle;
    }

    [Serializable]
    public sealed class DialogueLine
    {
        public string speaker;
        public string text;
    }
}
