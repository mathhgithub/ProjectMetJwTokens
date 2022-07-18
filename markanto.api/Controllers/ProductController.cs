using mark.webApi.AuthFolder.Models;
using mark.webApi.Models;
using mark.webApi.Services;
using markantoApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace markantoApi.Controllers;

[ApiController]
[Route("api/products/")]
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("getall")]
    public async Task<IEnumerable<Object>> GetAll()
    {
        var output = await _productService.GetAllAsync();
        if (output == null) { throw new Exception("geen lijst gevonden"); }
        else { return output; }
    }

    /*
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id)
    {
        var temp = await _mongoservice.FindByIdAsync(id);
        await _mongoservice.ReplaceOneAsync(temp);
        return Ok($"Record met Id = {id} is aangepast");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _mongoservice.DeleteByIdAsync(id);
        return Ok($"Record met Id = {id} is verwijderd");
    }
    */
}
