using FluentValidation;

namespace CassMach.Application.Features.Admin.Commands.GiftTokens
{
    public class GiftTokensCommandValidator : AbstractValidator<GiftTokensCommand>
    {
        public GiftTokensCommandValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("UserId must be greater than 0.");

            RuleFor(x => x.CreditAmount)
                .GreaterThan(0).WithMessage("CreditAmount must be greater than 0.");
        }
    }
}
