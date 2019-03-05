using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;

        public AuthorsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet("{id}")]
        public IActionResult GetAuthor(Guid id)
        {
            var authorFromRep = _libraryRepository.GetAuthor(id);
            if (authorFromRep == null)
            {
                return NotFound();
            }

            var author = Mapper.Map<AuthorDto>(authorFromRep);

            return Ok(author);
        }

        [HttpGet]
        public IActionResult GetAuthors()
        {
            var authorsFromRep = _libraryRepository.GetAuthors();

            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRep);

            return Ok(authors);
        }
    }
}