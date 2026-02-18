using FluentValidation;

namespace ResourceBooking.Application.Resources.Commands.CreateResource;

public class CreateResourceCommandValidator : AbstractValidator<CreateResourceCommand>
{
    public CreateResourceCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(c => c.Description)
            .MaximumLength(1000);
    }
}
