using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly ITypeHelperService _typeHelperService;
        private readonly IUrlHelper _urlHelper;

        public AuthorsController(
            ILibraryRepository libraryRepository,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            ITypeHelperService typeHelperService)
        {
            _libraryRepository = libraryRepository;
            _urlHelper = urlHelper;
            _propertyMappingService = propertyMappingService;
            _typeHelperService = typeHelperService;
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

        [HttpPost(Name = "CreateAuthor")]
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

            var links = CreateLinksForAuthor(authorEntity.Id, null);

            var linkedResourceToReturn = authorToReturn.ShapeData(null) as IDictionary<string, object>;
            linkedResourceToReturn.Add("links", links);

            return CreatedAtRoute(
                "GetAuthor",
                new { id = linkedResourceToReturn["Id"] },
                linkedResourceToReturn
            );
        }

        [HttpDelete("{id}", Name = "DeleteAuthor")]
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
        public IActionResult GetAuthor(Guid id, [FromQuery] string fields)
        {
            if (!_typeHelperService.TypeHasProperties<AuthorDto>(fields))
            {
                return BadRequest();
            }

            var authorFromRep = _libraryRepository.GetAuthor(id);
            if (authorFromRep == null)
            {
                return NotFound();
            }

            var author = Mapper.Map<AuthorDto>(authorFromRep);
            var links = CreateLinksForAuthor(id, fields);

            var linkedResourceToReturn = author.ShapeData(fields) as IDictionary<string, object>;
            linkedResourceToReturn.Add("links", links);

            return Ok(linkedResourceToReturn);
        }

        [HttpGet(Name = "GetAuthors")]
        public IActionResult GetAuthors(AuthorsResourceParameters authorsResourceParameters,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(authorsResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_typeHelperService.TypeHasProperties<AuthorDto>(authorsResourceParameters.Fields))
            {
                return BadRequest();
            }

            var authorsFromRep = _libraryRepository.GetAuthors(authorsResourceParameters);

            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRep);

            if (mediaType == "application/vnd.marvin.hateoas+json")
            {
                var paginationMetadata = new
                {
                    totalCount = authorsFromRep.TotalCount,
                    pageSize = authorsFromRep.PageSize,
                    currentPage = authorsFromRep.CurrentPage,
                    totalPages = authorsFromRep.TotalPages
                };

                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                var links = CreateLinksForAuthors(authorsResourceParameters, authorsFromRep.HasNext, authorsFromRep.HasPrevious);

                var shapedAuthors = authors.ShapeData(authorsResourceParameters.Fields);
                var shapedAuthorsWithLinks = shapedAuthors.Select(i =>
                {
                    var authorAsDict = i as IDictionary<string, object>;
                    var authorLinks = CreateLinksForAuthor((Guid)authorAsDict["Id"], authorsResourceParameters.Fields);

                    authorAsDict.Add("links", authorLinks);

                    return authorAsDict;
                });

                var linkedCollectionResource = new
                {
                    value = shapedAuthorsWithLinks,
                    links
                };

                return Ok(linkedCollectionResource);
            }
            else
            {
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
                return Ok(authors.ShapeData(authorsResourceParameters.Fields));
            }
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
                            fields = authorsResourceParameters.Fields,
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
                             fields = authorsResourceParameters.Fields,
                             orderBy = authorsResourceParameters.OrderBy,
                             searchQuery = authorsResourceParameters.SearchQuery,
                             genre = authorsResourceParameters.Genre,
                             pageNumber = authorsResourceParameters.PageNumber + 1,
                             pageSize = authorsResourceParameters.PageSize
                         });

                case ResourceUriType.Current:
                default:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            fields = authorsResourceParameters.Fields,
                            orderBy = authorsResourceParameters.OrderBy,
                            searchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber = authorsResourceParameters.PageNumber,
                            pageSize = authorsResourceParameters.PageSize
                        });
            }
        }

        private IEnumerable<LinkDto> CreateLinksForAuthor(Guid id, string fields)
        {
            var links = new List<LinkDto>();
            if (string.IsNullOrEmpty(fields))
            {
                links.Add(new LinkDto
                {
                    Href = _urlHelper.Link("GetAuthor", new { id }),
                    Rel = "self",
                    Method = "GET"
                });
            }
            else
            {
                links.Add(new LinkDto
                {
                    Href = _urlHelper.Link("GetAuthor", new { id, fields }),
                    Rel = "self",
                    Method = "GET"
                });
            }

            links.Add(new LinkDto
            {
                Href = _urlHelper.Link("DeleteAuthor", new { id }),
                Rel = "delete_author",
                Method = "DELETE"
            });

            links.Add(new LinkDto
            {
                Href = _urlHelper.Link("CreateBookForAuthor", new { authorId = id }),
                Rel = "create_book_for_authoer",
                Method = "POST"
            });

            links.Add(new LinkDto
            {
                Href = _urlHelper.Link("GetBooksForAuthor", new { authorId = id }),
                Rel = "books",
                Method = "GET"
            });

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForAuthors(AuthorsResourceParameters authorsResourceParameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>
            {
                new LinkDto
                {
                    Href = CreateAuthorsResrouceUri(authorsResourceParameters, ResourceUriType.Current),
                    Rel = "self",
                    Method = "GET"
                }
            };

            if (hasNext)
            {
                links.Add(new LinkDto
                {
                    Href = CreateAuthorsResrouceUri(authorsResourceParameters, ResourceUriType.NextPage),
                    Rel = "nextPage",
                    Method = "GET"
                });
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto
                {
                    Href = CreateAuthorsResrouceUri(authorsResourceParameters, ResourceUriType.PreviousPage),
                    Rel = "previousPage",
                    Method = "GET"
                });
            }

            return links;
        }
    }
}