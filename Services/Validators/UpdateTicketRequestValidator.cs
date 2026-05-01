using FluentValidation;
using Models.Enums;
using Services.DTOs.Requests;

namespace Services.Validators;

public class UpdateTicketRequestValidator : AbstractValidator<UpdateTicketRequest>
{
    public UpdateTicketRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido")
            .MinimumLength(5).WithMessage("El título debe tener al menos 5 caracteres")
            .MaximumLength(200).WithMessage("El título no puede exceder 200 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("La descripción no puede exceder 2000 caracteres");

        RuleFor(x => x.Address)
            .MaximumLength(300).WithMessage("La dirección no puede exceder 300 caracteres");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("El tipo de ticket no es válido");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("La prioridad no es válida");
    }
}
