namespace ModelDiffrence.Models;

public class ChangedField
{
    public string? Path { get; set; } // Hiyerarşik yolu gösterir (örneğin: "Address.CityId")
    public string? Name { get; set; } // Sadece alan adı (örneğin: "CityId")
    public string? Parent { get; set; } // Üst nesne (örneğin: "Address")
    public int? Index { get; set; } // Koleksiyonlar için sıralama bilgisi
}