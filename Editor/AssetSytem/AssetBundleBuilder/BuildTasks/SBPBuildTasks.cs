using System.Collections.Generic;
using IBuildTask = UnityEditor.Build.Pipeline.Interfaces.IBuildTask;


namespace UnityEditor.Build.Pipeline.Tasks
{
    public static class SbpBuildTasks
    {
        public static IList<IBuildTask> Create(string builtInShaderBundleName)
        {
            List<IBuildTask> buildTasks = new()
            {
                // Setup
                new SwitchToBuildPlatform(),
                new RebuildSpriteAtlasCache(),
                
                // Player Scripts
                new BuildPlayerScripts(),
                new PostScriptsCallback(),
                
                // Dependency
                new CalculateSceneDependencyData(),
                new CalculateCustomDependencyData(),
                new CalculateAssetDependencyData(),
                new StripUnusedSpriteSources(),
                new CreateBuiltInShadersBundle(builtInShaderBundleName),
                new PostDependencyCallback(),
                
                // Packing
                new GenerateBundlePacking(),
                new UpdateBundleObjectLayout(),
                new GenerateBundleCommands(),
                new GenerateSubAssetPathMaps(),
                new GenerateBundleMaps(),
                new PostPackingCallback(),
                
                // Writing
                new WriteSerializedFiles(),
                new ArchiveAndCompressBundles(),
                new AppendBundleHash(),
                new GenerateLinkXml(),
                new PostWritingCallback()
            };

            return buildTasks;
        }
    }
}