using System.ComponentModel.DataAnnotations;

namespace EssenceMvp.Web.Services;

public static class FormValidators
{
    private static readonly EmailAddressAttribute _emailAttr = new();

    public static string? Email(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "El email es obligatorio.";
        return _emailAttr.IsValid(value) ? null : "Formato de email inválido.";
    }
}
