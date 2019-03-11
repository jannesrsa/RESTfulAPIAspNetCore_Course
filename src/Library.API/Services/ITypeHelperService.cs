namespace Library.API.Services
{
    public interface ITypeHelperService
    {
        bool TypeHasProperties<TSource>(string fields);
    }
}