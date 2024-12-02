using ModelDiffrence.Models;
using System.Linq.Expressions;
namespace ModelDiffrence;

public static class ModelDiffHelper
{
    // Mevcut metot
    public static bool AreModelsDifferent<T>(T model1, T model2)
    {
        return AreModelsDifferent(model1, model2, null);
    }

    // Aşırı yükleme ile haric tutulacak kolonlar
    public static bool AreModelsDifferent<T>(T model1, T model2, IEnumerable<string>? excludedProperties)
    {
        return AreModelsDifferent<T>(model1, model2, excludedProperties?.ToArray());
    }

    // Aşırı yükleme ile haric tutulacak kolonlar
    public static bool AreModelsDifferent<T>(T model1, T model2, string[]? excludedProperties)
    {
        if (model1 == null || model2 == null)
        {
            throw new ArgumentNullException("Model değerlerinden biri null olamaz.");
        }

        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            // Haric tutulan kolonları kontrol et
            if (excludedProperties != null && excludedProperties.Any(s => s.Equals(property.Name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (property.CanRead)
            {
                var value1 = property.GetValue(model1);
                var value2 = property.GetValue(model2);

                if (!object.Equals(value1, value2))
                {
                    return true; // İlk farklılık bulunduğunda true döner
                }
            }
        }

        return false; // Tüm özellikler aynıysa false döner
    }

    public static bool AreModelsDifferent<T>(
        T model1,
        T model2,
        string[]? excludedProperties = null,
        string[]? includedProperties = null
    )
    {
        if (model1 == null || model2 == null)
        {
            throw new ArgumentNullException("Model değerlerinden biri null olamaz.");
        }

        var properties = typeof(T).GetProperties().Where(p => p.CanRead);

        // Haric tutulanlar
        if (excludedProperties != null)
        {
            properties = properties.Where(p => !excludedProperties.Any(s => s.Equals(p.Name, StringComparison.OrdinalIgnoreCase)));
        }

        // Dahil edilecek özellikler
        if (includedProperties != null)
        {
            properties = properties.Where(p => includedProperties.Any(s => s.Equals(p.Name, StringComparison.OrdinalIgnoreCase)));
        }

        foreach (var property in properties)
        {
            var value1 = property.GetValue(model1);
            var value2 = property.GetValue(model2);

            if (!object.Equals(value1, value2))
            {
                return true; // İlk farklılık bulunduğunda true döner
            }
        }

        return false; // Tüm özellikler aynıysa false döner
    }

    public static bool AreModelsDifferent<T>(
        T model1,
        T model2,
        IEnumerable<string>? excludedProperties = null,
        IEnumerable<string>? includedProperties = null
    )
    {
        return AreModelsDifferent<T>(model1, model2, excludedProperties?.ToArray(), includedProperties?.ToArray());
    }

    public static DiffResult GetModelDifferences<T>(
        T model1,
        T model2,
        IEnumerable<string>? excludedProperties = null,
        IEnumerable<string>? includedProperties = null
    )
    {
        if (model1 == null || model2 == null)
        {
            throw new ArgumentNullException("Model değerlerinden biri null olamaz.");
        }

        var properties = typeof(T).GetProperties().Where(p => p.CanRead);

        // Haric tutulanlar
        if (excludedProperties != null)
        {
            properties = properties.Where(p => !excludedProperties.Any(s => s.Equals(p.Name, StringComparison.OrdinalIgnoreCase)));
        }

        // Dahil edilecek özellikler
        if (includedProperties != null)
        {
            properties = properties.Where(p => includedProperties.Any(s => s.Equals(p.Name, StringComparison.OrdinalIgnoreCase)));
        }

        var result = new DiffResult();

        foreach (var property in properties)
        {
            var value1 = property.GetValue(model1);
            var value2 = property.GetValue(model2);

            if (!object.Equals(value1, value2))
            {
                result.IsChanged = true;
                result.ChangedFields.Add(new ChangedField { Name = property.Name });
            }
        }

        return result;
    }

    // Property adlarını lambda ifadesinden çıkaran metot
    public static IEnumerable<string> GetExcludedColumns<T>(Expression<Func<T, object>> propertiesSelector)
    {
        if (propertiesSelector.Body is NewExpression newExpression)
        {
            return newExpression.Members.Select(m => m.Name);
        }

        throw new ArgumentException("Lütfen bir 'new { }' ifadesi kullanın.");
    }

    public static IEnumerable<string> GetIncludedColumns<T>(Expression<Func<T, object>> propertiesSelector)
    {
        return GetExcludedColumns<T>(propertiesSelector);
    }
}