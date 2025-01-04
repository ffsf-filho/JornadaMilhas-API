using JornadaMilhas.Dominio.Entidades;
using JornadaMilhas.Dominio.ValueObjects;
using JornadaMilhas.Integration.Test.API.DataBuilders;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace JornadaMilhas.Integration.Test.API;

public class OfertaViagem_GET : IClassFixture<JornadaMilhasWebApplicationFactory>
{
    private readonly JornadaMilhasWebApplicationFactory _app;

    public OfertaViagem_GET(JornadaMilhasWebApplicationFactory app)
    {
        _app = app;
    }

    [Fact]
    public async Task Recupera_OfertaViagem_PorId()
    {
        //arrange
        var ofertaExistente = _app.Context.OfertasViagem.FirstOrDefault();

        if (ofertaExistente is null)
        {
            ofertaExistente = new OfertaViagem()
            {
                Preco = 100,
                Rota = new Rota("Origem", "Destino"),
                Periodo = new Periodo(DateTime.Parse("2024-03-03"), DateTime.Parse("2024-03-06"))
            };

            _app.Context.Add(ofertaExistente);
            _app.Context.SaveChanges();
        }

        using var client = await _app.GetClientWithAccessTokenAsync();

        //act
        var response = await client.GetFromJsonAsync<OfertaViagem>("/ofertas-viagem/" + ofertaExistente.Id);

        //assert
        Assert.NotNull(response);
        Assert.Equal(ofertaExistente.Preco, response.Preco, 0.001);
        Assert.Equal(ofertaExistente.Rota.Origem, response.Rota.Origem);
        Assert.Equal(ofertaExistente.Rota.Destino, response.Rota.Destino);
    }

    [Fact]
    public async Task Recuperar_OfertasViagem_Na_Consulta_Paginada()
    {
        //arrange
        var ofertaDataBuilder = new OfertaViagemDataBuilder();
        var listaDeOfertas = ofertaDataBuilder.Generate(80);
        _app.Context.OfertasViagem.AddRange(listaDeOfertas);
        _app.Context.SaveChanges();

        using var client = await _app.GetClientWithAccessTokenAsync();

        int pagina = 1;
        int tamanhoPorPagina = 80;

        //act
        var response = await client.GetFromJsonAsync<ICollection<OfertaViagem>>($"/ofertas-viagem?pagina={pagina}&tamanhoPorPagina={tamanhoPorPagina}");

        //assert
        Assert.NotNull(response);
        Assert.Equal(tamanhoPorPagina, response.Count);
    }

    [Fact]
    public async Task Recuperar_OfertasViagem_Na_Consulta_Ultima_Pagina()
    {
        //arrange
        _app.Context.Database.ExecuteSqlRaw("Delete from OfertasViagem");

        var ofertaDataBuilder = new OfertaViagemDataBuilder();
        var listaDeOfertas = ofertaDataBuilder.Generate(80);
        _app.Context.OfertasViagem.AddRange(listaDeOfertas);
        _app.Context.SaveChanges();

        using var client = await _app.GetClientWithAccessTokenAsync();

        int pagina = 4;
        int tamanhoPorPagina = 25;

        //act
        var response = await client.GetFromJsonAsync<ICollection<OfertaViagem>>($"/ofertas-viagem?pagina={pagina}&tamanhoPorPagina={tamanhoPorPagina}");

        //assert
        Assert.NotNull(response);
        Assert.Equal(5, response.Count);
    }

    [Fact]
    public async Task Recuperar_OfertasViagem_Na_Consulta_Com_Pagina_Inexistente()
    {
        //arrange
        _app.Context.Database.ExecuteSqlRaw("Delete from OfertasViagem");

        var ofertaDataBuilder = new OfertaViagemDataBuilder();
        var listaDeOfertas = ofertaDataBuilder.Generate(80);
        _app.Context.OfertasViagem.AddRange(listaDeOfertas);
        _app.Context.SaveChanges();

        using var client = await _app.GetClientWithAccessTokenAsync();

        int pagina = 5;
        int tamanhoPorPagina = 25;

        //act
        var response = await client.GetFromJsonAsync<ICollection<OfertaViagem>>($"/ofertas-viagem?pagina={pagina}&tamanhoPorPagina={tamanhoPorPagina}");

        //assert
        Assert.NotNull(response);
        Assert.Equal(0, response.Count);
    }

    [Fact]
    public async Task Recuperar_OfertasViagem_Na_Consulta_Com_Pagina_Com_Valor_Negativo()
    {
        //arrange
        _app.Context.Database.ExecuteSqlRaw("Delete from OfertasViagem");

        var ofertaDataBuilder = new OfertaViagemDataBuilder();
        var listaDeOfertas = ofertaDataBuilder.Generate(80);
        _app.Context.OfertasViagem.AddRange(listaDeOfertas);
        _app.Context.SaveChanges();

        using var client = await _app.GetClientWithAccessTokenAsync();

        int pagina = -5;
        int tamanhoPorPagina = 25;

        //act + assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            var response = await client.GetFromJsonAsync<ICollection<OfertaViagem>>($"/ofertas-viagem?pagina={pagina}&tamanhoPorPagina={tamanhoPorPagina}");
        });

        //assert
        //Assert.NotNull(response);
        //Assert.Equal(0, response.Count);
    }
}
