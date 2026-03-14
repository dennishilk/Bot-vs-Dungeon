using System.Collections.Generic;

public class PriorityQueue<T>
{
    private readonly List<(T item, float priority)> _items = new();

    public void Enqueue(T item, float priority)
    {
        _items.Add((item, priority));
    }

    public bool TryDequeue(out T item)
    {
        if (_items.Count == 0)
        {
            item = default;
            return false;
        }

        int bestIndex = 0;
        float bestPriority = _items[0].priority;

        for (int i = 1; i < _items.Count; i++)
        {
            if (_items[i].priority < bestPriority)
            {
                bestPriority = _items[i].priority;
                bestIndex = i;
            }
        }

        item = _items[bestIndex].item;
        _items.RemoveAt(bestIndex);
        return true;
    }
}
