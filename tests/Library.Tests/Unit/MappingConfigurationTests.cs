namespace Library.Tests.Unit;

public class MappingConfigurationTests
{
    /// <summary>נכשלת אם ב-DTO כלשהו יש שדה שלא ממופה ולא סומן במפורש כ-Ignore.</summary>
    [Fact]
    public void AutoMapperConfiguration_IsValid() =>
        TestMapper.Configuration.AssertConfigurationIsValid();
}
