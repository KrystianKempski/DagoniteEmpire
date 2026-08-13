using DA_Common.Barony;
using DA_DataAccess.BaronyData;
using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DA_Business.Repository.MarchMapRepos
{
    public static class MarchMapSeeder
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public static async Task EnsureInitializedAsync(ApplicationDbContext ctx)
        {
            var exists = await ctx.MarchMapStates.AnyAsync(x => x.Id == MarchMapState.GlobalId);
            if (exists)
                return;

            var seed = EasternMarchMapDefaults.CreateSeedDocument();
            ctx.MarchMapStates.Add(new MarchMapState
            {
                Id = MarchMapState.GlobalId,
                PayloadJson = JsonSerializer.Serialize(seed, JsonOptions),
            });
            await ctx.SaveChangesAsync();
        }
    }
}
