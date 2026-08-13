#if !NET5_0_OR_GREATER
// see https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Runtime/CompilerServices/IsExternalInit.cs
// also see https://www.reddit.com/r/Unity3D/comments/1r68dn1/is_anyone_having_success_with_using_polyfills/
// and https://docs.unity3d.com/2023.2/Documentation/Manual/CSharpCompiler.html
namespace System.Runtime.CompilerServices {
    internal static class IsExternalInit { }
}
#endif