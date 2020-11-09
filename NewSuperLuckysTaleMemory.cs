using LiveSplit.Model;
using LiveSplit.VoxSplitter;
using System;
using System.Collections.Generic;

namespace LiveSplit.NewSuperLuckysTale {
    public class NewSuperLuckysTaleMemory : Memory {

        private Pointer<IntPtr> loading;
        private StringPointer level;
        private Pointer<bool> cutscene;

        private bool isReady = true;
        private int cutsceneCount = 0;

        private readonly RemainingDictionary remainingSplits;
        private bool allLevels = false;
        private bool allPuzzles = false;

        private readonly MonoHelper mono;

        public NewSuperLuckysTaleMemory(LiveSplitState state, Logger logger) : base(state, logger) {
            SetProcessNames("nslt", "New Super Lucky's Tale");
            remainingSplits = new RemainingDictionary(logger);
            mono = new MonoHelper(this);
        }

        public override bool IsReady() => base.IsReady() && mono.IsCompleted;

        protected override void OnHook() {
            mono.Run(() => {
                MonoNestedPointerFactory ptrFactory = new MonoNestedPointerFactory(this, mono);

                loading = ptrFactory.Make<IntPtr>("UiLoadingScreen", "<Instance>k__BackingField", "_spinningImage", 0x30);

                level = ptrFactory.MakeString("GameScenesInterface", "_persistentLevelName", 0x14);
                level.StringType = EStringType.UTF16Sized;

                cutscene = ptrFactory.Make<bool>("CutsceneSequence", "<HasActiveCutscene>k__BackingField");

                Logger.Log(ptrFactory.ToString());
            });
        }

        public override bool Update() {
            if(!isReady && loading.New != default && !String.IsNullOrEmpty(level.New)) {
                isReady = true;
            }
            return isReady;
        }

        public override bool Start(int start) {
            return loading.Old == default && loading.New != default
                && (level.New.Contains("TitleScreen") || level.New.Equals("LW_Intro_Cinematic") || start == (int)EStart.AnyLevel);
        }

        public override void OnStart(HashSet<string> splits) {
            if(splits.Contains("Levels")) { allLevels = splits.Remove("Levels"); }
            if(splits.Contains("Puzzles")) { allPuzzles = splits.Remove("Puzzles"); }
            remainingSplits.Setup(splits);
            cutsceneCount = 0;
        }

        public override bool Split() {
            return remainingSplits.Count != 0 && (SplitLevels() || SplitCutscenes());

            bool SplitLevels() {
                if(!level.Changed || !(level.New.StartsWith("Chapter") || String.IsNullOrEmpty(level.New))) {
                    return false;
                }

                if(remainingSplits.ContainsKey("Level") && remainingSplits.Split("Level", level.Old)) {
                    return true;
                }

                bool isPuzzle = (level.Old.Contains("_Marble_") && level.Old.EndsWith("_Outro")) || level.Old.Contains("_SlidingBlock_");
                if((allLevels && !isPuzzle) || (allPuzzles && isPuzzle)) {
                    Logger.Log("Split any level " + level.Old);
                    return true;
                }

                return false;
            }

            bool SplitCutscenes() {
                if(!remainingSplits.ContainsKey("Cutscene")) { return false; }

                // Remove if more cutscenes are added
                if(!level.New.Equals("SP_Boss_01", StringComparison.Ordinal)) { return false; }

                if(level.Changed) {
                    cutsceneCount = 0;
                }
                if(!cutscene.Old && cutscene.New) {
                    return remainingSplits.Split("Cutscene", level.New + "_" + (++cutsceneCount));
                }
                return false;
            }
        }

        public override bool Loading() => loading.New != default || level.New.Equals("LW_Intro_Cinematic", StringComparison.Ordinal);

        public override void OnExit() {
            state.IsGameTimePaused = true;
            isReady = false;
        }

        public override void Dispose() => mono.Dispose();
    }
}