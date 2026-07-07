using System.Threading;
using Microsoft.Extensions.Logging;

namespace SwitchcraftKeys.Services;

public sealed class SingleInstanceGuard : IDisposable
{
    public const string MutexName = @"Global\SwitchcraftKeys";

    private readonly Mutex _mutex;

    private readonly ILogger<SingleInstanceGuard> _logger;
    private bool _disposed;

    private SingleInstanceGuard(Mutex mutex, ILogger<SingleInstanceGuard> logger)
    {
        _mutex = mutex;
        _logger = logger;
    }

    public static SingleInstanceGuard? TryAcquire(ILogger<SingleInstanceGuard> logger)
    {
        try
        {
            logger.LogInformation("Acquiring single-instance mutex name={MutexName}", MutexName);
            var mutex = new Mutex(initiallyOwned: true, MutexName, out var createdNew);
            logger.LogInformation("Single-instance mutex acquire result name={MutexName} createdNew={CreatedNew}", MutexName, createdNew);

            if (createdNew)
            {
                return new SingleInstanceGuard(mutex, logger);
            }

            logger.LogCritical("Named mutex already exists name={MutexName}", MutexName);
            mutex.Dispose();
            return null;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or AbandonedMutexException)
        {
            logger.LogCritical(ex, "Named mutex acquire failed name={MutexName}", MutexName);
            return null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            _logger.LogInformation("Releasing single-instance mutex name={MutexName}", MutexName);
            _mutex.ReleaseMutex();
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Single-instance mutex release failed name={MutexName}", MutexName);
        }
        finally
        {
            _mutex.Dispose();
            _disposed = true;
        }
    }
}
