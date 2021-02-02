using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SpacerTransformationsAPI.Functions;
using SpacerTransformationsAPI.Models;

namespace SpacerTransformationsAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]/[action]")]
    public class VariablesController: Controller
    {
        [HttpPost]
        public ActionResult Replace([FromBody]ReplaceRequestBody requestBody)
        {
            
            try
            {
                Console.WriteLine(requestBody.Instance);
                var rawSpacerInstance = requestBody.SpacerInstance;
                var lemmas = JsonConvert.DeserializeObject<SpacerInstance>(rawSpacerInstance);
                foreach (var kvp in lemmas.Lemmas)
                {
                    foreach (var io in requestBody.Params)
                    {
                        if (lemmas.Lemmas[kvp.Key].Readable == null) continue;
                        lemmas.Lemmas[kvp.Key].Raw = lemmas.Lemmas[kvp.Key].Raw.Replace(io.Source, io.Target);
                        lemmas.Lemmas[kvp.Key].Readable = lemmas.Lemmas[kvp.Key].Readable.Replace(io.Source, io.Target);
                    }
                }
                Console.WriteLine("Transformation complete");
                return Ok(JsonConvert.SerializeObject(lemmas.Lemmas));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.TargetSite);
                Console.WriteLine("Error: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
