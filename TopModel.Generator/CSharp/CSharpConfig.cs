﻿using System.Text.RegularExpressions;
using TopModel.Core;

namespace TopModel.Generator.CSharp;

/// <summary>
/// Paramètres pour la génération du C#.
/// </summary>
public class CSharpConfig : GeneratorConfigBase
{
    private string? _referencesModelPath;

    /// <summary>
    /// Localisation du modèle persisté, relative au répertoire de génération. Par défaut : {app}.{module}.Models.
    /// </summary>
    public string PersistantModelPath { get; set; } = "{app}.{module}.Models";

    /// <summary>
    /// Localisation des classes de références persistées, relative au répertoire de génération. Par défaut égal à "PersistantModelPath".
    /// </summary>
    public string PersistantReferencesModelPath { get => _referencesModelPath ?? PersistantModelPath; set => _referencesModelPath = value; }

    /// <summary>
    /// Localisation du modèle non persisté, relative au répertoire de génération. Par défaut : {app}.{module}.Models/Dto.
    /// </summary>
    public string NonPersistantModelPath { get; set; } = "{app}.{module}.Models/Dto";

    /// <summary>
    /// Localisation du l'API générée (client ou serveur), relatif au répertoire de génération. Par défaut : "{app}.Web".
    /// </summary>
    public string ApiRootPath { get; set; } = "{app}.Web";

    /// <summary>
    /// Chemin vers lequel sont créés les fichiers d'endpoints générés, relatif à la racine de l'API. Par défaut : "{module}".
    /// </summary>
    public string ApiFilePath { get; set; } = "{module}";

    /// <summary>
    /// Mode de génération de l'API ("Client" ou "Server").
    /// </summary>
    public string? ApiGeneration { get; set; }

    /// <summary>
    /// Génère des contrôleurs d'API synchrones.
    /// </summary>
    public bool NoAsyncControllers { get; set; }

    /// <summary>
    /// Localisation du DbContext, relatif au répertoire de génération.
    /// </summary>
    public string? DbContextPath { get; set; }

    /// <summary>
    /// Nom du DbContext. Par défaut : {app}DbContext.
    /// </summary>
    public string DbContextName { get; set; } = "{app}DbContext";

#nullable disable

    /// <summary>
    /// Chemin vers lequel générer les interfaces d'accesseurs de référence. Par défaut : {DbContextPath}/Reference.
    /// </summary>
    public string ReferenceAccessorsInterfacePath { get; set; }

    /// <summary>
    /// Chemin vers lequel générer les implémentation d'accesseurs de référence. Par défaut : {DbContextPath}/Reference.
    /// </summary>
    public string ReferenceAccessorsImplementationPath { get; set; }
#nullable enable

    /// <summary>
    /// Nom des accesseurs de référence (préfixé par 'I' pour l'interface). Par défaut : {module}ReferenceAccessors.
    /// </summary>
    public string ReferenceAccessorsName { get; set; } = "{module}ReferenceAccessors";

    /// <summary>
    /// Utilise les migrations EF pour créer/mettre à jour la base de données.
    /// </summary>
    public bool UseEFMigrations { get; set; }

    /// <summary>
    /// Utilise des noms de tables et de colonnes en lowercase.
    /// </summary>
    public bool UseLowerCaseSqlNames { get; set; }

    /// <summary>
    /// Le nom du schéma de base de données à cibler (si non renseigné, EF utilise 'dbo'/"public').
    /// </summary>
    public string? DbSchema { get; set; }

    /// <summary>
    /// Utilise les features C# 10 dans la génération.
    /// </summary>
    public bool UseLatestCSharp { get; set; } = true;

    /// <summary>
    /// Si on génère avec Kinetix.
    /// </summary>
    public bool Kinetix { get; set; } = true;

    /// <summary>
    /// Retire les attributs de colonnes sur les alias.
    /// </summary>
    public bool NoColumnOnAlias { get; set; }

    /// <summary>
    /// Considère tous les classes comme étant non-persistantes (= pas d'attribut SQL).
    /// </summary>
    public bool NoPersistance { get; set; }

    /// <summary>
    /// Utilise des enums au lieu de strings pour les PKs de listes de référence statiques.
    /// </summary>
    public bool EnumsForStaticReferences { get; set; }

    /// <summary>
    /// Annote les tables et les colonnes générées par EF avec les commentaires du modèle (nécessite `UseEFMigrations`).
    /// </summary>
    public bool UseEFComments { get; set; }

    /// <summary>
    /// Détermine si une classe utilise une enum pour sa clé primaire.
    /// </summary>
    /// <param name="classe">Classe.</param>
    /// <returns>Oui/non.</returns>
    public bool CanClassUseEnums(Class classe)
    {
        return EnumsForStaticReferences
            && classe.EnumKey != null
            && classe.Values.All(r => !Regex.IsMatch(r.Value[classe.EnumKey].ToString() ?? string.Empty, "^\\d"));
    }

    /// <summary>
    /// Récupère la valeur par défaut d'une propriété en C#.
    /// </summary>
    /// <param name="property">La propriété.</param>
    /// <param name="availableClasses">Classes disponibles dans le générateur.</param>
    /// <returns>La valeur par défaut.</returns>
    public string GetDefaultValue(IProperty property, IEnumerable<Class> availableClasses)
    {
        var fp = property as IFieldProperty;

        if (fp?.DefaultValue == null || fp.DefaultValue == "null" || fp.DefaultValue == "undefined")
        {
            return "null";
        }

        var prop = fp is AliasProperty alp ? alp.Property : fp;
        var ap = prop as AssociationProperty;

        if (ap?.Association.EnumKey != null && availableClasses.Contains(ap!.Association))
        {
            if (CanClassUseEnums(ap.Association))
            {
                return $"{ap.Association.Name}.{ap.Association.EnumKey.Name}s.{fp.DefaultValue}";
            }
            else
            {
                var refName = ap.Association.Values.SingleOrDefault(rv => rv.Value[ap.Association.EnumKey] == fp.DefaultValue)?.Name;
                if (refName != null)
                {
                    return $"{ap.Association.Name}.{refName}";
                }
            }
        }

        if (fp.Domain?.CSharp?.Type == "string")
        {
            return $@"""{fp.DefaultValue}""";
        }

        return fp.DefaultValue;
    }

    /// <summary>
    /// Récupère le nom du DbContext.
    /// </summary>
    /// <param name="appName">Nom de l'application.</param>
    /// <returns>Nom.</returns>
    public string GetDbContextName(string appName)
    {
        return DbContextName?.Replace("{app}", appName.Replace(".", string.Empty))
            ?? throw new ModelException("Le DbContext doit être renseigné.");
    }

    /// <summary>
    /// Récupère le chemin vers un fichier de classe à générer.
    /// </summary>
    /// <param name="classe">La classe.</param>
    /// <returns>Chemin.</returns>
    public string GetModelPath(Class classe)
    {
        var baseModelPath = classe.IsPersistent && !NoPersistance
            ? classe.Reference
                ? PersistantReferencesModelPath
                : PersistantModelPath
            : NonPersistantModelPath;
        return baseModelPath.Replace("{app}", classe.Namespace.App).Replace("{module}", classe.Namespace.Module.Replace('.', Path.DirectorySeparatorChar));
    }

    /// <summary>
    /// Récupère le namespace d'une classe.
    /// </summary>
    /// <param name="classe">La classe.</param>
    /// <returns>Namespace.</returns>
    public string GetNamespace(Class classe)
    {
        var baseModelPath = classe.IsPersistent && !NoPersistance
            ? classe.Reference
                ? PersistantReferencesModelPath
                : PersistantModelPath
            : NonPersistantModelPath;
        var ns = baseModelPath.Replace("/", ".")
            .Replace(".Dto", string.Empty);
        return ns[Math.Max(0, ns.IndexOf("{app}"))..]
            .Replace("{app}", classe.Namespace.App)
            .Replace("{module}", classe.Namespace.Module);
    }
}