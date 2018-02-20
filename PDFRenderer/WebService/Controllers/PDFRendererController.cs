using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PDFRendererActorSupervisorService.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using PDFRendererModels;
using PDFRendererHelpers;

namespace WebService.Controllers
{
    [Produces("application/json")]
    [Route("api/PDFRenderer")]
    public class PDFRendererController : Controller
    {
        private const int ActorsLimit = 20;
        [HttpGet("start")]
        public async Task<IActionResult> StartPDFRenderer()
        {
            var pagesToRender = new List<string>()
            {
                "https://en.wikipedia.org/wiki/Angular_(application_platform)",
                "https://en.wikipedia.org/wiki/MIT_License",
                "https://en.wikipedia.org/wiki/X_Window_System",
                "https://en.wikipedia.org/wiki/Free_and_open-source_software",
                "https://en.wikipedia.org/wiki/GIMP",
                "https://en.wikipedia.org/wiki/Linux",
                "https://en.wikipedia.org/wiki/Unicore"
            };

            IPDFRendererActorSupervisorService actorProcessorService = ActorProxy.Create<IPDFRendererActorSupervisorService>(
                ActorId.CreateRandom(),
                new Uri("fabric:/LongConcurrentTasks/LongTaskActorSupervisorService"));
            try
            {
                await actorProcessorService.StartProcessingAsync(new Message<string[]>
                {
                    Id = Guid.NewGuid(),
                    Payload = pagesToRender.ToArray()
                });
            }
            catch (Exception exception)
            {
                return new ObjectResult(exception);
            }
            return Accepted();
        }

        [HttpGet("start/normalParallel")]
        public IActionResult StartLongTaskWithoutActor()
        {
            var pagesToRender = new List<string>()
            {
                "https://en.wikipedia.org/wiki/Angular_(application_platform)",
                "https://en.wikipedia.org/wiki/MIT_License",
                "https://en.wikipedia.org/wiki/X_Window_System",
                "https://en.wikipedia.org/wiki/Free_and_open-source_software",
                "https://en.wikipedia.org/wiki/GIMP",
                "https://en.wikipedia.org/wiki/Linux",
                "https://en.wikipedia.org/wiki/Unicore"
            };

            Parallel.ForEach(pagesToRender, itemToAdd => {
                var stream = Helpers.LoadHTMLStreamFromUrl(itemToAdd);
                var pdf = Helpers.CreatePDFFromStream(stream, string.Empty);
                var result = Helpers.SavePDFFile(pdf, string.Format(@"C:\Temp\PDFRenderer\{0}", itemToAdd.ToString()));
            });

            return Ok();
        }
    }
}