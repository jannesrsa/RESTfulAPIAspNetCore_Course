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

        [HttpGet("{id}", Name = "GetAuthor")]
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

        [HttpPost]
        public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }

            var authorEntity = Mapper.Map<Entities.Author>(author);

            _libraryRepository.AddAuthor(authorEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating an author failed on save.");
            }

            var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);

            return CreatedAtRoute(
                "GetAuthor",
                new { id = authorToReturn.Id },
                authorToReturn
            );
        }

        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreation(Guid id)
        {
            if (_libraryRepository.AuthorExists(id))
            {
                return Conflict();
            }

            return NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            var authorFromRep = _libraryRepository.GetAuthor(id);
            if (authorFromRep == null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteAuthor(authorFromRep);
            if (!_libraryRepository.Save())
            {
                throw new Exception("Deleting a author failed on save.");
            }

            return NoContent();
        }
    }
}