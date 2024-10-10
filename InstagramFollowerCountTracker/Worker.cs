using FollowerCountDatabaseTools;
using FollowerCountDatabaseTools.Models;
using InstagramFollowerCountTracker;
using Microsoft.Extensions.Options;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private string connectionString;
    private string apiKey;
    private string reportRecipient;
    private string reportSender;
    private List<string> accountUsernames;
    private DatabaseManager databaseManager;

    private DateTime lastCollectionTime = DateTime.MinValue;

    public Worker(ILogger<Worker> logger, IOptions<WorkerOptions> options)
    {
        _logger = logger;

        if (options.Value.Args == null)
            throw new ArgumentNullException("Command line arguments needs to be provided! Should be first a connection string in the format postgres://username:password@hostname:port/database. Then a list of user account names separated by space like this: user1 user2 user3");

        connectionString = options.Value.Args[0];
        apiKey = options.Value.Args[1];
        reportRecipient = options.Value.Args[2];
        reportSender = options.Value.Args[3];
        accountUsernames = options.Value.Args.Skip(4).ToList();

        databaseManager = new DatabaseManager(connectionString);
    }

    private async Task SetupDatabaseAsync()
    {
        await Task.CompletedTask;
        databaseManager.SetupDatabase();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await SetupDatabaseAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            if (ShouldPerformCollection())
            {
                await PerformAccountInfoCollection();

                await HandleWeeklyReportEmail();
            }

            await Task.Delay(GetSleepTime(), stoppingToken);
        }
    }

    private bool ShouldPerformCollection()
    {
        DateTime now = DateTime.Now;
        bool shouldCollect = now - lastCollectionTime > TimeSpan.FromHours(4); // make sure at least 4 hours have passed since last collection

        // make sure it is between 3 and 5 in the morning or that at least 48 hours have passed since last collection
        shouldCollect = shouldCollect && ((now.Hour >= 3 && now.Hour <= 5) || now - lastCollectionTime > TimeSpan.FromHours(48));
        return shouldCollect;
    }

    private int GetSleepTime()
    {
        DateTime now = DateTime.Now;
        TimeSpan startTime = new TimeSpan(3, 0, 0); // 3:00 AM
        TimeSpan endTime = new TimeSpan(5, 0, 0);   // 5:00 AM
        TimeSpan currentTime = now.TimeOfDay;
        TimeSpan sixHours = TimeSpan.FromHours(6);

        bool isBetween3And5 = currentTime >= startTime && currentTime <= endTime;
        bool durationSinceLastCollectionExceeds6Hours = now - lastCollectionTime > sixHours;

        if (isBetween3And5 && durationSinceLastCollectionExceeds6Hours)
        {
            // Time is between 3:00 AM and 5:00 AM and duration since last collection exceeds 6 hours
            return 10 * 60 * 1000; // 10 minutes in milliseconds
        }
        else
        {
            // Time is not between 3:00 AM and 5:00 AM or duration since last collection is not 6 hours
            return 60 * 60 * 1000; // 1 hour in milliseconds
        }
    }

    private async Task PerformAccountInfoCollection()
    {
        List<AccountInfo> accountInfos = new List<AccountInfo>();

        foreach (string username in accountUsernames)
        {
            AccountInfo? accountInfo = await Instagram.Instance.GetAccountInfoAsync(username);

            if (accountInfo != null)
                accountInfos.Add(accountInfo);

            await Task.Delay(1000);
        }

        await databaseManager.StoreAccountInfoAsync(accountInfos.ToArray());
        lastCollectionTime = DateTime.Now;

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation($"Collected account info for {accountInfos.Count} accounts at: {DateTimeOffset.Now}");
        }
    }

    private async Task HandleWeeklyReportEmail()
    {
        if (DateTime.Now.DayOfWeek != DayOfWeek.Sunday) return;
        if (apiKey.Length < 10) return;

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation($"Will try to send weekly update email\nApiKey: {apiKey.First()}{new string('*', apiKey.Length - 2)}{apiKey.Last()}");
        }

        try
        {
            Graph singleGraph = await Graph.CreateFromAccountNamesAsync(databaseManager, accountUsernames.First());
            Graph totalGraph = await Graph.CreateFromAccountNamesAsync(databaseManager, accountUsernames.ToArray());

            EmailHelper emailHelper = new EmailHelper(apiKey, reportRecipient, reportSender);
            await emailHelper.SendGraphReport(totalGraph.Export(), singleGraph.Export());

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation($"Did send weekly update email");
            }
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(ex, "Failed to send weekly update email");
            }
        }
    }
}
