using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using LiveSplit.VoxSplitter;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace LiveSplit.NewSuperLuckysTale {
    public enum EStart {
        [Description("Off")]
        Off,
        [Description("New Game")]
        NewGame,
        [Description("Any Level")]
        AnyLevel
    }
    public class NewSuperLuckysTaleComponent : VoxSplitter.Component {

        protected override SettingInfo? Start => new SettingInfo((int)EStart.NewGame, GetEnumDescriptions<EStart>());
        protected override SettingInfo? Reset => null;
        protected override EGameTime GameTime => EGameTime.Loading;
        private DateTime lastInfoCheck = DateTime.MinValue;
        private TextComponent fpsComponent;

        public NewSuperLuckysTaleComponent(LiveSplitState state) : base(state) {
            memory = new NewSuperLuckysTaleMemory(state, logger);
            settings = new TreeSettings(state, Start, Reset, Options);
        }
        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode) {
            base.Update(invalidator, state, width, height, mode);

            DateTime dateTime = DateTime.Now;
            if(dateTime > lastInfoCheck) {
                fpsComponent = null;

                IList<ILayoutComponent> components = state.Layout.LayoutComponents;
                for(int i = components.Count - 1; i >= 0; i--) {
                    ILayoutComponent component = components[i];
                    if(component.Component is TextComponent text) {
                        if(text.Settings.Text1.IndexOf("FPS", StringComparison.OrdinalIgnoreCase) >= 0) {
                            fpsComponent = text;
                        }
                    }
                }
                lastInfoCheck = dateTime.AddSeconds(3);
            }

            if(fpsComponent != null) {
                string fps = ((NewSuperLuckysTaleMemory)memory).CurrentFPS().ToString("0.0");
                if(fps != fpsComponent.Settings.Text2) {
                    fpsComponent.Settings.Text2 = fps;
                }
            }
        }
    }
}