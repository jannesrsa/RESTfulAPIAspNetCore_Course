using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;

        public BooksController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet("{id}")]
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

            return Ok(book);
        }

        [HttpGet]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var booksFromRep = _libraryRepository.GetBooksForAuthor(authorId);

            var books = Mapper.Map<IEnumerable<BookDto>>(booksFromRep);

            return Ok(books);
        }
    }
}