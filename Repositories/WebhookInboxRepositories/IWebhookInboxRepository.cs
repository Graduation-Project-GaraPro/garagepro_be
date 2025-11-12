using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.PayOsModels;

namespace Repositories.WebhookInboxRepositories
{
    public interface IWebhookInboxRepository
    {
        Task<WebhookInbox> GetByConditionAsync(Expression<Func<WebhookInbox, bool>> predicate, CancellationToken ct = default);
        Task<List<WebhookInbox>> GetListByConditionAsync(Expression<Func<WebhookInbox, bool>> predicate, CancellationToken ct = default);
        Task<WebhookInbox> AddAsync(WebhookInbox entity);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
