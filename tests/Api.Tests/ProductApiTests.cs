using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Tests;

public class ProductApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private static object Body(string sku = "T-100", string name = "Тестовий магніт") =>
        new { sku, name, category = "магніт", unit = "шт", quantity = 5 };

    [Fact]
    public async Task Post_Returns201_AndLocationIsReadable()
    {
        var client = factory.CreateClient();

        var created = await client.PostAsJsonAsync("/api/products", Body("T-201"));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);

        var get = await client.GetAsync(created.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }

    [Fact]
    public async Task Post_InvalidBody_Returns400_WithFieldErrors()
    {
        var client = factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/products",
            new { sku = "", name = "X", category = "?", unit = "", quantity = -1 });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        var text = await resp.Content.ReadAsStringAsync();
        Assert.Contains("\"Sku\"", text);
        Assert.Contains("\"Quantity\"", text);
        Assert.DoesNotContain("Exception", text);
    }

    [Fact]
    public async Task Post_DuplicateSku_Returns409()
    {
        var client = factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/products", Body("SKU-001"));
        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }

    [Fact]
    public async Task Delete_Returns204_ThenGetReturns404()
    {
        var client = factory.CreateClient();
        var created = await client.PostAsJsonAsync("/api/products", Body("T-301"));
        var location = created.Headers.Location!;

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync(location)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(location)).StatusCode);
    }

    [Fact]
    public async Task Post_BrokenJson_Returns400_Not500()
    {
        var client = factory.CreateClient();
        var resp = await client.PostAsync("/api/products",
            new StringContent("{ not json", System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }
}
