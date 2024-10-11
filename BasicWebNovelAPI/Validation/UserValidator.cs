using BasicWebNovelAPI.Model.Dto.User;
using FluentValidation;

namespace BasicWebNovelAPI.Validation
{
    public class UserValidator : AbstractValidator<GetUserDto>
    {
        public UserValidator() 
        {
            RuleFor(u => u.Name).NotEmpty().WithMessage("name is required!")
                .MaximumLength(10).MinimumLength(4).WithMessage("name's length must be minimum 4 and maximum 10");

            RuleFor(u => u.Age).NotEqual(0).GreaterThan(18).LessThan(50).WithMessage("age must be minimum 18");

            RuleFor(u => u.Email).EmailAddress();

            RuleFor(u => u.Phone).MinimumLength(9).MaximumLength(9).WithMessage("number's length must be 9");

            RuleFor(u => u.Password).Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).*$");
        }
        
    }
}
