using FluentValidation;
using Services.DTOs.Requests;

namespace Services.Validators;

public class RegisterMovementRequestValidator : AbstractValidator<RegisterMovementRequest>
{
    public RegisterMovementRequestValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("La cuenta es requerida");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de movimiento inválido");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a 0");

        RuleFor(x => x.Method)
            .IsInEnum().WithMessage("Método de pago inválido");

        RuleFor(x => x.Concept)
            .NotEmpty().WithMessage("El concepto es requerido")
            .MaximumLength(300).WithMessage("El concepto no puede exceder 300 caracteres");

        RuleFor(x => x.VoucherNumber)
            .MaximumLength(50).WithMessage("El número de comprobante no puede exceder 50 caracteres");

        RuleFor(x => x.Observations)
            .MaximumLength(1000).WithMessage("Las observaciones no pueden exceder 1000 caracteres");

        RuleFor(x => x.MovementDate)
            .NotEmpty().WithMessage("La fecha del movimiento es requerida")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("La fecha no puede ser mayor a mañana");
    }
}