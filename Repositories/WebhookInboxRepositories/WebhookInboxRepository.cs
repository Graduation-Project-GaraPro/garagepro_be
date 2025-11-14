using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.PayOsModels;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Repositories.WebhookInboxRepositories
{
    public class WebhookInboxRepository : IWebhookInboxRepository
    {
        private readonly MyAppDbContext _context;

        public WebhookInboxRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<WebhookInbox> GetByConditionAsync(Expression<Func<WebhookInbox, bool>> predicate, CancellationToken ct = default)
        {
            return await _context.WebhookInboxes
                .FirstOrDefaultAsync(predicate, ct);
        }

        public async Task<List<WebhookInbox>> GetListByConditionAsync(Expression<Func<WebhookInbox, bool>> predicate, CancellationToken ct = default)
        {
            return await _context.WebhookInboxes
                .Where(predicate)
                .ToListAsync(ct);
        }

        public async Task<WebhookInbox> AddAsync(WebhookInbox entity)
        {
            _context.WebhookInboxes.Add(entity);
           
            return entity;
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await _context.SaveChangesAsync(ct);
        }
    }
}
