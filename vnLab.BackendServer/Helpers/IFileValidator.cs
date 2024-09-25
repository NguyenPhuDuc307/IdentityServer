namespace vnLab.BackendServer.Helpers
{
    public interface IFileValidator
    {
        bool IsValid(IFormFile file);
    }
}