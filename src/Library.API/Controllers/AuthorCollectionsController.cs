using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Library.API.Helpers;
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

            var authorCollectionToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorsEntity);

            return CreatedAtRoute(
                "GetAuthorCollection",
                new { ids = string.Join(",", authorCollectionToReturn.Select(i => i.Id)) },
                authorCollectionToReturn
            );
        }

        // (key1,key2, ...)
        [HttpGet("({ids})", Name = "GetAuthorCollection")]
        public IActionResult GetAuthorCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var authorsEntities = _libraryRepository.GetAuthors(ids);
            if (authorsEntities.Count() != ids.Count())
            {
                return NotFound();
            }

            var authorsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorsEntities);

            return Ok(authorsToReturn);
        }
    }
}