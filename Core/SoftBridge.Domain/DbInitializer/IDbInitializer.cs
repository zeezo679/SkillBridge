
namespace SoftBridge.Domain.DbInitializer
{
    public interface IDbInitializer
    {
        Task DataSeedAsync();
    }
}
