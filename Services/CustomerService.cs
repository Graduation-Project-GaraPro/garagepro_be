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
using Services.EmailSenders;
using DataAccessLayer;

namespace Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly MyAppDbContext _dbContext;

        public CustomerService(IUserRepository userRepository, UserManager<ApplicationUser> userManager, IMapper mapper, IEmailSender emailSender, IEmailTemplateService emailTemplateService, MyAppDbContext dbContext)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _mapper = mapper;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _dbContext = dbContext;
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
                        // Removed FullName assignment since it's no longer in the entity
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
                // Removed FullName assignment since it's no longer in the entity
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
                // Removed FullName assignment since it's no longer in the entity
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
                UserName = createCustomerDto.PhoneNumber, // Username is phone number
                PhoneNumber = createCustomerDto.PhoneNumber,
                FirstName = createCustomerDto.FirstName,
                LastName = createCustomerDto.LastName,
                Email = createCustomerDto.Email,
                Birthday = createCustomerDto.Birthday,
                DateOfBirth = createCustomerDto.Birthday,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };
            
            // Set default password: Garagepro123!
            var defaultPassword = "Garagepro123!";
            
            var result = await _userManager.CreateAsync(customer, defaultPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create customer: {errors}");
            }
            
            await _userManager.AddToRoleAsync(customer, "Customer");
            
            return new RoBoardCustomerDto
            {
                UserId = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Birthday = customer.Birthday,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber
            };
        }

        public async Task<RoBoardCustomerDto> QuickCreateCustomerAsync(QuickCreateCustomerDto quickCreateCustomerDto)
        {
            // Start database transaction
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            
            try
            {
                var validationContext = new ValidationContext(quickCreateCustomerDto);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(quickCreateCustomerDto, validationContext, validationResults, true))
                {
                    var errorMessage = string.Join("; ", validationResults.Select(vr => vr.ErrorMessage));
                    throw new ValidationException($"Invalid customer data: {errorMessage}");
                }
                
                // Validate birthday is not in the future
                if (quickCreateCustomerDto.Birthday.HasValue && quickCreateCustomerDto.Birthday.Value.Date > DateTime.UtcNow.Date)
                {
                    throw new ValidationException("Birthday cannot be in the future");
                }
                
                // Check if customer with same phone number already exists
                var existingCustomer = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == quickCreateCustomerDto.PhoneNumber);
                if (existingCustomer != null)
                {
                    throw new InvalidOperationException("Customer with this phone number already exists");
                }
                
                // Check if customer with same email already exists (if email is provided)
                if (!string.IsNullOrEmpty(quickCreateCustomerDto.Email))
                {
                    var existingEmailCustomer = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.Email == quickCreateCustomerDto.Email);
                    if (existingEmailCustomer != null)
                    {
                        throw new InvalidOperationException("Customer with this email already exists");
                    }
                }
                
                // Create new ApplicationUser with minimal info
                var customer = new ApplicationUser
                {
                    UserName = quickCreateCustomerDto.PhoneNumber,
                    PhoneNumber = quickCreateCustomerDto.PhoneNumber,
                    FirstName = quickCreateCustomerDto.FirstName,
                    LastName = quickCreateCustomerDto.LastName,
                    Email = quickCreateCustomerDto.Email,
                    Birthday = quickCreateCustomerDto.Birthday,
                    DateOfBirth = quickCreateCustomerDto.Birthday,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };
                
                // Set default password
                var defaultPassword = "Garagepro123!";
                
                var result = await _userManager.CreateAsync(customer, defaultPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create customer: {errors}");
                }
                
                // Add to Customer role
                await _userManager.AddToRoleAsync(customer, "Customer");
                
                // Commit transaction - all database operations succeeded
                await transaction.CommitAsync();
                
                // Send welcome email if customer has email address (after transaction commit)
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    try
                    {
                        var customerFullName = $"{customer.FirstName} {customer.LastName}".Trim();
                        var subject = "Welcome to GaragePro - Your Account Information";
                        
                        var htmlMessage = await _emailTemplateService.GetWelcomeEmailTemplateAsync(
                            customerFullName,
                            customer.PhoneNumber,
                            customer.PhoneNumber,
                            customer.Email,
                            defaultPassword
                        );

                        await _emailSender.SendEmailAsync(customer.Email, subject, htmlMessage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send welcome email to {customer.Email}: {ex.Message}");
                        // Customer is still created successfully even if email fails
                    }
                }
                
                // Return the created customer DTO
                return new RoBoardCustomerDto
                {
                    UserId = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Birthday = customer.Birthday,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber
                };
            }
            catch
            {
                // Rollback transaction on any error
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<RoBoardCustomerDto> UpdateCustomerAsync(string customerId, UpdateCustomerDto updateCustomerDto)
        {
            // Validate the DTO
            var validationContext = new ValidationContext(updateCustomerDto);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(updateCustomerDto, validationContext, validationResults, true))
            {
                var errorMessage = string.Join("; ", validationResults.Select(vr => vr.ErrorMessage));
                var validationException = new ValidationException($"Invalid customer data: {errorMessage}");
                throw validationException;
            }
            
            // Additional custom validation
            if (string.IsNullOrWhiteSpace(updateCustomerDto.FirstName) || 
                string.IsNullOrWhiteSpace(updateCustomerDto.LastName))
            {
                throw new ValidationException("First name and last name are required");
            }
            
            if (string.IsNullOrWhiteSpace(updateCustomerDto.PhoneNumber))
            {
                throw new ValidationException("Phone number is required");
            }
            
            // Check if customer with same phone number already exists
            var existingCustomer = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == updateCustomerDto.PhoneNumber);
            if (existingCustomer != null && existingCustomer.Id != customerId)
            {
                throw new InvalidOperationException("Customer with this phone number already exists");
            }
            
            // Check if customer with same email already exists (if email is provided)
            if (!string.IsNullOrEmpty(updateCustomerDto.Email))
            {
                var existingEmailCustomer = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Email == updateCustomerDto.Email);
                if (existingEmailCustomer != null && existingEmailCustomer.Id != customerId)
                {
                    throw new InvalidOperationException("Customer with this email already exists");
                }
            }
            
            // Update the existing customer
            existingCustomer.PhoneNumber = updateCustomerDto.PhoneNumber;
            existingCustomer.FirstName = updateCustomerDto.FirstName;
            existingCustomer.LastName = updateCustomerDto.LastName;
            // Removed FullName assignment since it's no longer in the entity
            existingCustomer.Email = updateCustomerDto.Email;
            existingCustomer.Birthday = updateCustomerDto.Birthday;
            existingCustomer.DateOfBirth = updateCustomerDto.Birthday;
            existingCustomer.UpdatedAt = DateTime.UtcNow;
            
            var result = await _userManager.UpdateAsync(existingCustomer);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update customer: {errors}");
            }
            
            // Return the updated customer DTO
            return new RoBoardCustomerDto
            {
                UserId = existingCustomer.Id,
                FirstName = existingCustomer.FirstName,
                LastName = existingCustomer.LastName,
                Birthday = existingCustomer.Birthday,
                // Removed FullName assignment since it's no longer in the entity
                Email = existingCustomer.Email,
                PhoneNumber = existingCustomer.PhoneNumber
            };
        }
    }
}