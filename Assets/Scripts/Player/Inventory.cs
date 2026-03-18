using System;
using System.Collections.Generic;

public class Inventory {
    private readonly List<ItemDefinition> items = new();
    public event Action<ItemDefinition> OnItemAdded;
    public event Action OnInventoryClear;
    
    public Inventory() {
        this.items = new List<ItemDefinition>();
    }

    public void AddItem(ItemDefinition item) {
        items.Add(item);
        OnItemAdded?.Invoke(item);
    }

    public IReadOnlyList<ItemDefinition> Items => items;

    public void Reset() {
        OnInventoryClear?.Invoke();
        items.Clear();
    }
}
