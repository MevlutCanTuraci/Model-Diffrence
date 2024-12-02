using ModelDiffrence.Models;
namespace ModelDiffrence;


internal class Program
{
    static int Main(string[] args)
    {
        var model1 = new User
        {
            Name            = "Mevlut Can",
            Surname         = "Turacı",
            Age             = 20,
            Job             = "Developer",
            Sallary         = 10,
            UpdatedTime     = DateTime.Now,
        };

        var model2 = new User
        {
            Name            = "Mevlut Can",
            Surname         = "Turaci",
            Age             = 20,
            Job             = "Developer 1",
            Sallary         = 10,
            UpdatedTime     = DateTime.Now.AddMilliseconds(Random.Shared.Next(2500, 999990)),
        };

        //var isChanged = Usege_1(model1, model2);
        //var isChanged = Usege_2(model1, model2);

        var isChanged = Usege_3ReturnedFileds(model1, model2);

        Console.WriteLine("Model is changed: {0}", isChanged.IsChanged);
        Console.WriteLine("Changed fields:");
        foreach (var item in isChanged.ChangedFields)
        {
            Console.WriteLine("- {0}", item.Name);
        }
        
        return 1;
    }

    public static bool Usege_1<T>(T model1, T model2)
    {
        var excludedColumns = ModelDiffHelper.GetExcludedColumns<User>(p => new
        {
            p.UpdatedTime
        });

        var includedColumns = ModelDiffHelper.GetIncludedColumns<User>(p => new
        {
            p.Name,
            p.Surname
        });
        return ModelDiffHelper.AreModelsDifferent(model1, model2, excludedProperties: excludedColumns, includedProperties: includedColumns);        
    }

    public static bool Usege_2<T>(T model1, T model2)
    {
        var excludedColumns = ModelDiffHelper.GetExcludedColumns<User>(p => new
        {
            p.UpdatedTime
        });

        return ModelDiffHelper.AreModelsDifferent(model1, model2, excludedProperties: excludedColumns);
    }

    public static DiffResult Usege_3ReturnedFileds<T>(T model1, T model2)
    {
        var excludedColumns = ModelDiffHelper.GetExcludedColumns<User>(p => new
        {
            p.UpdatedTime
        });

        return ModelDiffHelper.GetModelDifferences(model1, model2, excludedProperties: excludedColumns);
    }
}

public class User
{
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public int Age { get; set; }
    public string Job { get; set; } = null!;
    public decimal Sallary { get; set; }
    public DateTime UpdatedTime { get; set; }
}