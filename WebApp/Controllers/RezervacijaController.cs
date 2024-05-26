using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class RezervacijaController : Controller
    {
        private readonly infsusContext ctx;
        private readonly ILogger<PutovanjeController> logger;

        public RezervacijaController(infsusContext ctx, ILogger<PutovanjeController> logger)
        {
            this.ctx=ctx;
            this.logger=logger;
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var rezervacija = await ctx.Rezervacija.FindAsync(id);

            if (rezervacija == null)
            {
                return NotFound();
            }

            return View(rezervacija);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdRezervacija,IdKorisnik,Potvrdeno,IdPutovanje,BrojMjesta,DatumRezervacije")] Rezervacija rezervacija)
        {

            if (!RezervacijaExists(rezervacija.IdRezervacija))
            {
                return View(rezervacija);
            }
            else
            {
                var postojecaRezervacija = await ctx.Rezervacija.FindAsync(id);
                postojecaRezervacija.Potvrdeno = rezervacija.Potvrdeno;
                ctx.Update(postojecaRezervacija);
                await ctx.SaveChangesAsync();
                return RedirectToAction("Details", "Putovanje", new { id = rezervacija.IdPutovanje });
            }



        }

        private bool RezervacijaExists(int id)
        {
            return ctx.Rezervacija.Any(r => r.IdRezervacija == id);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var rezervacija = await ctx.Rezervacija.FindAsync(id);

            if (rezervacija == null)
            {
                return NotFound();
            }
            var idPutovanja = rezervacija.IdPutovanje;
            ctx.Rezervacija.Remove(rezervacija);
            await ctx.SaveChangesAsync();
            return RedirectToAction("Details", "Putovanje", new { id = idPutovanja });
        }

        public async Task<IActionResult> Create (int id)
        {

            var usersWithoutReservation = ctx.Korisnik
            .Where(k => !ctx.Rezervacija.Any(r => r.IdPutovanje == id && r.IdKorisnik == k.IdKorisnik))
            .ToList();

            ViewData["IdKorisnik"] = new SelectList(usersWithoutReservation, "IdKorisnik", "KorisnickoIme");

            var rezervacija = new Rezervacija
            {
                IdPutovanje = id,
                DatumRezervacije = DateTime.Today
            };

            return View(rezervacija);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create (int idPutovanje, [Bind("IdKorisnik,IdPutovanje,DatumRezervacije,BrojMjesta")] Rezervacija rezervacija)
        {

            if (ModelState.IsValid)
            {
                rezervacija.DatumRezervacije = DateTime.Now;

                ctx.Add(rezervacija);
                ctx.SaveChanges();
                return RedirectToAction("Details", "Putovanje", new { id = rezervacija.IdPutovanje });
            }

            var usersWithoutReservation = ctx.Korisnik
            .Where(k => !ctx.Rezervacija.Any(r => r.IdPutovanje == idPutovanje && r.IdKorisnik == k.IdKorisnik))
            .ToList();

            ViewData["IdKorisnik"] = new SelectList(usersWithoutReservation, "IdKorisnik", "KorisnickoIme");

            var rezervacijaReturn = new Rezervacija
            {
                IdPutovanje = idPutovanje,
                DatumRezervacije = DateTime.Today
            };

            return View(rezervacijaReturn);
        }
    }

}
