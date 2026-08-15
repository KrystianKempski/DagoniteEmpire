# Wątpliwości i pytania do lokalizacji (PL)

> Tu zbieram pytania/niejasności napotkane w trakcie tłumaczenia.
> Dopisuj odpowiedzi pod pytaniami (np. w linii `ODP:`), a ja je uwzględnię.

## Status: otwarte

<!-- Format:
### [Faza X] Krótki tytuł — plik/miejsce
Pytanie...
ODP:
-->

### [Faza 2] Komunikaty walidacji i błędów z frameworka (Identity / DataAnnotations)
Domyślne komunikaty ASP.NET Core są po angielsku i NIE są tekstem w naszych plikach:
- Błędy hasła/rejestracji z `IdentityErrorDescriber` (np. „Passwords must have at least one digit").
- Walidacja `DataAnnotations` bez własnego `ErrorMessage` (np. „The Email field is not a valid e-mail address").
Aby je spolszczyć, trzeba dodać (a) własny `IdentityErrorDescriber` po polsku
oraz (b) lokalizację DataAnnotations lub jawne `ErrorMessage` przy każdym polu.
Proponuję zrobić to w osobnej mini-turze (Faza 2b) albo w Fazie 10 (QA).
ODP:

---

## Rozstrzygnięte
<!-- Przenoszę tu pytania z odpowiedziami, żeby był ślad decyzji. -->
