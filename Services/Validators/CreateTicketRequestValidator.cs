using FluentValidation;
using Services.DTOs.Requests;

namespace Services.Validators;

public class CreateTicketRequestValidator : AbstractValidator<CreateTicketRequest>
{
    public CreateTicketRequestValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("El cliente es requerido");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de ticket inválido");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Prioridad inválida");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido")
            .MaximumLength(200).WithMessage("El título no puede exceder 200 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("La descripción no puede exceder 2000 caracteres");

        RuleFor(x => x.Address)
            .MaximumLength(300).WithMessage("La dirección no puede exceder 300 caracteres");
    }
}