namespace Application;

public class SignUpRequestValidator : AbstractValidator<SignUpRequest>
{
    public SignUpRequestValidator()
    {
        RuleFor(x => x.Fullname)
            .NotEmpty().WithMessage(ValidationMessages.Required);

        //RuleFor(x => x.Username)
        //    .NotEmpty().WithMessage(ValidationMessages.Required)
        //    .MinimumLength(3).WithMessage(ValidationMessages.MinLength)
        //    .MaximumLength(50).WithMessage(ValidationMessages.MaxLength)
        //    .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("Username can only contain letters, numbers, underscores, and hyphens");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .EmailAddress().WithMessage(ValidationMessages.InvalidEmail);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .MinimumLength(6).WithMessage(ValidationMessages.MinLength);

        //RuleFor(x => x.SchoolName)
        //    .NotEmpty().WithMessage(ValidationMessages.Required);
    }
}
