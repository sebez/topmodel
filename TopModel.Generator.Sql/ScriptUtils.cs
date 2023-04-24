﻿using TopModel.Core;
using TopModel.Core.Model.Implementation;

namespace TopModel.Generator.Sql;

/// <summary>
/// Classe utilitaire pour écritre du SQL.
/// </summary>
public static class ScriptUtils
{
    public const string InsertKeyName = "InsertKey";

    /// <summary>
    /// Retourne le nom du type de table SQL correspondant à la classe.
    /// </summary>
    /// <param name="classe">Classe.</param>
    /// <returns>Nom du type de table.</returns>
    public static string GetTableTypeName(this Class classe)
    {
        if (classe == null)
        {
            throw new ArgumentNullException(nameof(classe));
        }

        return classe.SqlName + "_TABLE_TYPE";
    }

    /// <summary>
    /// Prépare une chaîne de caractères à être écrite dans un script SQL.
    /// </summary>
    /// <param name="raw">La chaîne à préparer.</param>
    /// <returns>La chaîne de caractère équivalente, mise au format SQL.</returns>
    public static string PrepareDataToSqlDisplay(string? raw)
    {
        return string.IsNullOrEmpty(raw)
            ? string.Empty
            : raw.Replace("'", "''");
    }

    public static bool ShouldQuoteValue(this DomainImplementation domain)
    {
        return (domain?.Type ?? string.Empty).Contains("varchar")
            || domain?.Type == "text"
            || domain?.Type == "uniqueidentifier"
            || domain?.Type == "uuid"
            || (domain?.Type ?? string.Empty).Contains("date")
            || (domain?.Type ?? string.Empty).Contains("time");
    }
}