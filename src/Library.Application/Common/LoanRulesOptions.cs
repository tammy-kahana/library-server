using System.ComponentModel.DataAnnotations;

namespace Library.Application.Common;

/// <summary>
/// חוקי ההשאלה, מסעיף "LoanRules" ב-appsettings.json.
/// אין כאן ערכי ברירת מחדל בכוונה: המקור היחיד לערכים הוא הקונפיגורציה,
/// ואם הם חסרים, הולידציה בעליית השרת נכשלת (0 לא עובר את ה-Range).
/// </summary>
public class LoanRulesOptions
{
    public const string SectionName = "LoanRules";

    [Range(1, 20)]
    public int MaxActiveLoans { get; set; }

    [Range(1, 90)]
    public int LoanPeriodDays { get; set; }
}
