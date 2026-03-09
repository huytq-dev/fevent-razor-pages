namespace Application;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(ValidationMessages.Required);

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .MinimumLength(6).WithMessage(ValidationMessages.MinLength)
            .MaximumLength(100).WithMessage(ValidationMessages.MaxLength);

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .Equal(x => x.NewPassword).WithMessage(ValidationMessages.MustMatch);
    }
}
