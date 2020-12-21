using Microsoft.AspNetCore.Mvc;

namespace SpacerTransformationsAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]/[action]")]
    public class VariablesController: Controller
    {
        [HttpPost]
        public ActionResult Replace()
        {
            return Ok();
        }
    }
}