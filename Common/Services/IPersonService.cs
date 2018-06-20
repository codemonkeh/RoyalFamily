using System.Threading.Tasks;

namespace RoyalFamily.Common.Services
{
    public interface IPersonService
    {
        Task<string> GetRelationship(string fromName, string toName);
    }
}