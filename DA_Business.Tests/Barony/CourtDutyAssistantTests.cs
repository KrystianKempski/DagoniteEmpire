using DA_Common.Barony;
using DA_Models.BaronyModels;
using DagoniteEmpire.Pages.Barony;

namespace DA_Business.Tests.Barony;

public class CourtDutyAssistantTests
{
    [Fact]
    public void AssistantInfluence_FloorsEachSignificantDomainSkillByFive()
    {
        var domain = new PpbVector();
        domain[Ppb.Loyalty] = 11m;
        domain[Ppb.Stability] = 5m;
        domain[Ppb.Culture] = 4m;
        domain[Ppb.Law] = 20m;

        var bonus = BaronyCalc.AssistantInfluenceFromDomain(
            domain,
            AdvisorSignificantSkills.DefaultForOffice(OfficeType.Chancellor));

        Assert.Equal(2m, bonus[Ppb.Loyalty]);
        Assert.Equal(1m, bonus[Ppb.Stability]);
        Assert.Equal(0m, bonus[Ppb.Culture]);
        Assert.Equal(0m, bonus[Ppb.Law]);
    }

    [Fact]
    public void BuildAdvisorInfluenceRows_AddsNamedAssistantSourceWithSalaryCost()
    {
        var office = new AdvisorDTO
        {
            Id = 1,
            OfficeType = OfficeType.Steward,
            AvailableAdvisorId = 9,
            Skills = new PpbVector(),
        };
        var helper = new AvailableAdvisorDTO
        {
            Id = 4,
            Name = "Marta Scribe",
            Skills = new PpbVector(),
            Duties =
            [
                new CourtDutyDTO
                {
                    Id = 7,
                    DutyKind = CourtDutyKind.Assistant,
                    DutyOfficeType = OfficeType.Steward,
                    SalaryGold = 3m,
                },
            ],
        };
        helper.Skills[Ppb.Food] = 10m;
        helper.Skills[Ppb.Production] = 6m;
        helper.Skills[Ppb.Economy] = 3m;

        var rows = BaronyCalc.BuildAdvisorInfluenceRows(office, null, [helper]);
        var assistant = Assert.Single(rows, r => r.SystemKind == AdvisorInfluenceSystemKind.Assistant);

        Assert.Equal("Scribe Marta Scribe", assistant.Source);
        Assert.Equal(3m, assistant.Cost);
        Assert.Equal(2m, assistant.Values[Ppb.Food]);
        Assert.Equal(1m, assistant.Values[Ppb.Production]);
        Assert.Equal(0m, assistant.Values[Ppb.Economy]);
        Assert.True(assistant.IsSystem);
    }

    [Fact]
    public void OccupiedAssistantOfficeTypes_OnePerOffice()
    {
        var courtiers = new[]
        {
            new AvailableAdvisorDTO
            {
                Duties =
                [
                    new CourtDutyDTO { Id = 1, DutyKind = CourtDutyKind.Assistant, DutyOfficeType = OfficeType.Chancellor },
                ],
            },
        };

        var taken = BaronyCalc.OccupiedAssistantOfficeTypes(courtiers);
        Assert.Contains(OfficeType.Chancellor, taken);
        Assert.DoesNotContain(OfficeType.Steward, taken);

        var exceptSelf = BaronyCalc.OccupiedAssistantOfficeTypes(courtiers, exceptDutyId: 1);
        Assert.Empty(exceptSelf);
    }
}
