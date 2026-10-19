using AutoMapper;
using Library.Application.Mappings;

namespace Library.Tests.Unit;

/// <summary>AutoMapper אמיתי עם ה-Profiles של הפרויקט: בבדיקות מדמים רק את ה-Repositories.</summary>
public static class TestMapper
{
    public static readonly MapperConfiguration Configuration =
        new(cfg => cfg.AddMaps(typeof(BookProfile).Assembly));

    public static IMapper Create() => Configuration.CreateMapper();
}
