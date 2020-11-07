using LiveSplit.Model;
using LiveSplit.VoxSplitter;
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

        public NewSuperLuckysTaleComponent(LiveSplitState state) : base(state) {
            memory = new NewSuperLuckysTaleMemory(state, logger);
            settings = new TreeSettings(state, Start, Reset, Options);
        }
    }
}