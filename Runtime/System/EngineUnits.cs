using System.Collections.Generic;

namespace Universe
{
    internal static class EngineUnits
    {
        internal static readonly List<EngineSystem> Units = new()
        {
            EngineSystem.Create<EntitySystem>(),
            EngineSystem.Create<HearBeatSystem>(),
            EngineSystem.Create<MessageSystem>(),
            EngineSystem.Create<GameplaySystem>(),
            EngineSystem.Create<AssetSystem>(),
            EngineSystem.Create<AssetDownloadSystem>(),
            EngineSystem.Create<OperationSystem>(),
            EngineSystem.Create<AutoVariableSystem>(),
            EngineSystem.Create<SequencerSystem>(),
        };
    }
}