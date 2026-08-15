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
            var row = await ctx.MarchMapStates.FirstOrDefaultAsync(x => x.Id == MarchMapState.GlobalId);
            if (row is not null && ReadSeedVersion(row.PayloadJson) >= EasternMarchMapDefaults.CurrentSeedVersion)
                return;

            var seed = EasternMarchMapDefaults.CreateSeedDocument();
            var payload = JsonSerializer.Serialize(seed, JsonOptions);
            if (row is null)
            {
                ctx.MarchMapStates.Add(new MarchMapState
                {
                    Id = MarchMapState.GlobalId,
                    PayloadJson = payload,
                });
            }
            else
            {
                row.PayloadJson = payload;
            }

            await ctx.SaveChangesAsync();
        }

        private static int ReadSeedVersion(string? payloadJson)
        {
            if (string.IsNullOrWhiteSpace(payloadJson))
                return 0;
            try
            {
                return JsonSerializer.Deserialize<MarchMapDocument>(payloadJson, JsonOptions)?.SeedVersion ?? 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}
