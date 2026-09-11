// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.DotNet.HotReload;

internal sealed class RunningProcess(
    int id,
    Task<int?> lifetimeTask,
    CancellationTokenSource exitedSource,
    CancellationTokenSource terminationSource) : IAsyncDisposable
{
    public int Id => id;

    /// <summary>
    /// Task that tracks the lifetime of the process. The task completes when the process exits, either normally or due to termination.
    /// </summary>
    public Task<int?> LifetimeTask => lifetimeTask;

    /// <summary>
    /// Used to signal to terminate the process.
    /// Null when disposed.
    /// </summary>
    private CancellationTokenSource? _terminationSource = terminationSource;

    /// <summary>
    /// Cancellation token triggered when the process exits.
    /// Stores the token to allow callers to use the token even after the source has been disposed.
    /// </summary>
    public readonly CancellationToken ExitedCancellationToken = exitedSource.Token;

    ValueTask IAsyncDisposable.DisposeAsync()
        => DisposeAsync(isProcessExiting: false);

    public ValueTask OnExitDisposeAsync()
        => DisposeAsync(isProcessExiting: true);

    /// <summary>
    /// Disposes the process.
    /// Completes <see cref="LifetimeTask"/> and signals <see cref="ExitedCancellationToken"/>.
    /// </summary>
    /// <param name="isProcessExiting">
    /// True if called from onExit handler of the <see cref="ProcessState"/>.
    /// False if called from <see cref="IAsyncDisposable.DisposeAsync"/>.
    /// </param>
    private async ValueTask DisposeAsync(bool isProcessExiting)
    {
        var terminationSource = Interlocked.Exchange(ref _terminationSource, null);
        if (terminationSource is null)
        {
            ObjectDisposedException.ThrowIf(isProcessExiting, this);
            return;
        }

        // When disposing from onExit handler do not await the lifetime task.
        // The handler runs runs as part of the task and awaiting the task would deadlock.
        if (!isProcessExiting)
        {
            terminationSource.Cancel();
            await lifetimeTask;
        }

        terminationSource.Dispose();

        exitedSource.Cancel();
        exitedSource.Dispose();
    }

    /// <summary>
    /// Terminates the process if it hasn't terminated yet.
    /// Awating the task triggers OnExit handler, which in turn calls <see cref="OnExitDisposeAsync"/>.
    /// </summary>
    public Task TerminateAsync()
    {
        _terminationSource?.Cancel();
        return lifetimeTask;
    }
}
