using Microsoft.AspNetCore.Mvc;

namespace CafeteriaApi.External.ViaCep;

[ApiController]
[Route("api/[controller]")]
public class ViaCepController(ViaCepService viaCepService) : ControllerBase
{
    [HttpGet("{cep}")]
    public async Task<ActionResult<AddressLookupResponse>> GetByCep(string cep, CancellationToken cancellationToken)
    {
        var normalizedCep = ViaCepService.NormalizeCep(cep);
        if (normalizedCep is null)
        {
            return BadRequest(new { message = "Invalid CEP. Provide an 8-digit postal code (e.g., 01001000 or 01001-000)." });
        }

        var address = await viaCepService.LookupAsync(normalizedCep, cancellationToken);
        if (address is null)
        {
            return NotFound(new { message = "CEP not found." });
        }

        return Ok(address);
    }
}