using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace Datinate.App.WinForms.View.Util
{
    // Captured during control construction on the UI thread. Dispatch does not depend
    // on the view already having a handle, so early RMVC updates are not discarded.
    internal sealed class UiDispatcher
    {
        private readonly Control owner;
        private readonly SynchronizationContext context;
        private readonly int threadId;

        public UiDispatcher(Control owner)
        {
            this.owner = owner;
            threadId = Environment.CurrentManagedThreadId;
            context = SynchronizationContext.Current as WindowsFormsSynchronizationContext
                ?? new WindowsFormsSynchronizationContext();
        }

        public bool CheckAccess => Environment.CurrentManagedThreadId == threadId;

        public void Run(Action action)
        {
            if (CheckAccess)
            {
                if (!owner.IsDisposed && !owner.Disposing)
                    action();
                return;
            }

            Post(action);
        }

        private void Post(Action action)
        {
            try
            {
                context.Post(_ =>
                {
                    if (owner.IsDisposed || owner.Disposing)
                        return;
                    try { action(); }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"UI operation failed in {owner.GetType().Name}: {ex}");
                        throw;
                    }
                }, null);
            }
            catch (InvalidAsynchronousStateException ex)
            {
                Debug.WriteLine($"UI dispatcher is unavailable for {owner.GetType().Name}: {ex}");
                throw;
            }
        }

        // Reads must finish before returning to the command. Never hold a model lock
        // while waiting for this call, because the UI may need that same lock.
        public T Read<T>(Func<T> read)
        {
            T result = default!;
            Exception? failure = null;
            void ReadOnUi()
            {
                try
                {
                    if (owner.IsDisposed || owner.Disposing)
                        throw new ObjectDisposedException(owner.GetType().Name);
                    result = read();
                }
                catch (Exception ex) { failure = ex; }
            }

            if (CheckAccess) ReadOnUi();
            else context.Send(_ => ReadOnUi(), null);

            if (failure != null)
                ExceptionDispatchInfo.Capture(failure).Throw();
            return result;
        }

        public void PostWhenHandleCreated(Control target, Action action)
        {
            Post(() =>
            {
                if (target.IsDisposed || target.Disposing)
                    return;
                if (target.IsHandleCreated)
                {
                    action();
                    return;
                }

                EventHandler? ready = null;
                EventHandler? disposed = null;
                ready = (_, _) =>
                {
                    target.HandleCreated -= ready;
                    target.Disposed -= disposed;
                    Post(() =>
                    {
                        if (!target.IsDisposed && !target.Disposing)
                            action();
                    });
                };
                disposed = (_, _) =>
                {
                    target.HandleCreated -= ready;
                    target.Disposed -= disposed;
                };
                target.HandleCreated += ready;
                target.Disposed += disposed;
            });
        }
    }
}
