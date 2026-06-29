using System.Diagnostics;
using System.Net.Http.Json;

var client = new HttpClient
{
    BaseAddress = new Uri("http://localhost:5078")
};

var totalRequests = 100000;
var concurency = 8000;

var semaphore = new SemaphoreSlim(concurency);
var stopwatch = Stopwatch.StartNew();

var tasks = Enumerable.Range(1, totalRequests).Select(async i =>
{
    await semaphore.WaitAsync();
    
    try
    {
        var request = new
        {            
            type = "offline",
            timeStamp = DateTimeOffset.UtcNow
        };

        var response = await client.PostAsJsonAsync($"api/devices/GDO_{i}/events", request);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"{(int)response.StatusCode}: {body}");
        }        
    }
    finally
    {
        semaphore.Release();
    }
});

await Task.WhenAll(tasks);
stopwatch.Stop();
Console.WriteLine($"Finished {totalRequests} requests in {stopwatch.Elapsed}");