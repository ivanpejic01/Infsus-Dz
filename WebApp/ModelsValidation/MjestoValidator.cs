using FluentValidation;
using WebApp.Models;
using Microsoft.EntityFrameworkCore;


namespace WebApp.ModelsValidation
{
    public class MjestoValidator : AbstractValidator<Mjesto>
    {

        public MjestoValidator()
        {
            RuleFor(d => d.Naziv)
                .NotEmpty().WithMessage("Naziv mjesta je obavezno polje!")
                .MaximumLength(50).WithMessage("Naziv mjesta ne može biti dulji od 50 znakova!");

            RuleFor(d => d.Postanskibroj)
                .NotEmpty().WithMessage("Poštanski broj je obavezno polje!")
                .GreaterThan(0).WithMessage("Poštanski broj mora biti pozitivan")
                .Must(w => w.ToString().Length == 5).WithMessage("Poštanski broj mora sadržavati točno 5 znamenki!");
        }



    }

}
