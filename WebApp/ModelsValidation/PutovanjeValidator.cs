using FluentValidation;
using WebApp.Models;

namespace WebApp.ModelsValidation
{
    public class PutovanjeValidator : AbstractValidator<Putovanje>
    {
        public PutovanjeValidator() {

            RuleFor(p => p.IdMjesto)
                .NotEmpty()
                .WithMessage("Mjesto je obavezno");
            RuleFor(p => p.Opis)
                .NotEmpty()
                .WithMessage("Opis je obavezan")
                .MaximumLength(200)
                .WithMessage("Duljina opisa mora biti najviše 5");

            RuleFor(p => p.Cijena)
                .NotEmpty()
                .WithMessage("Cijena je obavezno polje")
                .GreaterThan(0)
                .WithMessage("Cijena mora biti veća od 0");

            RuleFor(p => p.DatumPolaska)
                .NotEmpty()
                .WithMessage("Datum polaska je obavezno polje")
                .GreaterThan(DateTime.Now)
                .WithMessage("Datum polaska ne smije biti u prošlosti");

            RuleFor(p => p.DatumPovratka)
                .NotEmpty()
                .WithMessage("Datum povratka je obavezno polje")
                .GreaterThan(DateTime.Now)
                .WithMessage("Datum povratka ne smije biti u prošlosti")
                .GreaterThanOrEqualTo(p => p.DatumPolaska)
                .WithMessage("Datum povratka mora biti veći ili jednak datumu polaska");

            RuleFor(p => p.ImeprezimeVoditelj)
                .MaximumLength(50)
                .WithMessage("Ime i prezime voditelja smije biti dugo najviše 50 znakova");

        }
    }
}
