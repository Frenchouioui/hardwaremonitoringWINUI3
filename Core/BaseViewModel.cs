using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace HardwareMonitorWinUI3.Core
{
    public abstract class BaseViewModel : ObservableObject, IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DisposeManaged();
                }

                DisposeUnmanaged();

                _disposed = true;
            }
        }

        protected virtual void DisposeManaged() { }

        protected virtual void DisposeUnmanaged() { }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}