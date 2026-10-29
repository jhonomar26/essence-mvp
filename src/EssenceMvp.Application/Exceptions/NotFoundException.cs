namespace EssenceMvp.Application.Exceptions;

// El recurso no existe o no pertenece al usuario: ambos casos se responden igual (ADR-0001).
public class NotFoundException : Exception
{
    public NotFoundException(string message = "Not found") : base(message) { }
}
