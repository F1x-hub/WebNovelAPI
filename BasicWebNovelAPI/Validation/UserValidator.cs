using BasicWebNovelAPI.Model.Dto.User;
using FluentValidation;

namespace BasicWebNovelAPI.Validation
{
    public class UserValidator : AbstractValidator<RegisterUserDto>
    {
        public UserValidator() 
        {
            RuleFor(u => u.UserName).NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters long")
                .MaximumLength(20).WithMessage("Username cannot exceed 20 characters");

            RuleFor(u => u.FirstName).NotEmpty().WithMessage("First name is required")
                .MinimumLength(2).WithMessage("First name must be at least 2 characters long")
                .MaximumLength(30).WithMessage("First name cannot exceed 50 characters");

            RuleFor(u => u.LastName).NotEmpty().WithMessage("Last name is required")
                .MinimumLength(2).WithMessage("Last name must be at least 2 characters long")
                .MaximumLength(30).WithMessage("Last name cannot exceed 50 characters");
            
            RuleFor(u => u.Email).NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(u => u.Password).NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                .Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).*$")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, and one number");
        }
    }
}
