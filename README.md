# מערכת השאלת ספרים בספרייה: שרת

פרויקט סיום בקורס ‎.NET Web API. השרת מנהל ספרייה קהילתית: ספרים, קטגוריות, עותקים, וקוראות שמשאילות ומחזירות ספרים. ספרניות מנהלות את הקטלוג ואת ההשאלות, ומנהלת אחראית על המשתמשות ועל המחיקות. השרת בנוי ב-ASP.NET Core 8 עם EF Core ו-PostgreSQL, ומאובטח ב-JWT עם שלושה תפקידים.

הלקוח (React) נמצא ב-repository נפרד: `library-client`.

## בענן (Render)

| | כתובת |
|---|---|
| האתר (לקוח) | https://library-client-h52y.onrender.com |
| השרת (Swagger) | https://library-server-cz2j.onrender.com/swagger |

השירותים בתוכנית החינמית "נרדמים" אחרי 15 דקות בלי שימוש. הפנייה הראשונה אחרי זה לוקחת בערך דקה, עד שהשרת מתעורר. פרטי הכניסה של משתמשי הדמו נמצאים למטה.

## המשאב המוגבל והטיפול בתחרות

**המשאב המוגבל הוא עותק ספר (`BookCopy`)**: כל עותק יכול להיות מושאל לקוראת אחת בלבד בכל רגע. התחרות מתרחשת כששתי קוראות לוחצות "השאילי" על אותו עותק באותו רגע. שתי הבקשות קוראות את העותק כשהוא עדיין פנוי, ובלי הגנה שתיהן היו מאושרות.

הפתרון הוא **optimistic concurrency**:
- לעותק ולקוראת יש שדה `Version` (‏Guid עם `[ConcurrencyCheck]`), שמתחלף בכל שמירה בדריסה של `SaveChangesAsync` ב-`AppDbContext`.
- בזמן העדכון EF מוסיף את הערך הישן לתנאי ה-`WHERE`. אם מישהי הספיקה לשנות את השורה בינתיים, אף שורה לא מתעדכנת, ו-EF זורק `DbUpdateConcurrencyException`.
- ה-Repository תופס **רק** את החריגה הזו, כותב ללוג ברמת **Warning**, ומתרגם אותה ל-`ConcurrencyConflictException` שמוגדרת בשכבת Application. כך Application לא מכיר את EF.
- ה-Service מחזיר **409 Conflict** עם ההודעה "העותק נתפס על ידי משתמשת אחרת. נסי שוב.", והלקוח מציג אותה ומרענן את רשימת העותקים.
- הבדיקה העסקית והשמירה רצות באותה טרנזקציה: העדכון של העותק, של הקוראת וההשאלה החדשה נשמרים יחד או לא נשמרים בכלל.

**נקודת תחרות נוספת היא מכסת ההשאלות.** קוראת עם 2 השאלות שמשאילה שני עותקים שונים בו-זמנית הייתה יכולה לעבור את המכסה (3). לכן גם על הקוראת יש `Version`, וכל השאלה מסמנת אותה כ"שונתה". הבקשה השנייה נכשלת ב-409.

**ההוכחה** נמצאת ב-`tests/Library.Tests/Concurrency`. שני `DbContext` נפרדים על SQLite בזיכרון קוראים את אותה ישות, ושניהם משנים אותה. הראשון שומר בהצלחה, והשני מקבל `DbUpdateConcurrencyException`. אחרי זה בבסיס הנתונים יש השאלה פעילה אחת בלבד, והקוראת נשארת עם 3 השאלות ולא 4.

## הרצה מקומית

**דרישות מוקדמות:** ‏.NET 8 SDK, ‏Docker, והכלי `dotnet-ef` (`dotnet tool install --global dotnet-ef`).

**1. בסיס נתונים: PostgreSQL ב-Docker**
```
docker run --name library-pg -e POSTGRES_USER=library -e POSTGRES_PASSWORD=<סיסמה> -e POSTGRES_DB=library -p 5432:5432 -d postgres:17
```

**2. סודות ב-User Secrets.** הם לא נמצאים בקוד ולא ב-repo. מריצים מתיקיית ה-solution:
```
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=library;Username=library;Password=<סיסמה>" --project src/Library.API
dotnet user-secrets set "Jwt:Key" "<מחרוזת אקראית של 32 תווים לפחות>" --project src/Library.API
```

**3. הרצה**
```
dotnet run --project src/Library.API
```
בעלייה השרת מריץ לבד את ה-migrations ומכניס נתוני דמו, אם בסיס הנתונים ריק. לא צריך שום התערבות ידנית בבסיס הנתונים. Swagger זמין בכתובת `https://localhost:56124/swagger`. כדי להשתמש ב-endpoints מוגנים, מתחברים דרך `POST /api/auth/login`, מעתיקים את ה-token ומדביקים אותו בכפתור **Authorize**.

**4. בדיקות**
```
dotnet test
```

## משתמשי דמו

| תפקיד | מייל | סיסמה |
|---|---|---|
| מנהלת | admin@library.local | Admin123! |
| ספרנית | librarian@library.local | Librarian123! |
| קוראת 1 | reader1@library.local | Reader123! |
| קוראת 2 | reader2@library.local | Reader123! |

יש שתי קוראות כדי שאפשר יהיה להדגים התנגשות. בנתוני הדמו, לספר "הסוד של מחנה הקיץ" יש עותק יחיד. כל הספרים, המחברים ומספרי ה-ISBN בנתוני הדמו בדויים.

## ארכיטקטורה: Clean Architecture

```
src/
  Library.Domain          ישויות ו-Enums. אפס תלויות חיצוניות (בלי EF Core)
  Library.Application     ממשקים, DTOs, Services, AutoMapper Profiles, Result
  Library.Infrastructure  DbContext, Repositories, Migrations, JWT, BCrypt
  Library.API             Controllers, Middleware, Program.cs, nlog.config
tests/
  Library.Tests           xUnit + Moq, ובדיקות התנגשות על SQLite
```

**ההבדל מהמבנה שנלמד** (Core / Data / Service / API):
- Core מתפצל לשניים: **Domain**, שמכיל ישויות טהורות, ו-**Application**, שמכיל את החוזים, ה-DTOs והלוגיקה העסקית.
- ה-Service עובר ל-Application. ה-Data הופך ל-**Infrastructure**, שמכיל את כל מה שנוגע "לעולם החיצוני": בסיס נתונים, JWT והצפנת סיסמאות.
- הממשקים של ה-Repositories נמצאים ב-Application, והמימוש שלהם ב-Infrastructure (Dependency Inversion).
- כיוון התלויות: Application ← Domain, ‏Infrastructure ← Application. ‏API מכיר את Infrastructure רק לצורך רישום השירותים ב-Program.cs.

## הערות על החלטות

- **Lifetimes ב-DI.** ה-`DbContext`, ה-Repositories וה-Services רשומים כ-Scoped: מופע אחד לכל בקשה. **Singleton שמחזיק Scoped הוא באג.** ה-Singleton חי לאורך כל חיי השרת, ולכן הוא "תופס" את ה-DbContext הראשון ומשתמש בו בכל הבקשות, במקביל ומכמה threads. זה גורם לשגיאות, לנתונים ישנים ב-Change Tracker ולדליפת זיכרון. שירותים בלי מצב, כמו `JwtTokenService` ו-`BcryptPasswordHasher`, רשומים כ-Singleton.
- **בדיקת ModelState.** אין `if (!ModelState.IsValid)` בפעולות. `[ApiController]` בודק את ה-ModelState אוטומטית ומחזיר 400 לפני שהפעולה רצה, ולכן בדיקה ידנית הייתה כפל קוד. ולידציה עסקית, כמו ISBN כפול או מכסה, נמצאת ב-Services.
- **מחיקה מול "לא זמין".** היסטוריית ההשאלות לא נמחקת לעולם, כי הקשר מוגדר כ-Restrict. לכן ספר או עותק שהושאלו אי פעם לא נמחקים, והמחיקה מחזירה 409. עותק שנהרס או אבד מסמנים כ-`Lost` דרך `PATCH /api/copies/{id}`.
- **אזהרת ה-build על AutoMapper 14.0.0** (GHSA-rvv3-g6hj-g44x: קריסה ממיפוי של אובייקט מקונן מאוד). נשארנו על גרסה 14 בכוונה, כי זו הגרסה החינמית האחרונה. החולשה לא רלוונטית כאן משתי סיבות: ה-DTOs שטוחים, ו-ASP.NET Core דוחה JSON שמקונן יותר מ-64 רמות. האזהרה נשארה גלויה בכוונה, ולא הושתקה.

## לוגים

- **NLog**, דרך `ILogger` הרגיל. ההגדרות נמצאות ב-`src/Library.API/nlog.config`.
- הלוגים נכתבים לקונסול ולקובץ יומי ב-`bin/.../logs/`.
- כל שורה כוללת את ה-**CorrelationId** של הבקשה, מהכותרת `X-Correlation-Id`.
- הרמות: **Information** לכל בקשה ולהשאלה או החזרה, **Warning** להתנגשות ולדחייה עסקית, **Error** לחריגה שלא טופלה.
- סיסמאות, tokens וגוף בקשות ההרשמה וההתחברות לא נכתבים ללוג.
