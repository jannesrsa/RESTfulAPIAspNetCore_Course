using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authorcollections")]
    public class AuthorCollectionsController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;

        // Inject repository with constructor injection
        public AuthorCollectionsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpPost]
        public IActionResult CreateAuthorCollection([FromBody] IEnumerable<AuthorForCreationDto> authorCollection)
        {
            if (authorCollection == null)
            {
                return BadRequest();
            }

            var authorsEntity = Mapper.Map<IEnumerable<Entities.Author>>(authorCollection);

            foreach (var authorEntity in authorsEntity)
            {
                _libraryRepository.AddAuthor(authorEntity);
            }

            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating an author collection failed on save.");
            }

            return Ok();

            //var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);

            //return CreatedAtRoute(
            //    "GetAuthor",
            //    new { id = authorToReturn.Id },
            //    authorToReturn
            //);
        }
    }
}