using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace BullethellPrototype.Unity
{
    public sealed class GameFlowController : MonoBehaviour
    {
        [System.Serializable]
        public sealed class SceneChangedEvent : UnityEvent<string> { }

        [SerializeField] private string stageId = "stage-1-prototype";
        [SerializeField] private bool loadOnStart = true;

        public StageDefinition CurrentStage { get; private set; }
        public DialogueScript CurrentDialogue { get; private set; }
        public CharacterProfile CurrentCharacter { get; private set; }
        public string CurrentSceneId { get; private set; }
        public int CurrentDialogueIndex { get; private set; }

        public SceneChangedEvent OnSceneChanged = new();

        private void Start()
        {
            if (loadOnStart)
            {
                BeginStageLoad();
            }
        }

        public void BeginStageLoad()
        {
            StartCoroutine(LoadStageRoutine());
        }

        public void Advance()
        {
            if (CurrentStage == null)
            {
                return;
            }

            if (CurrentSceneId == "stage-intro")
            {
                SetScene("dialogue-pre");
                CurrentDialogueIndex = 0;
                return;
            }

            DialogueLine[] block = GetCurrentDialogueBlock();
            if (block != null && CurrentDialogueIndex < block.Length - 1)
            {
                CurrentDialogueIndex += 1;
                return;
            }

            if (CurrentSceneId == "dialogue-pre")
            {
                SetScene("battle");
                return;
            }

            if (CurrentSceneId == "dialogue-post")
            {
                SetScene("stage-clear");
                return;
            }

            if (CurrentSceneId == "stage-clear" || CurrentSceneId == "game-over")
            {
                SetScene("stage-intro");
                CurrentDialogueIndex = 0;
            }
        }

        public DialogueLine GetCurrentDialogueLine()
        {
            DialogueLine[] block = GetCurrentDialogueBlock();
            if (block == null || block.Length == 0)
            {
                return null;
            }

            return block[Mathf.Clamp(CurrentDialogueIndex, 0, block.Length - 1)];
        }

        public SceneDefinition GetCurrentSceneDefinition()
        {
            if (CurrentStage == null)
            {
                return null;
            }

            return CurrentStage.scenes.FirstOrDefault(scene => scene.scene == CurrentSceneId);
        }

        private IEnumerator LoadStageRoutine()
        {
            yield return StageLoader.LoadStage(
                stageId,
                stage =>
                {
                    CurrentStage = stage;
                    StartCoroutine(LoadStageContentRoutine(stage));
                },
                error =>
                {
                    Debug.LogError(error);
                });
        }

        private IEnumerator LoadStageContentRoutine(StageDefinition stage)
        {
            yield return StageLoader.LoadDialogue(
                stage.dialogueScriptFile,
                dialogue => CurrentDialogue = dialogue,
                error => Debug.LogError(error));

            yield return StageLoader.LoadCharacter(
                stage.characterProfileFile,
                character => CurrentCharacter = character,
                error => Debug.LogError(error));

            CurrentDialogueIndex = 0;
            SetScene("stage-intro");
            Debug.Log($"Loaded stage: {stage.stageLabel}");
        }

        private DialogueLine[] GetCurrentDialogueBlock()
        {
            if (CurrentDialogue == null)
            {
                return null;
            }

            if (CurrentSceneId == "dialogue-pre")
            {
                return CurrentDialogue.preBattle;
            }

            if (CurrentSceneId == "dialogue-post")
            {
                return CurrentDialogue.postBattle;
            }

            return null;
        }

        private void SetScene(string sceneId)
        {
            CurrentSceneId = sceneId;
            OnSceneChanged?.Invoke(sceneId);
        }
    }
}
