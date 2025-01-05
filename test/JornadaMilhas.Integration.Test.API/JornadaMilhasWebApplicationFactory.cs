using JornadaMilhas.API.DTO.Auth;
using JornadaMilhas.Dados;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Testcontainers.MsSql;

namespace JornadaMilhas.Integration.Test.API;

public class JornadaMilhasWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public JornadaMilhasContext Context { get; private set; }
    private IServiceScope scope;
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

    public JornadaMilhasWebApplicationFactory()
    {
        _msSqlContainer.StartAsync().GetAwaiter().GetResult();
        this.scope = Services.CreateScope();
        Context = scope.ServiceProvider.GetRequiredService<JornadaMilhasContext>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<JornadaMilhasContext>));
            services.AddDbContext<JornadaMilhasContext>(options =>
                options
                .UseLazyLoadingProxies()
                .UseSqlServer(_msSqlContainer.GetConnectionString())
                );
                //.UseSqlServer("Server=localhost,1438;Database=JornadaMilhasV3;User Id=sa;Password=ssExpress1823;Encrypt=false;TrustServerCertificate=true;MultipleActiveResultSets=true;")
        });

        base.ConfigureWebHost(builder);
    }

    public async Task<HttpClient> GetClientWithAccessTokenAsync()
    {
        var client = this.CreateClient();
        var user = new UserDTO { Email = "tester@email.com", Password = "Senha123@" };
        var resultado = await client.PostAsJsonAsync("/auth-login", user);
        resultado.EnsureSuccessStatusCode();
        var result = await resultado.Content.ReadFromJsonAsync<UserTokenDTO>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token);
        return client;
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
        //this.scope = Services.CreateScope();
        //Context = scope.ServiceProvider.GetRequiredService<JornadaMilhasContext>();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
    }
}
