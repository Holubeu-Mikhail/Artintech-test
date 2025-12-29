using System.Collections;

namespace Test;

public class Inventory : IEnumerable<Item>
{
    private readonly List<Item> _items = [];
    private readonly object _lock = new();
    private const int MaxWeight = 100;
    private int _currentWeight = 0;

    public IReadOnlyList<Item> Items
    {
        get
        {
            lock (_lock)
            {
                return _items.Select(i => i.Clone()).ToList().AsReadOnly();
            }
        }
    }

    public int CurrentWeight
    {
        get
        {
            lock (_lock)
            {
                return _currentWeight;
            }
        }
    }

    public void AddItem(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.Weight <= 0)
        {
            throw new ArgumentException("Weight must be positive", nameof(item));
        }

        lock (_lock)
        {
            var existingItem = _items.FirstOrDefault(i => i.Name == item.Name);

            if (existingItem != null)
            {
                if (_currentWeight + item.Weight > MaxWeight)
                {
                    throw new InvalidOperationException($"Cannot add item. Weight would exceed maximum. Current: {_currentWeight}, Added: {item.Weight}, Max: {MaxWeight}");
                }

                existingItem.Weight += item.Weight;
                _currentWeight += item.Weight;
            }
            else
            {
                if (_currentWeight + item.Weight > MaxWeight)
                {
                    throw new InvalidOperationException($"Cannot add item. Weight would exceed maximum. Current: {_currentWeight}, Added: {item.Weight}, Max: {MaxWeight}");
                }

                _items.Add(item.Clone());
                _currentWeight += item.Weight;
            }
        }
    }

    public bool RemoveItem(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (_lock)
        {
            var existingItem = _items.FirstOrDefault(i => i.Name == item.Name);

            if (existingItem == null)
            {
                return false;
            }

            if (existingItem.Weight < item.Weight)
            {
                throw new InvalidOperationException($"Cannot remove {item.Weight} weight. Item has only {existingItem.Weight}");
            }

            if (existingItem.Weight == item.Weight)
            {
                _items.Remove(existingItem);
                _currentWeight -= item.Weight;
            }
            else
            {
                existingItem.Weight -= item.Weight;
                _currentWeight -= item.Weight;
            }

            return true;
        }
    }

    public bool RemoveItemByName(string name)
    {
        lock (_lock)
        {
            var item = _items.FirstOrDefault(i => i.Name == name);

            if (item == null)
            {
                return false;
            }

            _currentWeight -= item.Weight;
            return _items.Remove(item);
        }
    }

    public List<Item> FindItems(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return [];
        }

        lock (_lock)
        {
            return _items.Where(i => i.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).Select(i => i.Clone()).ToList();
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _items.Clear();
            _currentWeight = 0;
        }
    }

    public IEnumerator<Item> GetEnumerator()
    {
        lock (_lock)
        {
            return _items.Select(i => i.Clone()).ToList().GetEnumerator();
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
