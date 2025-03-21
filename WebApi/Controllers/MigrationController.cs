// HungNgo96

using Application.Interfaces.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers
{
    public class MigrationController(IWriteApplicationDbContext applicationDbContext) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> MigrationAsync(CancellationToken cancellationToken)
        {
            await applicationDbContext.Database.MigrateAsync(cancellationToken);
            return Ok();
        }
    }
}
