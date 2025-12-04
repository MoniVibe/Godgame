using Unity.Entities;
using Unity.Collections;

namespace Godgame.Demo
{
    public struct DemoOptions : IComponentData
    {
        public FixedString64Bytes ScenarioPath;
        public byte BindingsSet; // 0=Minimal, 1=Fancy
        public byte Veteran;     // 0/1 (for Space4X compatibility)
    }
}
