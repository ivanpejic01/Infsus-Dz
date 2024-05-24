using FluentValidation;
using WebApp.Models;
using Microsoft.EntityFrameworkCore;


namespace WebApp.ModelsValidation
{
    public class SmjestajValidator : AbstractValidator<Smjestaj>
    {

        public SmjestajValidator()
        {
            RuleFor(d => d.Naziv)
                .NotEmpty().WithMessage("Naziv mjesta je obavezno polje!")
                .MaximumLength(50).WithMessage("Naziv mjesta ne može biti dulji od 50 znakova!");
        }



    }

}
