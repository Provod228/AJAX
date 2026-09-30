using WebApplication5.Api.Models;

namespace WebApplication5.Api.Services;

public interface IBookRepository
{
    IEnumerable<Book> GetAll();
    Book? GetById(int id);
    Book Add(Book book);
    bool Delete(int id);
}