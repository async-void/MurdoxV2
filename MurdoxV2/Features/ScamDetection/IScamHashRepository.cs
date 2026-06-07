using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Features.ScamDetection
{
    public interface IScamHashRepository
    {
        /// <summary>
        /// Returns all known scam image records from the database.
        /// </summary>
        Task<IReadOnlyList<ScamImageRecord>> GetAllAsync();

        /// <summary>
        /// Inserts a new scam image record (aHash, dHash, pHash, metadata).
        /// </summary>
        Task AddAsync(ScamImageRecord record);

        /// <summary>
        /// Returns a single scam image record by ID, or null if not found.
        /// </summary>
        Task<ScamImageRecord?> GetByIdAsync(Guid id);

        /// <summary>
        /// Deletes a scam image record by ID.
        /// </summary>
        Task DeleteAsync(Guid id);
    }
}
