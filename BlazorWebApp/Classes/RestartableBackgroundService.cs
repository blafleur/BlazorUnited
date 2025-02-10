using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using BlazorWebApp.Classes;

public class MyBackgroundService : BackgroundService
{
    private readonly CancellationTokenSource _manualTokenSource = new CancellationTokenSource();
   
    

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Combine the stoppingToken with the manual token
        using (var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _manualTokenSource.Token))
        {
            var combinedToken = combinedTokenSource.Token;

            while (!combinedToken.IsCancellationRequested)
            {
                // Call another method and pass the combined token
                 DoSomeWorkAsync(combinedToken);
                 Console.WriteLine("checking the token " + _manualTokenSource.IsCancellationRequested.ToString());
                // Wait for a period of time before running again
                try
                {
                    await Task.Delay(5000, combinedToken); // 5 seconds delay
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Task delay was canceled.");
                    break;
                }
            }
        }
    }

    private void DoSomeWorkAsync(CancellationToken cancellationToken)
    {
        
        if (_manualTokenSource.IsCancellationRequested)
        {
            Console.WriteLine("Cancellation requested. Stopping work...");
            return;
        }

        Console.WriteLine("Doing some work at: " + DateTime.Now + " "+cancellationToken.IsCancellationRequested.ToString());
        
        // Simulate work with a delay
        try
        {
           //  Task.Delay(1000, cancellationToken); // Pass the cancellationToken
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Work delay was canceled.");
        }
    }

    // Method to manually cancel the token
    public void Cancel()
    {
        _manualTokenSource.Cancel();
        Console.WriteLine("Manual cancellation requested.");
        Console.WriteLine(_manualTokenSource.IsCancellationRequested.ToString());
    }

    public async Task CheckStatus()
    {
        Console.WriteLine("Checking status..." + _manualTokenSource.IsCancellationRequested.ToString());
    }
}