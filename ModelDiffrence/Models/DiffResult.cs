namespace ModelDiffrence.Models;

public class DiffResult
{
    public bool IsChanged { get; set; }
    public List<ChangedField> Changes { get; set; } = new List<ChangedField>();
}