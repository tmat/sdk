// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Watcher.Internal;

namespace Microsoft.DotNet.Watcher.Tests;

public class HotReloadFileSetWatcherTests
{
    [Fact]
    public void IsGitMetadataDataFile()
    {
        var root = Path.GetDirectoryName(typeof(HotReloadFileSetWatcherTests).Assembly.Location);
        Assert.True(HotReloadFileSetWatcher.IsGitMetadataFile(Path.Combine(root, ".git", "a")));
        Assert.True(HotReloadFileSetWatcher.IsGitMetadataFile(Path.Combine(root, ".git", "a", "b")));
        Assert.False(HotReloadFileSetWatcher.IsGitMetadataFile(Path.Combine(root, "a", "b")));
    }
}
