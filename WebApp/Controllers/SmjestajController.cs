using WebApp.Models;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApp.ViewModels;
using System.Text.Json;
using System.Runtime.CompilerServices;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Azure;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Extensions;


namespace WebApp.Controllers
{

    public class SmjestajController : Controller
    {
        private readonly infsusContext ctx;
        private readonly ILogger<SmjestajController> logger;

        public SmjestajController(infsusContext ctx, ILogger<SmjestajController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var query = ctx.Smjestaj.AsNoTracking();
            var smjestaj = query.Select(o => new SmjestajViewModel
            {
                idSmjestaj = o.IdSmjestaj,
                naziv = o.Naziv,
                vrstaObroka = o.IdObrokNavigation.VrstaObroka,
                vrstaSmjestaja = o.IdVrstaSmjestajaNavigation.VrstaSmjestaja1,
                vrstaSobe = o.IdSobaNavigation.VrstaSobe
            });

            var model = new SmjestajiViewModel
            {
                smjestaji = smjestaj,
            };

            return View(model);

        }

        public async Task<IActionResult> Search(string? naziv)
        {
            Console.WriteLine("aha");

            var query = ctx.Smjestaj.AsNoTracking();

            if (naziv != null)
            {
                Console.WriteLine("usao sam u ovo");
                query = ctx.Smjestaj.Where(p => p.Naziv.Equals(naziv));

                bool postoji = await ctx.Smjestaj.AnyAsync(m => m.Naziv == naziv);

                if (!postoji)
                {
                    Console.WriteLine("nemam takav");
                    TempData[Constants.Message] = $"Ne postoji smještaj s tim imenom!!";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }

                var smjestaj = query.Select(o => new SmjestajViewModel
                {
                    idSmjestaj = o.IdSmjestaj,
                    naziv = o.Naziv,
                    vrstaObroka = o.IdObrokNavigation.VrstaObroka,
                    vrstaSmjestaja = o.IdVrstaSmjestajaNavigation.VrstaSmjestaja1,
                    vrstaSobe = o.IdSobaNavigation.VrstaSobe
                });

                var model = new SmjestajiViewModel
                {
                    smjestaji = smjestaj,
                };

                return View(model);
            }
            else
            {
                Console.WriteLine("nemam takav");
                TempData[Constants.Message] = $"Polje je prazno!!";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Index));
            }




        }


        //stvaranje nove vrste-GET
        [HttpGet]
        public IActionResult Create()
        {
            var obroci = ctx.Obrok.ToList();
            ViewBag.Obroci = new SelectList(obroci, "IdObroka", "VrstaObroka");
            var vrstaSmjestaja1 = ctx.VrstaSmjestaja.ToList();
            ViewBag.VrstaSmjestaja = new SelectList(vrstaSmjestaja1, "IdVrstaSmjestaja", "VrstaSmjestaja1");
            var sobe = ctx.Soba.ToList();
            ViewBag.Sobe = new SelectList(sobe, "IdSobe", "VrstaSobe");
            return View();
        }

        //stvaranje nove vrste-POST
        [HttpPost]
        [ValidateAntiForgeryToken]  
        public IActionResult Create(Smjestaj smjestaj)
        {
            if (ModelState.IsValid)
            {        
                try
                {

                    var postojeciSmjestaj = ctx.Smjestaj.AsNoTracking().FirstOrDefault(m => m.Naziv.Equals(smjestaj.Naziv));
                    if (postojeciSmjestaj != null)
                    {
                        Console.WriteLine("isti naziv");
                        ModelState.AddModelError("Naziv", "Naziv već postoji u bazi podataka.");

                        ViewData["Obroci"] = new SelectList(ctx.Obrok, "IdObroka", "VrstaObroka", smjestaj.IdObrok);
                        ViewData["Sobe"] = new SelectList(ctx.Soba, "IdSobe", "VrstaSobe", smjestaj.IdSoba);
                        ViewData["VrstaSmjestaja"] = new SelectList(ctx.VrstaSmjestaja, "IdVrstaSmjestaja", "VrstaSmjestaja1", smjestaj.IdVrstaSmjestaja);
                        return View(smjestaj);

                    }

                    ctx.Add(smjestaj);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Smještaj naziva {smjestaj.Naziv} dodan!");

                    TempData[Constants.Message] = $"Smještaj naziva {smjestaj.Naziv} dodan!";
                    TempData[Constants.ErrorOccurred] = false;
                    Console.WriteLine("Dodana!");
                    return RedirectToAction(nameof(Index));

                }

                catch (Exception exc)
                {
                    Console.WriteLine("Pogreška se dogodila!");
                    logger.LogError("Pogreška prilikom kreiranja smještaja");
                    TempData[Constants.Message] = $"Morate ispuniti sva polja!";
                    TempData[Constants.ErrorOccurred] = false;
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    var obroci = ctx.Obrok.ToList();
                    ViewBag.Obroci = new SelectList(obroci, "IdObroka", "VrstaObroka");
                    var vrstaSmjestaja1 = ctx.VrstaSmjestaja.ToList();
                    ViewBag.VrstaSmjestaja = new SelectList(vrstaSmjestaja1, "IdVrstaSmjestaja", "VrstaSmjestaja1");
                    var sobe = ctx.Soba.ToList();
                    ViewBag.Sobe = new SelectList(sobe, "IdSobe", "VrstaSobe");
                    return View();
                }
            }
            else
            {
                var obroci = ctx.Obrok.ToList();
                ViewBag.Obroci = new SelectList(obroci, "IdObroka", "VrstaObroka");
                var vrstaSmjestaja1 = ctx.VrstaSmjestaja.ToList();
                ViewBag.VrstaSmjestaja = new SelectList(vrstaSmjestaja1, "IdVrstaSmjestaja", "VrstaSmjestaja1");
                var sobe = ctx.Soba.ToList();
                ViewBag.Sobe = new SelectList(sobe, "IdSobe", "VrstaSobe");
                return View(smjestaj);
            }
        }

        public IActionResult Delete(int idSmjestaja)
        {
            var brisanje = ctx.Smjestaj.AsNoTracking()
                           .Where(smjestaj => smjestaj.IdSmjestaj == idSmjestaja).SingleOrDefault();
            if (brisanje != null)
            {
                try
                {
                    string naziv = brisanje.Naziv;
                    ctx.Remove(brisanje);
                    ctx.SaveChanges();
                    logger.LogInformation($"Smještaj {naziv} uspješno obrisan");
                    TempData[Constants.Message] = $"Smještaj {naziv} uspješno obrisan";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Greška prilikom brisanja!";
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError("Greška prilikom brisanja!");
                }
            }
            else
            {
                logger.LogWarning("Ne postoji smještaj s ID-jem {0}", idSmjestaja);
                TempData[Constants.Message] = "Ne postoji smještaj s ID-jem " + idSmjestaja;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var smjestaj = await ctx.Smjestaj.FindAsync(id);
            if (smjestaj == null)
            {
                return NotFound();
            }

            Console.WriteLine("usao sam");
            ViewData["IdObrok"] = new SelectList(ctx.Obrok, "IdObroka", "VrstaObroka", smjestaj.IdObrok);
            ViewData["IdSoba"] = new SelectList(ctx.Soba, "IdSobe", "VrstaSobe", smjestaj.IdSoba);
            ViewData["IdVrstaSmjestaja"] = new SelectList(ctx.VrstaSmjestaja, "IdVrstaSmjestaja", "VrstaSmjestaja1", smjestaj.IdVrstaSmjestaja);
          

            return View(smjestaj);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdSmjestaj", "IdObrok,IdSoba,IdVrstaSmjestaja,Naziv")] Smjestaj smjestaj)
        {
            if (id != smjestaj.IdSmjestaj)
            {
                return RedirectToAction(nameof(Edit));
            }

            if (ModelState.IsValid)
            {

                bool nazivExists = await ctx.Smjestaj
                .AnyAsync(s => s.Naziv == smjestaj.Naziv && s.IdSmjestaj != smjestaj.IdSmjestaj);

                if (nazivExists)
                {
                    ModelState.AddModelError("Naziv", "Naziv već postoji u bazi podataka.");
                    ViewData["IdObrok"] = new SelectList(ctx.Obrok, "IdObroka", "VrstaObroka", smjestaj.IdObrok);
                    ViewData["IdSoba"] = new SelectList(ctx.Soba, "IdSobe", "VrstaSobe", smjestaj.IdSoba);
                    ViewData["IdVrstaSmjestaja"] = new SelectList(ctx.VrstaSmjestaja, "IdVrstaSmjestaja", "VrstaSmjestaja1", smjestaj.IdVrstaSmjestaja);
                    return View(smjestaj);
                }
                try
                {
                    ctx.Update(smjestaj);
                    await ctx.SaveChangesAsync();
                    TempData[Constants.Message] = "Ažuriran smještaj!";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception)
                {
                    if (!SmjestajExists(smjestaj.IdSmjestaj))
                    {
                        TempData["ErrorMessage"] = "Došlo je do greške prilikom uređivanja smještaja.";
                        TempData[Constants.Message] = "Ažuriran smještaj!";
                        TempData[Constants.ErrorOccurred] = true;
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["IdObrok"] = new SelectList(ctx.Obrok, "IdObroka", "VrstaObroka", smjestaj.IdObrok);
            ViewData["IdSoba"] = new SelectList(ctx.Soba, "IdSobe", "VrstaSobe", smjestaj.IdSoba);
            ViewData["IdVrstaSmjestaja"] = new SelectList(ctx.VrstaSmjestaja, "IdVrstaSmjestaja", "VrstaSmjestaja1", smjestaj.IdVrstaSmjestaja);
            return View(smjestaj);
        }


        private bool SmjestajExists(int id)
        {
            return ctx.Smjestaj.Any(p => p.IdSmjestaj == id);
        }



    }
}
