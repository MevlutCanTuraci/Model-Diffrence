namespace ModelDiffrence.Models;

public class DiffResult
{
    public bool IsChanged { get; set; }
    public List<ChangedField> ChangedFields { get; set; } = new List<ChangedField>();
}