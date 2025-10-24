using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Dtos.RoBoard;

namespace Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public CustomerService(IUserRepository userRepository, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoBoardCustomerDto>> SearchCustomersAsync(string searchTerm)
        {
            var customers = await _userRepository.SearchCustomersAsync(searchTerm);
            
            // Filter to only include users with "Customer" role
            var customerDtos = new List<RoBoardCustomerDto>();
            foreach (var customer in customers)
            {
                var roles = await _userRepository.GetRolesAsync(customer);
                if (roles.Contains("Customer"))
                {
                    customerDtos.Add(new RoBoardCustomerDto
                    {
                        UserId = customer.Id,
                        FirstName = customer.FirstName,
                        LastName = customer.LastName,
                        Birthday = customer.Birthday,
                        FullName = customer.FullName,
                        Email = customer.Email,
                        PhoneNumber = customer.PhoneNumber
                    });
                }
            }
            
            return customerDtos;
        }

        public async Task<IEnumerable<RoBoardCustomerDto>> GetAllCustomersAsync()
        {
            // Get all users with "Customer" role
            var customers = await _userRepository.GetAllCustomersAsync();
            return customers.Select(c => new RoBoardCustomerDto
            {
                UserId = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Birthday = c.Birthday,
                FullName = c.FullName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber
            });
        }

        public async Task<RoBoardCustomerDto> GetCustomerByIdAsync(string customerId)
        {
            var customer = await _userRepository.GetByIdAsync(customerId);
            if (customer == null) return null;
            
            // Verify that the user has the "Customer" role
            var roles = await _userRepository.GetRolesAsync(customer);
            if (!roles.Contains("Customer"))
            {
                return null; // User is not a customer
            }
            
            return new RoBoardCustomerDto
            {
                UserId = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Birthday = customer.Birthday,
                FullName = customer.FullName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber
            };
        }
        
        public async Task<RoBoardCustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto)
        {
            // Validate the DTO
            var validationContext = new ValidationContext(createCustomerDto);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(createCustomerDto, validationContext, validationResults, true))
            {
                var errorMessage = string.Join("; ", validationResults.Select(vr => vr.ErrorMessage));
                var validationException = new ValidationException($"Invalid customer data: {errorMessage}");
                throw validationException;
            }
            
            // Additional custom validation
            if (string.IsNullOrWhiteSpace(createCustomerDto.FirstName) || 
                string.IsNullOrWhiteSpace(createCustomerDto.LastName))
            {
                throw new ValidationException("First name and last name are required");
            }
            
            if (string.IsNullOrWhiteSpace(createCustomerDto.PhoneNumber))
            {
                throw new ValidationException("Phone number is required");
            }
            
            // Check if customer with same phone number already exists
            var existingCustomer = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == createCustomerDto.PhoneNumber);
            if (existingCustomer != null)
            {
                throw new InvalidOperationException("Customer with this phone number already exists");
            }
            
            // Check if customer with same email already exists (if email is provided)
            if (!string.IsNullOrEmpty(createCustomerDto.Email))
            {
                var existingEmailCustomer = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Email == createCustomerDto.Email);
                if (existingEmailCustomer != null)
                {
                    throw new InvalidOperationException("Customer with this email already exists");
                }
            }
            
            // Create new ApplicationUser
            var customer = new ApplicationUser
            {
                UserName = createCustomerDto.PhoneNumber,
                PhoneNumber = createCustomerDto.PhoneNumber,
                FirstName = createCustomerDto.FirstName,
                LastName = createCustomerDto.LastName,
                FullName = createCustomerDto.FullName,
                Email = createCustomerDto.Email,
                Birthday = createCustomerDto.Birthday,
                DateOfBirth = createCustomerDto.Birthday, // Also set DateOfBirth for consistency
                CreatedAt = DateTime.UtcNow
            };
            
            // Set a default password
            var defaultPassword = "Customer@123"; // In a real implementation, this should be more secure
            
            var result = await _userManager.CreateAsync(customer, defaultPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create customer: {errors}");
            }
            
            // Add to Customer role
            await _userManager.AddToRoleAsync(customer, "Customer");
            
            // Return the created customer DTO
            return new RoBoardCustomerDto
            {
                UserId = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Birthday = customer.Birthday,
                FullName = customer.FullName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber
            };
        }
    }
}