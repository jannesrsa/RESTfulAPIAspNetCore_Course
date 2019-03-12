using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Library.API.Controllers
{
    [Route("api")]
    public class RootController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly ILogger<RootController> _logger;
        private readonly IUrlHelper _urlHelper;

        public RootController(ILibraryRepository libraryRepository, ILogger<RootController> logger, IUrlHelper urlHelper)
        {
            _libraryRepository = libraryRepository;
            _logger = logger;
            _urlHelper = urlHelper;
        }

        [HttpGet(Name = "GetRoot")]
        public IActionResult GetRoot([FromHeader(Name = "Accept")] string mediaType)
        {
            if (mediaType == "application/vnd.marvin.hateoas+json")
            {
                var links = new List<LinkDto>();

                links.Add(
                  new LinkDto
                  {
                      Href = _urlHelper.Link("GetRoot", new { }),
                      Rel = "self",
                      Method = "GET"
                  });

                links.Add(
                     new LinkDto
                     {
                         Href = _urlHelper.Link("GetAuthors", new { }),
                         Rel = "authors",
                         Method = "GET"
                     });

                links.Add(
                   new LinkDto
                   {
                       Href = _urlHelper.Link("CreateAuthor", new { }),
                       Rel = "create_author",
                       Method = "POST"
                   });

                return Ok(links);
            }

            return NoContent();
        }
    }
}