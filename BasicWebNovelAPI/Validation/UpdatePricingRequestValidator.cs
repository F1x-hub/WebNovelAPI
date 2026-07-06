using BasicWebNovelAPI.Model.Dto.Coins;
using FluentValidation;

namespace BasicWebNovelAPI.Validation
{
    public class UpdatePricingRequestValidator : AbstractValidator<UpdatePricingRequest>
    {
        public UpdatePricingRequestValidator()
        {
            RuleFor(x => x.FreeChaptersCount)
                .GreaterThanOrEqualTo(0).WithMessage("Free chapters count must be 0 or more");

            RuleFor(x => x.CoinPricePerChapter)
                .InclusiveBetween(1, 10).WithMessage("Price per chapter must be between 1 and 10 coins");

            RuleFor(x => x.UnlockIntervalDays)
                .InclusiveBetween(1, 30).WithMessage("Unlock interval days must be between 1 and 30 days");
        }
    }
}
