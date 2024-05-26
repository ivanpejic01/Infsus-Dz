
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WebApp.Controllers;
using WebApp.Models;
using WebApp.ViewModels;

namespace TestProject1
{
    public class Tests { 

        [SetUp]
        public void Setup()
        {

    
        }
        [Test]
        public async Task PutovanjeControllerIndexViewModelTest()
        {

            var options = new DbContextOptionsBuilder<infsusContext>().UseInMemoryDatabase(databaseName: "infsus").Options;

            var context = new infsusContext(options);
            var mockLogger = new Mock<ILogger<PutovanjeController>>();
            PutovanjeController controller = new PutovanjeController(context, mockLogger.Object);

            // Dodajte Putovanje entitet i povežite ga sa prethodno kreiranim entitetima
            context.Putovanje.Add(new Putovanje
            {
                IdPutovanja = 3,
                IdMjesto = 1,
                IdSmjestaj = 1,
                IdVrstaPutovanja = 1,
                IdVozilo = 1,
                Opis = "Test Putovanje",
                ImeprezimeVoditelj = "Voditelj 1",
                Cijena = 1000,
                DatumPolaska = DateTime.Now,
                DatumPovratka = DateTime.Now.AddDays(7),
                Rate = "da"
            });

            context.SaveChanges();

            //Act
            var result = await controller.Index(null, null, 1, 4);

            //Assert
    
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as PutovanjaViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.TotalCount);

        }

        [Test]
        public async Task PutovanjeControllerDelete()
        {
            var options = new DbContextOptionsBuilder<infsusContext>()
.UseInMemoryDatabase(databaseName: "infsus")
.Options;

            var context = new infsusContext(options);
            var mockLogger = new Mock<ILogger<PutovanjeController>>();
            PutovanjeController controller = new PutovanjeController(context, mockLogger.Object);

            // Dodajte Putovanje entitet i povežite ga sa prethodno kreiranim entitetima
            context.Putovanje.Add(new Putovanje
            {
                IdPutovanja = 2,
                IdMjesto = 1,
                IdSmjestaj = 1,
                IdVrstaPutovanja = 1,
                IdVozilo = 1,
                Opis = "Test Putovanje",
                ImeprezimeVoditelj = "Voditelj 1",
                Cijena = 1000,
                DatumPolaska = DateTime.Now,
                DatumPovratka = DateTime.Now.AddDays(7),
                Rate = "da"
            });

            context.SaveChanges();

            //Act
            var result = await controller.Delete(2) as RedirectToActionResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);

            var deletedPutovanje = await context.Putovanje.FindAsync(2);
            Assert.IsNull(deletedPutovanje);


        }

        [Test]
        public async Task MjestoControllerIndexTest()
        {
            var options = new DbContextOptionsBuilder<infsusContext>().UseInMemoryDatabase(databaseName: "infsus").Options;

            var context = new infsusContext(options);
            var mockLogger = new Mock<ILogger<MjestoController>>();
            MjestoController controller = new MjestoController(context, mockLogger.Object);

            context.Mjesto.Add(new Mjesto
            {
                   IdMjesto = 1,
                   Naziv = "MjestoTest",
                   Postanskibroj = 35000
            });

            context.SaveChanges();

            //Act
            var result = await controller.Index();
            ViewResult viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("Index", viewResult.ViewName);

            var model = viewResult.Model as MjestaViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.mjesta.Count());
        }

        [Test]
        public async Task SmjestajControllerSearchTest()
        {
            var options = new DbContextOptionsBuilder<infsusContext>().UseInMemoryDatabase(databaseName: "infsus").Options;

            var context = new infsusContext(options);
            var mockLogger = new Mock<ILogger<SmjestajController>>();
            SmjestajController controller = new SmjestajController(context, mockLogger.Object);

            var obrok = new Obrok { IdObroka = 1, VrstaObroka = "Doručak" };
            var soba = new Soba { IdSobe = 1, VrstaSobe = "Jednokrevetna" };
            var vrstaSmjestaja = new VrstaSmjestaja { IdVrstaSmjestaja = 1, VrstaSmjestaja1 = "Hotel" };

            context.Obrok.Add(obrok);
            context.Soba.Add(soba);
            context.VrstaSmjestaja.Add(vrstaSmjestaja);


            context.Smjestaj.Add(new Smjestaj
            {
                IdSmjestaj = 1,
                IdObrok = 1,
                IdSoba = 1,
                IdVrstaSmjestaja = 1,
                Naziv = "Test",
                IdObrokNavigation = obrok,
                IdSobaNavigation = soba,
                IdVrstaSmjestajaNavigation = vrstaSmjestaja
            });

            context.SaveChanges();

            var goodResult = await controller.Search("Test");
            ViewResult viewResultGood = goodResult as ViewResult;
            Assert.IsNotNull(viewResultGood);
            var modelGood = viewResultGood.Model as SmjestajiViewModel;
            Assert.IsNotNull(modelGood);
            Assert.AreEqual(1, modelGood.smjestaji.Count());
        }


        [TearDown]
        public void TearDown()
        {
            //context.Dispose();
            //controller.Dispose();
        }

    }
}