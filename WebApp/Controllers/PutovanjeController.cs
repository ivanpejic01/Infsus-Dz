using WebApp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection.Metadata;
using WebApp.Models;
using WebApp.ViewModels;


namespace WebApp.Controllers
{
    public class PutovanjeController : Controller
    {

        private readonly infsusContext ctx;
        private readonly ILogger<PutovanjeController> logger;


        public PutovanjeController(infsusContext ctx, ILogger<PutovanjeController> logger)
        {
            this.ctx=ctx;
            this.logger=logger;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["IdMjesto"] = new SelectList(ctx.Mjesto, "IdMjesto", "Naziv");
            ViewData["IdSmjestaj"] = new SelectList(ctx.Smjestaj, "IdSmjestaj", "Naziv");
            ViewData["IdVozilo"] = new SelectList(ctx.Vozilo, "IdVozila", "VrstaVozila");
            ViewData["IdVrstaPutovanja"] = new SelectList(ctx.VrstaPutovanja, "IdVrste", "NazivVrstePut");
            ViewData["IdVrstaSobe"] = new SelectList(ctx.Soba, "IdSobe", "VrstaSobe");

            var rateOptions = new List<SelectListItem>
            {
                new SelectListItem {Value = "da", Text = "da"},
                new SelectListItem {Value = "ne", Text = "ne"}
            };

            ViewData["Rate"] = new SelectList(rateOptions, "Value", "Text");
            return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdMjesto,IdSmjestaj,IdVrstaPutovanja,IdVozilo,Opis,ImeprezimeVoditelj,Cijena,DatumPolaska,DatumPovratka,Rate")] Putovanje putovanje)
        {
            if (ModelState.IsValid)
            {
                ctx.Add(putovanje);
                await ctx.SaveChangesAsync();
                TempData[Constants.Message] = "Uspješno dodano putovanje";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Index));
            }

            ViewData["IdMjesto"] = new SelectList(ctx.Mjesto, "IdMjesto", "Naziv");
            ViewData["IdSmjestaj"] = new SelectList(ctx.Smjestaj, "IdSmjestaj", "Naziv");
            ViewData["IdVozilo"] = new SelectList(ctx.Vozilo, "IdVozila", "VrstaVozila");
            ViewData["IdVrstaPutovanja"] = new SelectList(ctx.VrstaPutovanja, "IdVrste", "NazivVrstePut");

            var rateOptions = new List<SelectListItem>
            {
                new SelectListItem {Value = "da", Text = "da"},
                new SelectListItem {Value = "ne", Text = "ne"}
            };

            ViewData["Rate"] = new SelectList(rateOptions, "Value", "Text");
            TempData[Constants.Message] = "Neuspješno dodano putovanje, dogodila se greška!";
            TempData[Constants.ErrorOccurred] = true;

            return View(putovanje);
        }

    
        public async Task<IActionResult> Delete(int id)
        {
            var putovanje = await ctx.Putovanje.FindAsync(id);
            if (putovanje == null)
            {
                return NotFound();
            }

            try
            {
                ctx.Putovanje.Remove(putovanje);
                await ctx.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                // logger.LogError(ex, "An error occurred while deleting the record.");

                // Pass the error message to the Index action
                TempData[Constants.Message] = "Došlo je do greške prilikom brisanja zapisa.";
                TempData[Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var putovanje = await ctx.Putovanje.FindAsync(id);
            if (putovanje == null)
            {
                return NotFound();
            }


            ViewData["IdMjesto"] = new SelectList(ctx.Mjesto, "IdMjesto", "Naziv", putovanje.IdMjesto);
            ViewData["IdSmjestaj"] = new SelectList(ctx.Smjestaj, "IdSmjestaj", "Naziv", putovanje.IdSmjestaj);
            ViewData["IdVozilo"] = new SelectList(ctx.Vozilo, "IdVozila", "VrstaVozila", putovanje.IdVozilo);
            ViewData["IdVrstaPutovanja"] = new SelectList(ctx.VrstaPutovanja, "IdVrste", "NazivVrstePut", putovanje.IdVrstaPutovanja);
            var rateOptions = new List<SelectListItem>
            {
                new SelectListItem {Value = "da", Text = "da"},
                new SelectListItem {Value = "ne", Text = "ne"}
            };

            ViewData["Rate"] = new SelectList(rateOptions, "Value", "Text", putovanje.Rate);

            return View(putovanje);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPutovanja","IdMjesto,IdSmjestaj,IdVrstaPutovanja,IdVozilo,Opis,ImeprezimeVoditelj,Cijena,DatumPolaska,DatumPovratka,Rate")] Putovanje putovanje)
        {
            if (id != putovanje.IdPutovanja)
            {
                return RedirectToAction(nameof(Edit));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(putovanje);
                    await ctx.SaveChangesAsync();
                }
                catch(Exception)
                {
                    if (!PutovanjeExists(putovanje.IdPutovanja))
                    {
                        TempData["ErrorMessage"] = "Došlo je do greške prilikom uređivanja putovanja.";
                        return RedirectToAction(nameof(Index));
                    } else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            return View(putovanje);
        }

        private bool PutovanjeExists(int id)
        {
            return ctx.Putovanje.Any(p => p.IdPutovanja == id);
        }
        public async Task<IActionResult> Index(DateTime? datumPolaska, DateTime? datumPovratka, int page = 1, int pageSize = 4)
        {
            var query = ctx.Putovanje.AsNoTracking();

            if (datumPolaska != null)
            {
                query = query.Where(p => p.DatumPolaska >= datumPolaska);
            }

            if (datumPovratka != null)
            {
                query = query.Where(p => p.DatumPovratka <= datumPovratka);
            }



  
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var putovanjaStranica = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var putovanje = putovanjaStranica.Select(o => new PutovanjeViewModel
            {
                IdPutovanja = o.IdPutovanja,
                Opis = o.Opis,
                Cijena = o.Cijena,
                DatumPolaska = o.DatumPolaska,
                DatumPovratka = o.DatumPovratka

            });

            var model = new PutovanjaViewModel
            {
                putovanja = putovanje,
                CurrentPageNumber = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalCount = totalCount,
                PageNumber = page

            };

            return View(model);

            


            
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Putovanje putovanje =  ctx.Putovanje
                                  .Include("IdMjestoNavigation")
                                  .Include("IdSmjestajNavigation")
                                  .Include("IdVoziloNavigation")
                                  .Include("IdVrstaPutovanjaNavigation")
                                  .Include(p => p.IdSmjestajNavigation)
                                  .ThenInclude(s => s.IdSobaNavigation)
                                  .Include(p => p.IdSmjestajNavigation)
                                  .ThenInclude(s => s.IdObrokNavigation)
                                  .Include(p => p.Rezervacijas)
                                  .ThenInclude(r => r.IdKorisnikNavigation)
                                  .Include(p => p.IdSmjestajNavigation)
                                  .ThenInclude(s => s.IdVrstaSmjestajaNavigation)
                                   .FirstOrDefault(p => p.IdPutovanja == id);

            if (putovanje == null)
            {
                return NotFound();
            }
            return View(putovanje);
        }


        
    }
}
