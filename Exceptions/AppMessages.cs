namespace Exceptions;

public static class AppMessages
{
    public static class Auth
    {
        public const string InvalidCredentials = "Email o contraseña incorrectos";
        public const string UserDisabled = "Usuario desactivado. Contactá al administrador.";
        public const string CompanyInactive = "La empresa no está activa";
        public const string WrongCurrentPassword = "Contraseña actual incorrecta";
        public const string SlugAlreadyInUse = "El slug de empresa ya está en uso. Elegí otro.";
        public const string EmailAlreadyRegistered = "El email ya está registrado en esta empresa";
        public const string EmailAlreadyInUse = "El email ya está en uso en esta empresa";
        public const string InvalidRefreshToken = "Token de refresco inválido o expirado";
    }

    public static class User
    {
        public const string NotFound = "Usuario no encontrado";
    }

    public static class Ticket
    {
        public const string NotFound = "Ticket no encontrado";
        public const string AssignedUserNotFound = "Usuario asignado no encontrado";
        public const string InvalidStatusTransition = "La transición de estado no está permitida";
    }

    public static class Client
    {
        public const string NotFound = "Cliente no encontrado";
    }

    public static class Movement
    {
        public const string NotFound = "Movimiento no encontrado";
    }

    public static class Account
    {
        public const string NotFound = "Cuenta no encontrada";
        public const string Inactive = "La cuenta está desactivada y no puede recibir movimientos";
    }

    public static class Company
    {
        public const string NotFound = "Empresa no encontrada";
        public const string SlugAlreadyInUse = "El slug ya está en uso";
    }

    public static class CashRegister
    {
        public const string NotFound = "Registro de caja no encontrado";
        public const string AlreadyOpenToday = "Ya existe una apertura de caja para hoy";
        public const string AlreadyClosed = "Este registro ya fue cerrado";
        public const string NotOpenForDate = "No se puede registrar un movimiento sin haber abierto la caja del día";
    }

    public static class Subscription
    {
        public const string NotFound = "Suscripción no encontrada";
    }

    public static class Budget
    {
        public const string NotFound = "Presupuesto no encontrado";
        public const string CreateError = "Error al crear el presupuesto";
        public const string UpdateError = "Error al actualizar el presupuesto";
        public const string DeleteError = "Error al eliminar el presupuesto";
        public const string PdfGenerationError = "Error al generar el PDF del presupuesto";
        public const string NoItems = "El presupuesto debe tener al menos un item";
        public const string InvalidTotal = "Los totales del presupuesto no son válidos";
    }
}
