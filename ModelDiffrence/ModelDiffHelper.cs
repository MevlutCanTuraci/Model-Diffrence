using System.Collections;
using ModelDiffrence.Models;
using System.Linq.Expressions;
using System.Reflection;

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
                result.Changes.Add(new ChangedField { Name = property.Name });
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
    
        ///////////////////////////////////

    public static DiffResult CompareObjects(object oldObj, object newObj, string parentName = "")
    {
        var result = new DiffResult();
        if (oldObj == null || newObj == null)
        {
            result.IsChanged = oldObj != newObj;
            return result;
        }

        var type = oldObj.GetType();

        // Property'leri iterate et
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var oldValue = property.GetValue(oldObj);
            var newValue = property.GetValue(newObj);

            // Parent bilgisini oluştur
            var fullName = string.IsNullOrEmpty(parentName) ? property.Name : $"{parentName}.{property.Name}";

            if (IsSimpleType(property.PropertyType))
            {
                // Basit türleri kıyasla
                if (!Equals(oldValue, newValue))
                {
                    result.IsChanged = true;
                    result.Changes.Add(new ChangedField
                    {
                        Path = fullName,         // Tam yol
                        Name = property.Name,    // Alan adı
                        Parent = (string.IsNullOrEmpty(parentName?.Trim())) ? null : parentName,     // Üst nesne
                        Index = null             // Basit türlerde Index yok
                    });
                }
            }
            else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
            {
                // Koleksiyonları kontrol et
                CompareCollections(result, (IEnumerable)oldValue, (IEnumerable)newValue, fullName, property.Name);
            }
            else
            {
                // Kompleks nesneler için rekürsif çağrı
                var nestedResult = CompareObjects(oldValue, newValue, fullName);
                if (nestedResult.IsChanged)
                {
                    result.IsChanged = true;
                    result.Changes.AddRange(nestedResult.Changes);
                }
            }
        }

        return result;
    }

    private static void CompareCollections(DiffResult result, IEnumerable oldCollection, IEnumerable newCollection, string parentName, string propertyName)
    {
        var oldList = oldCollection?.Cast<object>().ToList() ?? new List<object>();
        var newList = newCollection?.Cast<object>().ToList() ?? new List<object>();

        var maxCount = Math.Max(oldList.Count, newList.Count);
        for (int i = 0; i < maxCount; i++)
        {
            var oldItem = i < oldList.Count ? oldList[i] : null;
            var newItem = i < newList.Count ? newList[i] : null;
            var fullName = $"{parentName}[{i}]";

            var nestedResult = CompareObjects(oldItem, newItem, fullName);
            if (nestedResult.IsChanged)
            {
                result.IsChanged = true;
                foreach (var change in nestedResult.Changes)
                {
                    // Koleksiyon elemanları için Index bilgisi ekle
                    result.Changes.Add(new ChangedField
                    {
                        Path = change.Path,
                        Name = change.Name,
                        Parent = propertyName,
                        Index = i
                    });
                }
            }
        }
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime);
    }
}