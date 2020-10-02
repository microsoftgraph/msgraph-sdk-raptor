using System;

namespace MsGraphSDKSnippetsCompiler.Models
{
    /// <summary>
    /// Programming languages that we generate snippets for
    /// </summary>
    public enum Languages
    {
        CSharp,
        JavaScript,
        Java,
        ObjC
    }

    /// <summary>
    /// Microsoft Graph Documetation Versions
    /// </summary>
    public enum Versions
    {
        V1,
        Beta
    }

    /// <summary>
    /// String representation for docs versions as file path or url segments
    /// </summary>
    public class VersionString
    {
        private readonly Versions Version;

        public VersionString(Versions version)
        {
            Version = version;
        }

        public override string ToString()
        {
            return Version switch
            {
                Versions.V1 => "v1.0",
                Versions.Beta => "beta",
                _ => throw new ArgumentException("Unexpected version, we can't resolve this to a path or url segment."),
            };
        }

        public string DocsUrlSegment()
        {
            return Version switch
            {
                Versions.V1 => "1.0",
                Versions.Beta => "beta",
                _ => throw new ArgumentException("Unexpected version, we can't resolve this to a path or url segment."),
            };
        }
    }
}