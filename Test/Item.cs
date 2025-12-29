namespace Test;

public class Item
{
    public string Name { get; set; }
    public int Weight { get; set; }

    public bool Equals(Item? other)
    {
        if (other is null) return false;
        return Name == other.Name;
    }

    public override bool Equals(object? obj) => Equals(obj as Item);

    public override int GetHashCode() => Name?.GetHashCode() ?? 0;

    public Item Clone() => new Item { Name = Name, Weight = Weight };
}
