using Unity.Entities;
using Unity.Collections;

namespace Godgame.Scenario
{
    public struct ScenarioOptions : IComponentData
    {
        public FixedString64Bytes ScenarioPath;
        public byte BindingsSet; // 0=Minimal, 1=Fancy
        public byte Veteran;     // 0/1 (for Space4X compatibility)
    }
}
