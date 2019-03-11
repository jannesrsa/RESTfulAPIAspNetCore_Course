using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private const int maxAuthorPageSize = 20;
        private readonly ILibraryRepository _libraryRepository;
        private readonly IUrlHelper _urlHelper;
        private readonly IPropertyMappingService _propertyMappingService;

        public AuthorsController(ILibraryRepository libraryRepository, IUrlHelper urlHelper, IPropertyMappingService propertyMappingService)
        {
            _libraryRepository = libraryRepository;
            _urlHelper = urlHelper;
            _propertyMappingService = propertyMappingService;
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

        [HttpGet(Name = "GetAuthors")]
        public IActionResult GetAuthors(AuthorsResourceParameters authorsResourceParameters)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(authorsResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            var authorsFromRep = _libraryRepository.GetAuthors(authorsResourceParameters);

            var previousPageLink = authorsFromRep.HasPrevious ? CreateAuthorsResrouceUri(authorsResourceParameters, ResourceUriType.PreviousPage) : null;
            var nextPageLink = authorsFromRep.HasNext ? CreateAuthorsResrouceUri(authorsResourceParameters, ResourceUriType.NextPage) : null;

            var paginationMetadata = new
            {
                totalCount = authorsFromRep.TotalCount,
                pageSize = authorsFromRep.PageSize,
                currentPage = authorsFromRep.CurrentPage,
                totalPages = authorsFromRep.TotalPages,
                previousPageLink,
                nextPageLink
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRep);

            return Ok(authors);
        }

        private string CreateAuthorsResrouceUri(
            AuthorsResourceParameters authorsResourceParameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            orderBy = authorsResourceParameters.OrderBy,
                            searchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber = authorsResourceParameters.PageNumber - 1,
                            pageSize = authorsResourceParameters.PageSize,
                        });

                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetAuthors",
                         new
                         {
                             orderBy = authorsResourceParameters.OrderBy,
                             searchQuery = authorsResourceParameters.SearchQuery,
                             genre = authorsResourceParameters.Genre,
                             pageNumber = authorsResourceParameters.PageNumber + 1,
                             pageSize = authorsResourceParameters.PageSize
                         });

                default:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            orderBy = authorsResourceParameters.OrderBy,
                            searchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber = authorsResourceParameters.PageNumber,
                            pageSize = authorsResourceParameters.PageSize
                        });
            }
        }
    }
}