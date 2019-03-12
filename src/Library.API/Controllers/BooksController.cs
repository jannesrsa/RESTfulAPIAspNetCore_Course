using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly ILogger<BooksController> _logger;
        private readonly IUrlHelper _urlHelper;

        public BooksController(ILibraryRepository libraryRepository, ILogger<BooksController> logger, IUrlHelper urlHelper)
        {
            _libraryRepository = libraryRepository;
            _logger = logger;
            _urlHelper = urlHelper;
        }

        [HttpPost(Name = "CreateBookForAuthor")]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDto bookForCreationDto)
        {
            if (bookForCreationDto == null)
            {
                return BadRequest();
            }

            if (bookForCreationDto.Description == bookForCreationDto.Title)
            {
                ModelState.AddModelError(nameof(BookForCreationDto), "The provided description should be different from the title.");
            }

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = Mapper.Map<Entities.Book>(bookForCreationDto);

            _libraryRepository.AddBookForAuthor(authorId, bookEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating a book failed on save.");
            }

            var bookDtoToReturn = Mapper.Map<BookDto>(bookEntity);

            return CreatedAtRoute(
                "GetBookForAuthor",
                new { id = bookDtoToReturn.Id },
                CreateLinksForBook(bookDtoToReturn)
            );
        }

        [HttpDelete("{id}", Name = "DeleteBookForAuthor")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            var bookFromRep = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookFromRep == null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteBook(bookFromRep);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Deleting a book failed on save.");
            }

            _logger.LogInformation("Book deleted");
            return NoContent();
        }

        [HttpGet("{id}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookFromRep = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookFromRep == null)
            {
                return NotFound();
            }

            var book = Mapper.Map<BookDto>(bookFromRep);

            return Ok(CreateLinksForBook(book));
        }

        [HttpGet(Name = "GetBooksForAuthor")]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var booksFromRep = _libraryRepository.GetBooksForAuthor(authorId);

            var books = Mapper.Map<IEnumerable<BookDto>>(booksFromRep);

            return Ok(CreateLinksForBook(new LinkedCollectionResrouceWrapperDto<BookDto>()
            {
                Value = books.Select(book => CreateLinksForBook(book))
            }));
        }

        [HttpPatch("{id}", Name = "PartiallyUpdateBookForAuthor")]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId, Guid id, [FromBody] JsonPatchDocument<BookForUpdateDto> jsonPatchDocument)
        {
            if (jsonPatchDocument == null)
            {
                return BadRequest();
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
            {
                var bookForUpdateDto = new BookForUpdateDto();
                jsonPatchDocument.ApplyTo(bookForUpdateDto, ModelState);

                if (bookForUpdateDto.Description == bookForUpdateDto.Title)
                {
                    ModelState.AddModelError(nameof(BookForUpdateDto), "The provided description should be different from the title.");
                }

                TryValidateModel(bookForUpdateDto);

                if (!ModelState.IsValid)
                {
                    return UnprocessableEntity(ModelState);
                }

                bookForAuthorFromRepo = Mapper.Map<Entities.Book>(bookForUpdateDto);
                bookForAuthorFromRepo.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookForAuthorFromRepo);
                if (!_libraryRepository.Save())
                {
                    throw new Exception("Update a book failed on save.");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookForAuthorFromRepo);

                return CreatedAtRoute(
                    "GetBookForAuthor",
                    new { authorId, id },
                    bookToReturn);
            }

            var bookToPatch = Mapper.Map<BookForUpdateDto>(bookForAuthorFromRepo);

            jsonPatchDocument.ApplyTo(bookToPatch, ModelState);

            if (bookToPatch.Description == bookToPatch.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto), "The provided description should be different from the title.");
            }

            TryValidateModel(bookToPatch);

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            Mapper.Map(bookToPatch, bookForAuthorFromRepo);

            _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Update a book failed on save.");
            }

            return NoContent();
        }

        [HttpPut("{id}", Name = "UpdateBook")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookForUpdateDto bookForUpdateDto)
        {
            if (bookForUpdateDto == null)
            {
                return BadRequest();
            }

            if (bookForUpdateDto.Description == bookForUpdateDto.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto), "The provided description should be different from the title.");
            }

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookEntity == null)
            {
                bookEntity = Mapper.Map<Entities.Book>(bookForUpdateDto);
                bookEntity.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookEntity);
                if (!_libraryRepository.Save())
                {
                    throw new Exception("Upserting a book failed on save.");
                }

                var bookDtoToReturn = Mapper.Map<BookDto>(bookEntity);

                return CreatedAtRoute(
                    "GetBookForAuthor",
                    new { id = bookDtoToReturn.Id },
                    CreateLinksForBook(bookDtoToReturn));
            }
            else
            {
                Mapper.Map(bookForUpdateDto, bookEntity);
                _libraryRepository.UpdateBookForAuthor(bookEntity);

                if (!_libraryRepository.Save())
                {
                    throw new Exception("Update a book failed on save.");
                }

                return NoContent();
            }
        }

        private BookDto CreateLinksForBook(BookDto book)
        {
            book.Links.Add(new LinkDto
            {
                Href = _urlHelper.Link("GetBookForAuthor", new { book.Id }),
                Rel = "self",
                Method = "GET"
            });

            book.Links.Add(new LinkDto
            {
                Href = _urlHelper.Link("DeleteBookForAuthor", new { book.Id }),
                Rel = "delete_book",
                Method = "DELETE"
            });

            book.Links.Add(new LinkDto
            {
                Href = _urlHelper.Link("UpdateBook", new { book.Id }),
                Rel = "update_book",
                Method = "PUT"
            });

            book.Links.Add(new LinkDto
            {
                Href = _urlHelper.Link("PartiallyUpdateBookForAuthor", new { book.Id }),
                Rel = "part_update_book",
                Method = "PATCH"
            });

            return book;
        }

        private LinkedCollectionResrouceWrapperDto<BookDto> CreateLinksForBook(LinkedCollectionResrouceWrapperDto<BookDto> booksWrapper)
        {
            booksWrapper.Links.Add(new LinkDto
            {
                Href = _urlHelper.Link("GetBooksForAuthor", new { }),
                Rel = "self",
                Method = "GET"
            });

            return booksWrapper;
        }
    }
}