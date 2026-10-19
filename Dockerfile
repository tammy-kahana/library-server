# בנייה והרצה של השרת ב-Render (ל-.NET אין שם סביבת הרצה מובנית, ולכן Docker)

# ---- שלב 1: בנייה עם ה-SDK ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# קודם רק קובצי הפרויקטים: כך שחזור החבילות נשמר במטמון כל עוד הן לא השתנו
COPY src/Library.Domain/Library.Domain.csproj src/Library.Domain/
COPY src/Library.Application/Library.Application.csproj src/Library.Application/
COPY src/Library.Infrastructure/Library.Infrastructure.csproj src/Library.Infrastructure/
COPY src/Library.API/Library.API.csproj src/Library.API/
RUN dotnet restore src/Library.API/Library.API.csproj

COPY src/ src/
RUN dotnet publish src/Library.API/Library.API.csproj -c Release -o /app/publish --no-restore

# ---- שלב 2: הרצה, על סביבה קלה בלי ה-SDK ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# תיקיית הלוגים של NLog, ורק אחר כך מעבר למשתמש בלי הרשאות מנהל
RUN mkdir -p /app/logs && chown $APP_UID /app/logs
USER $APP_UID

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Render מעביר את הפורט במשתנה PORT. בלעדיו (למשל בהרצה מקומית ב-Docker): 8080
ENTRYPOINT ["sh", "-c", "dotnet Library.API.dll --urls http://0.0.0.0:${PORT:-8080}"]
