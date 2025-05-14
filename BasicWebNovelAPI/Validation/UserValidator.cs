using BasicWebNovelAPI.Model.Dto.User;
using FluentValidation;

namespace BasicWebNovelAPI.Validation
{
    public class UserValidator : AbstractValidator<RegisterUserDto>
    {
        public UserValidator() 
        {
            RuleFor(u => u.FirstName).NotEmpty().WithMessage("name is required!")
                .MaximumLength(10).MinimumLength(4).WithMessage("first name's length must be minimum 4 and maximum 15");

            RuleFor(u => u.LastName).NotEmpty().WithMessage("name is required!")
                .MaximumLength(10).MinimumLength(4).WithMessage("last name's length must be minimum 4 and maximum 15");

            
            RuleFor(u => u.Email).EmailAddress();

            RuleFor(u => u.Password).Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).*$");
        }
        
    }
}
