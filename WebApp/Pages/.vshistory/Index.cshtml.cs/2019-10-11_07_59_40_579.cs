using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using WebApp.Data;
using WebApp.Models;

namespace WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            Context = context;
        }

        public ApplicationDbContext Context { get; }

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            await Context.Set<Entity01>().AddAsync(new Entity01 { InsertDate = DateTimeOffset.UtcNow, Entity01Id = 1, IsDeleted = true });
            await Context.SaveChangesAsync(cancellationToken);
            return Page();
        }
    }
}
