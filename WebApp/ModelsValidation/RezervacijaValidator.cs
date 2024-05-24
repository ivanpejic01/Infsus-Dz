using FluentValidation;
using WebApp.Models;

namespace WebApp.ModelsValidation
{
    public class RezervacijaValidator : AbstractValidator<Rezervacija>
    {

        public RezervacijaValidator()
        {
            RuleFor(r => r.BrojMjesta)
                .NotEmpty()
                .WithMessage("Broj mjesta je obavezno polje")
                .GreaterThan(0)
                .WithMessage("Broj mjesta mora biti veći od 0");

        }
    }
}
