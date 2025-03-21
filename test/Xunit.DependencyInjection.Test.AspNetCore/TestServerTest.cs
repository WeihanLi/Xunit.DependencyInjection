﻿namespace Xunit.DependencyInjection.Test.AspNetCore;

public class TestServerTest(HttpClient httpClient)
{
    public static string Key { get; } = Guid.NewGuid().ToString("N");

    [Fact]
    public async Task HttpTest()
    {
        using var response = await httpClient.GetAsync("/", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        Assert.Equal(Key, await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }
}
