using Microsoft.AspNetCore.Mvc;
using TeamDesk.DTOs;
using TeamDesk.Services.Interfaces;

namespace TeamDesk.Controllers
{

    [ApiController]
    [Route("api/faq")]
    public class FaqController : ControllerBase
    {
        private readonly IFaqService _faqService;

        public FaqController(IFaqService faqService)
        {
            _faqService = faqService;
        }

        [HttpPost("search")]
        public IActionResult Search([FromBody] QueryModel model)
        {
            var results = _faqService.Search(model.Query);
            return Ok(new { answers = results });
        }

       
    }
}