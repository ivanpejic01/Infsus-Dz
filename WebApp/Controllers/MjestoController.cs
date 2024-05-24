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
using WebApp.Extensions;


namespace WebApp.Controllers
{

    public class MjestoController : Controller
    {
        private readonly infsusContext ctx;
        private readonly ILogger<MjestoController> logger;

        public MjestoController(infsusContext ctx, ILogger<MjestoController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            Console.WriteLine("aha");

            var query = ctx.Mjesto.AsNoTracking();

            var mjesto = query.Select(o => new MjestoViewModel
            {
                idMjesto = o.IdMjesto,
                naziv = o.Naziv,
                postanskibroj = o.Postanskibroj

            });
            var model = new MjestaViewModel
            {
                mjesta = mjesto,
            };

            return View("Index", model);

        }


        public async Task<IActionResult> Search(string? naziv)
        {
            Console.WriteLine("aha");

            var query = ctx.Mjesto.AsNoTracking();

            if (naziv != null)
            {
                Console.WriteLine("usao sam u ovo");
                query = ctx.Mjesto.Where(p => p.Naziv.Equals(naziv));

                bool postoji = await ctx.Mjesto.AnyAsync(m => m.Naziv == naziv);

                if (!postoji)
                {
                    Console.WriteLine("nemam takav");
                    TempData[Constants.Message] = $"Ne postoji mjesto s tim imenom!!";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }

                var mjesto = query.Select(o => new MjestoViewModel
                {
                    idMjesto = o.IdMjesto,
                    naziv = o.Naziv,
                    postanskibroj = o.Postanskibroj

                });
                var model = new MjestaViewModel
                {
                    mjesta = mjesto,
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
            return View();
        }

        //stvaranje nove vrste-POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Mjesto mjesto)
        {
            logger.LogTrace(JsonSerializer.Serialize(mjesto));
            if (ModelState.IsValid)
            {
                var postojeceMjesto1 = ctx.Mjesto.AsNoTracking().FirstOrDefault(m => m.Naziv.Equals(mjesto.Naziv));
                if (postojeceMjesto1 != null)
                {
                    ModelState.AddModelError("Naziv", "Naziv već postoji u bazi podataka.");
                    return View(); ;
                }
                var postojeceMjesto = ctx.Mjesto.AsNoTracking().FirstOrDefault(m => m.Postanskibroj == mjesto.Postanskibroj);
                if (postojeceMjesto != null)
                {
                    ModelState.AddModelError("Postanskibroj", "Poštanski broj već postoji u bazi podataka.");
                    return View(); ;
                }
                try
                {

                    ctx.Add(mjesto);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Mjesto naziva {mjesto.Naziv} dodano!");

                    TempData[Constants.Message] = $"Mjesto naziva {mjesto.Naziv} dodano!";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));

                }

                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom kreiranja mjesta");
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(mjesto);
                }
            }
            else
            {
                return View(mjesto);
            }
        }

        public IActionResult Delete(int idMjesta)
        {
            var brisanje = ctx.Mjesto.AsNoTracking()
                           .Where(Mjesto => Mjesto.IdMjesto == idMjesta).SingleOrDefault();
            if (brisanje != null)
            {
                try
                {
                    string naziv = brisanje.Naziv;
                    ctx.Remove(brisanje);
                    ctx.SaveChanges();
                    logger.LogInformation($"Mjesto naziva {naziv} uspješno obrisano");
                    TempData[Constants.Message] = $"Mjesto naziva {naziv} uspješno obrisano";
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
                logger.LogWarning("Ne postoji mjesto s ID-jem {0}", idMjesta);
                TempData[Constants.Message] = "Ne postoji mjesto s ID-jem " + idMjesta;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index));
        }

        //azuriranje GET
        [HttpGet]
        public IActionResult Edit(int idMjesta)
        {
            var mjesto = ctx.Mjesto.AsNoTracking()
                            .Where(mjesto => mjesto.IdMjesto == idMjesta).SingleOrDefault();
            if (mjesto == null)
            {
                logger.LogWarning("Ne postoji mjesto");
                return NotFound("Ne postoji odabrano mjesto" + idMjesta);
            }
            else
            {
                return View(mjesto);
            }
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Update(int idMjesta)
        {
            try
            {
                Mjesto mjesto = await
                     ctx.Mjesto.Where(mjesta => mjesta.IdMjesto == idMjesta)
                     .FirstOrDefaultAsync();
                if (mjesto == null)
                {
                    return NotFound("Pogrešan id " + idMjesta);
                }

                if (await TryUpdateModelAsync<Mjesto>(mjesto, "",
                    vrsta => vrsta.IdMjesto, vrsta => vrsta.Naziv, vrsta => vrsta.Postanskibroj))
                {

                    bool nazivExists = await ctx.Mjesto
                      .AnyAsync(m => m.Naziv == mjesto.Naziv && m.IdMjesto != mjesto.IdMjesto);

                    if (nazivExists)
                    {
                        ModelState.AddModelError("Naziv", "Naziv već postoji u bazi podataka.");
                        TempData[Constants.Message] = $"Mjesto s nazivom {mjesto.Naziv} već postoji u bazi podataka.!";
                        TempData[Constants.ErrorOccurred] = true;
                        return View(mjesto);
                    }
                    bool exists = await ctx.Mjesto
                        .AnyAsync(m => m.Postanskibroj == mjesto.Postanskibroj && m.IdMjesto != mjesto.IdMjesto);

                    if (exists)
                    {
                        ModelState.AddModelError("Postanskibroj", "Poštanski broj već postoji u bazi podataka.");
                        return View(mjesto);
                    }

                  
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[Constants.Message] = $"Ažurirano mjesto naziva {mjesto.Naziv}!";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index));

                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(mjesto);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podaci se ne mogu povezati");
                    return View(mjesto);
                }

            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Edit), idMjesta);
            }
        }

    }
}
