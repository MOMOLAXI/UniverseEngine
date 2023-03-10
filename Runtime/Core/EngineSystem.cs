namespace Universe
{
    internal class EngineSystem : ISystem
    {
        internal static T Create<T>() where T : EngineSystem, new()
        {
            return new();
        }
    }
}