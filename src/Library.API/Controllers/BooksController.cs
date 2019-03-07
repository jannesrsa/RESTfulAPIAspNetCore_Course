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

        [HttpPost]
        public IActionResult CreateBook(Guid authorId, [FromBody] BookForCreationDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = Mapper.Map<Entities.Book>(book);

            _libraryRepository.AddBookForAuthor(authorId, bookEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating a book failed on save.");
            }

            var bookToReturn = Mapper.Map<BookDto>(bookEntity);

            return CreatedAtRoute(
                "GetBookForAuthor",
                new { id = bookToReturn.Id },
                bookToReturn
            );
        }

        [HttpDelete("{id}")]
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

        [HttpPut("{id}")]
        public IActionResult UpdateBook(Guid authorId, Guid id, [FromBody] BookForCreationDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookEntity == null)
            {
                bookEntity = Mapper.Map<Entities.Book>(book);
                bookEntity.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookEntity);
                if (!_libraryRepository.Save())
                {
                    throw new Exception("Upserting a book failed on save.");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookEntity);

                return CreatedAtRoute(
                    "GetBookForAuthor", 
                    new { id = bookToReturn.Id }, 
                    bookToReturn);
            }
            else
            {
                Mapper.Map(book, bookEntity);
                _libraryRepository.UpdateBookForAuthor(bookEntity);

                if (!_libraryRepository.Save())
                {
                    throw new Exception("Update a book failed on save.");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookEntity);

                return NoContent();
            }
        }
    }
}