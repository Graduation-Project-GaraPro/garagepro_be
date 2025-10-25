using Microsoft.AspNetCore.Mvc;
using Services;
using Dtos.RoBoard;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData.Query;

namespace Garage_pro_api.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET: api/Customer
        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetCustomers()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Customer/search?searchTerm=
        [HttpGet("search")]
        public async Task<IActionResult> SearchCustomers([FromQuery] string searchTerm)
        {
            try
            {
                var customers = await _customerService.SearchCustomersAsync(searchTerm);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Customer/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(string id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    return NotFound(new { message = "Customer not found" });
                }
                return Ok(customer);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        // POST: api/Customer
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto createCustomerDto)
        {
            // Check if model state is valid
            if (!ModelState.IsValid)
            {
                var errors = new SerializableError(ModelState);
                return BadRequest(new { 
                    message = "Validation failed", 
                    errors 
                });
            }

            try
            {
                var customer = await _customerService.CreateCustomerAsync(createCustomerDto);
                return CreatedAtAction(nameof(GetCustomer), new { id = customer.UserId }, customer);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { 
                    message = "Validation error", 
                    details = ex.Message,
                    validationErrors = ex.ValidationResult?.ErrorMessage
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { 
                    message = "Operation failed", 
                    details = ex.Message 
                });
            }
            catch (Exception ex)
            {
                // Log the full exception details for debugging
                return StatusCode(500, new { 
                    message = "An error occurred while creating the customer", 
                    error = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }
    }
}