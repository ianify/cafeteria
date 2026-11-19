using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CafeteriaApi.External;

public record ViaCepResponse(
    [property: JsonPropertyName("cep")] string? Cep,
    [property: JsonPropertyName("logradouro")]
    string? Logradouro,
    [property: JsonPropertyName("complemento")]
    string? Complemento,
    [property: JsonPropertyName("bairro")] string? Bairro,
    [property: JsonPropertyName("localidade")]
    string? Localidade,
    [property: JsonPropertyName("uf")] string? Uf,
    [property: JsonPropertyName("erro")] string? Erro
);

public record AddressLookupResponse(
    string Cep,
    string Street,
    string Complement,
    string Neighborhood,
    string City,
    string State
);

public partial class ViaCepService(HttpClient httpClient)
{
    [GeneratedRegex(@"\D")]
    private static partial Regex NonDigitsRegex();

    public static string? NormalizeCep(string? rawCep)
    {
        if (string.IsNullOrWhiteSpace(rawCep))
            return null;

        var digits = NonDigitsRegex().Replace(rawCep, string.Empty);
        return digits.Length == 8 ? digits : null;
    }

    public async Task<AddressLookupResponse?> LookupAsync(string normalizedCep,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"{normalizedCep}/json/", cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var data = await response.Content.ReadFromJsonAsync<ViaCepResponse>(cancellationToken);

        if (data is null || string.Equals(data.Erro, "true", StringComparison.OrdinalIgnoreCase))
            return null;

        return new AddressLookupResponse(
            Cep: data.Cep ?? normalizedCep,
            Street: data.Logradouro ?? string.Empty,
            Complement: data.Complemento ?? string.Empty,
            Neighborhood: data.Bairro ?? string.Empty,
            City: data.Localidade ?? string.Empty,
            State: data.Uf ?? string.Empty
        );
    }
}