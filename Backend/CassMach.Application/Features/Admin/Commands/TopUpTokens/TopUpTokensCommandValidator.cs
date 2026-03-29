using FluentValidation;

namespace CassMach.Application.Features.Admin.Commands.TopUpTokens
{
    public class TopUpTokensCommandValidator : AbstractValidator<TopUpTokensCommand>
    {
        public TopUpTokensCommandValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("UserId must be greater than 0.");

            RuleFor(x => x.CreditAmount)
                .GreaterThan(0).WithMessage("CreditAmount must be greater than 0.");
        }
    }
}
