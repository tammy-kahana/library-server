using System.ComponentModel.DataAnnotations;
using Library.Domain.Enums;

namespace Library.Application.DTOs.Copies;

/// <summary>PATCH /api/copies/{id}: סימון עותק כ-Lost (אבד/נהרס) או החזרתו ל-Available.</summary>
public class CopyStatusRequest
{
    /// <summary>
    /// EnumDataType דוחה ערך שלא מוגדר ב-Enum, כולל 0
    /// (ה-Enum מתחיל מ-1, כך שערך חסר לא יתפרש בטעות כסטטוס תקין).
    /// </summary>
    [EnumDataType(typeof(CopyStatus))]
    public CopyStatus Status { get; set; }
}
