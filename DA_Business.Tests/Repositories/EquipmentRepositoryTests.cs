using DA_Business.Repository.CharacterReps;
using DA_Business.Tests.Fixtures;
using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;

namespace DA_Business.Tests.Repositories;

public class EquipmentRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly EquipmentRepository _repository;

    public EquipmentRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new EquipmentRepository(_fixture.DbContextFactory, _fixture.Mapper);
    }

    [Fact]
    public async Task Create_ShouldAddEquipment_AndReturnWithId()
    {
        // Arrange
        var equipmentDto = new EquipmentDTO
        {
            Name = "Test Sword",
            Description = "A sharp blade",
            ShortDescr = "Sword",
            Price = 50.0m,
            Weight = 2.5m,
            EquipmentType = "weapon",
            RelatedSkill = "Swords",
            IsApproved = true,
            IsTwoHanded = false
        };

        // Act
        var result = await _repository.Create(equipmentDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Test Sword", result.Name);
        Assert.Equal(50.0m, result.Price);
    }

    [Fact]
    public async Task GetById_ShouldReturnEquipment_WhenExists()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var equipment = new Equipment
        {
            Name = "GetById Test Shield",
            Description = "A sturdy shield",
            ShortDescr = "Shield",
            Price = 30.0m,
            Weight = 5.0m,
            EquipmentType = "shield",
            IsApproved = true
        };
        context.Equipment.Add(equipment);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetById(equipment.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("GetById Test Shield", result.Name);
        Assert.Equal("shield", result.EquipmentType);
    }

    [Fact]
    public async Task GetByName_ShouldReturnEquipment_WhenExists()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var equipment = new Equipment
        {
            Name = "Unique Named Axe",
            Description = "An axe",
            EquipmentType = "weapon",
            IsApproved = true
        };
        context.Equipment.Add(equipment);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByName("Unique Named Axe");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Unique Named Axe", result.Name);
    }

    [Fact]
    public async Task GetByName_ShouldReturnEmptyDTO_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByName("NonExistent Equipment Name 12345");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllEquipment()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var equipment1 = new Equipment { Name = "GetAll Item 1", EquipmentType = "weapon" };
        var equipment2 = new Equipment { Name = "GetAll Item 2", EquipmentType = "armor" };
        context.Equipment.AddRange(equipment1, equipment2);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAll();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count() >= 2);
    }

    [Fact]
    public async Task GetAllApproved_ShouldReturnOnlyApprovedEquipment()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var approved = new Equipment { Name = "Approved Bow", EquipmentType = "weapon", IsApproved = true };
        var notApproved = new Equipment { Name = "NotApproved Dagger", EquipmentType = "weapon", IsApproved = false };
        context.Equipment.AddRange(approved, notApproved);
        await context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllApproved();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result, e => e.Name == "Approved Bow");
        Assert.DoesNotContain(result, e => e.Name == "NotApproved Dagger");
    }

    [Fact]
    public async Task Delete_ShouldRemoveEquipment_WhenExists()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var equipment = new Equipment
        {
            Name = "Equipment To Delete",
            EquipmentType = "other"
        };
        context.Equipment.Add(equipment);
        await context.SaveChangesAsync();
        var equipmentId = equipment.Id;

        // Act
        await _repository.Delete(equipmentId);

        // Assert
        using var verifyContext = _fixture.CreateContext();
        var deletedEquipment = await verifyContext.Equipment.FindAsync(equipmentId);
        Assert.Null(deletedEquipment);
    }

    [Fact]
    public async Task Update_ShouldModifyEquipment()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var equipment = new Equipment
        {
            Name = "Original Equipment Name",
            Description = "Original Description",
            Price = 10.0m,
            Weight = 1.0m,
            EquipmentType = "weapon",
            IsApproved = false
        };
        context.Equipment.Add(equipment);
        await context.SaveChangesAsync();

        var updateDto = new EquipmentDTO
        {
            Id = equipment.Id,
            Name = "Updated Equipment Name",
            Description = "Updated Description",
            Price = 25.0m,
            Weight = 1.5m,
            EquipmentType = "weapon",
            IsApproved = true
        };

        // Act
        var result = await _repository.Update(updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Equipment Name", result.Name);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal(25.0m, result.Price);
        Assert.True(result.IsApproved);
    }
}
