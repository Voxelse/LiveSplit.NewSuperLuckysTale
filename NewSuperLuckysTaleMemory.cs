using LiveSplit.Model;
using LiveSplit.VoxSplitter;
using System;
using System.Collections.Generic;

namespace LiveSplit.NewSuperLuckysTale {
    public class NewSuperLuckysTaleMemory : Memory {

        Pointer<IntPtr> loading;
        StringPointer level;

        private bool isReady = true;

        private readonly RemainingDictionary remainingSplits;

        private readonly MonoHelper mono;
        
        public NewSuperLuckysTaleMemory(LiveSplitState state, Logger logger) : base(state, logger) {
            processName = "nslt";
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
            remainingSplits.Setup(splits);
        }

        public override bool Split() {
            return remainingSplits.Count != 0 && (SplitLevels());

            bool SplitLevels() {
                return remainingSplits.ContainsKey("Level") && level.Changed
                    && (level.New.StartsWith("Chapter") || String.IsNullOrEmpty(level.New))
                    && remainingSplits.Split("Level", level.Old);
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