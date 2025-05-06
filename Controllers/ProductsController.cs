using ClientAppPOSWebAPI.Managers;
using ClientAppPOSWebAPI.Models;
using ClientAppPOSWebAPI.Success;
using Microsoft.AspNetCore.Mvc;

namespace ClientAppPOSWebAPI.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsManager _productManager;

        public ProductsController(ProductsManager productManager)
        {
            _productManager = productManager;
        }

        // POST: api/products
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest(Result.FailureResult("Product data is required"));
            }

            // Call the manager to add the product
            var createdProduct = await _productManager.AddProductAsync(product);

            if (createdProduct == null)
            {
                return StatusCode(500, Result.FailureResult("An error occurred while adding the product"));
            }

            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, Result.SuccessResult(createdProduct));
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productManager.GetProductAsync(id);

            if (product == null)
            {
                return NotFound(Result.FailureResult("Product not found"));
            }

            return Ok(Result.SuccessResult(product));
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productManager.GetAllProductsAsync();

            if (products == null || !products.Any())
            {
                return NotFound(Result.FailureResult("No products found"));
            }

            return Ok(Result.SuccessResult(products));
        }

        [Route("{id}")]
        [HttpPatch]
        public async Task<IActionResult> PatchProduct(int id, [FromBody] ProductDto dto)
        {
            if (dto == null)
                return BadRequest(Result.FailureResult("No data provided"));

            var updatedTheme = await _productManager.UpdateProduct(id, dto);

            if (updatedTheme == null)
                return NotFound(Result.FailureResult("Product not found"));

            return Ok(Result.SuccessResult(updatedTheme));
        }
    }
}
