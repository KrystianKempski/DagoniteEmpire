using AutoMapper;
using DA_Business.Mapper;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DA_Business.Tests.Mapper
{
    public class MappingProfileTests
    {
        [Fact]
        public void MappingProfile_CanCreateMapper()
        {
            // Arrange - AutoMapper 15.x requires MapperConfigurationExpression + ILoggerFactory
            var configExpression = new MapperConfigurationExpression();
            configExpression.AddProfile<MappingProfile>();
            var config = new MapperConfiguration(configExpression, NullLoggerFactory.Instance);

            // Act - Creating mapper should not throw
            var mapper = config.CreateMapper();

            // Assert
            Assert.NotNull(mapper);
        }

        [Fact]
        public void MappingProfile_AllowsNullCollections()
        {
            // Assert - profile should have null handling enabled
            var profile = new MappingProfile();
            Assert.True(profile.AllowNullCollections);
            Assert.True(profile.AllowNullDestinationValues);
        }
    }
}
