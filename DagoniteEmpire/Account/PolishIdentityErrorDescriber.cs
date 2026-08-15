using Microsoft.AspNetCore.Identity;

namespace DagoniteEmpire.Account;

/// <summary>
/// Polskie komunikaty błędów ASP.NET Core Identity (reguły haseł, konta, role).
/// </summary>
public class PolishIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError()
        => new() { Code = nameof(DefaultError), Description = "Wystąpił nieznany błąd." };

    public override IdentityError ConcurrencyFailure()
        => new() { Code = nameof(ConcurrencyFailure), Description = "Wystąpił konflikt równoczesnych zmian. Obiekt został zmodyfikowany przez inny proces." };

    public override IdentityError PasswordMismatch()
        => new() { Code = nameof(PasswordMismatch), Description = "Nieprawidłowe hasło." };

    public override IdentityError InvalidToken()
        => new() { Code = nameof(InvalidToken), Description = "Nieprawidłowy token." };

    public override IdentityError RecoveryCodeRedemptionFailed()
        => new() { Code = nameof(RecoveryCodeRedemptionFailed), Description = "Wykorzystanie kodu odzyskiwania nie powiodło się." };

    public override IdentityError LoginAlreadyAssociated()
        => new() { Code = nameof(LoginAlreadyAssociated), Description = "Istnieje już użytkownik powiązany z tym logowaniem." };

    public override IdentityError InvalidUserName(string? userName)
        => new() { Code = nameof(InvalidUserName), Description = $"Nazwa użytkownika „{userName}” jest nieprawidłowa — może zawierać tylko litery i cyfry." };

    public override IdentityError InvalidEmail(string? email)
        => new() { Code = nameof(InvalidEmail), Description = $"Adres e-mail „{email}” jest nieprawidłowy." };

    public override IdentityError DuplicateUserName(string userName)
        => new() { Code = nameof(DuplicateUserName), Description = $"Nazwa użytkownika „{userName}” jest już zajęta." };

    public override IdentityError DuplicateEmail(string email)
        => new() { Code = nameof(DuplicateEmail), Description = $"Adres e-mail „{email}” jest już zajęty." };

    public override IdentityError InvalidRoleName(string? role)
        => new() { Code = nameof(InvalidRoleName), Description = $"Nazwa roli „{role}” jest nieprawidłowa." };

    public override IdentityError DuplicateRoleName(string role)
        => new() { Code = nameof(DuplicateRoleName), Description = $"Nazwa roli „{role}” jest już zajęta." };

    public override IdentityError UserAlreadyHasPassword()
        => new() { Code = nameof(UserAlreadyHasPassword), Description = "Użytkownik ma już ustawione hasło." };

    public override IdentityError UserLockoutNotEnabled()
        => new() { Code = nameof(UserLockoutNotEnabled), Description = "Blokada konta nie jest włączona dla tego użytkownika." };

    public override IdentityError UserAlreadyInRole(string role)
        => new() { Code = nameof(UserAlreadyInRole), Description = $"Użytkownik należy już do roli „{role}”." };

    public override IdentityError UserNotInRole(string role)
        => new() { Code = nameof(UserNotInRole), Description = $"Użytkownik nie należy do roli „{role}”." };

    public override IdentityError PasswordTooShort(int length)
        => new() { Code = nameof(PasswordTooShort), Description = $"Hasło musi mieć co najmniej {length} znaków." };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
        => new() { Code = nameof(PasswordRequiresUniqueChars), Description = $"Hasło musi zawierać co najmniej {uniqueChars} różnych znaków." };

    public override IdentityError PasswordRequiresNonAlphanumeric()
        => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Hasło musi zawierać co najmniej jeden znak niealfanumeryczny." };

    public override IdentityError PasswordRequiresDigit()
        => new() { Code = nameof(PasswordRequiresDigit), Description = "Hasło musi zawierać co najmniej jedną cyfrę (0–9)." };

    public override IdentityError PasswordRequiresLower()
        => new() { Code = nameof(PasswordRequiresLower), Description = "Hasło musi zawierać co najmniej jedną małą literę (a–z)." };

    public override IdentityError PasswordRequiresUpper()
        => new() { Code = nameof(PasswordRequiresUpper), Description = "Hasło musi zawierać co najmniej jedną wielką literę (A–Z)." };
}
