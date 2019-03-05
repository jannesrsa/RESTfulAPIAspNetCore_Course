using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        readonly ILibraryRepository _libraryRepository;

        public AuthorsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet]
        public IActionResult GetAuthors()
        {
            var authorsFromRep = _libraryRepository.GetAuthors();

            var authors = new List<AuthorDto>();
            foreach (var author in authorsFromRep)
            {
                authors.Add(new AuthorDto()
                {
                    Id = author.Id,
                    Name = $"{author.FirstName} {author.LastName}",
                    Genre = author.Genre,
                    Age = author.DateOfBirth.GetCurrentAge()
                });
            }

            return new JsonResult(authors);
        }
    }
}
