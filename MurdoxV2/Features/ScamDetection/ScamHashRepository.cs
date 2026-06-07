using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Features.ScamDetection
{
    public sealed class ScamHashRepository : IScamHashRepository
    {
        private readonly AppDbContext _db;

        public ScamHashRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<ScamImageRecord>> GetAllAsync()
        {
            return await _db.ScamImages
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ScamImageRecord?> GetByIdAsync(Guid id)
        {
            return await _db.ScamImages
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddAsync(ScamImageRecord record)
        {
            var exists = await _db.ScamImages.AnyAsync(x =>
                            x.AHash == record.AHash &&
                            x.DHash == record.DHash &&
                            x.PHash == record.PHash);
            if (!exists)
            {
                _db.ScamImages.Add(record);
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _db.ScamImages.FindAsync(id);
            if (entity is null)
                return;

            _db.ScamImages.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}
