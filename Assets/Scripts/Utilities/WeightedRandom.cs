using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WeightedItem<T>
{
    public T item;
    [Min(0f)] public float weight;
    
}

public static class WeightedRandom
{
    public static T Choose<T>(List<WeightedItem<T>> items)
    {
        if (items == null || items.Count == 0)
            return default;

        float totalWeight = 0f;
        foreach (var entry in items)
        {
            totalWeight += entry.weight;
        }

        if (totalWeight <= 0f)
            return default;

        float randomValue = Random.Range(0f, totalWeight);

        foreach (var entry in items)
        {
            if (randomValue < entry.weight)
            {
                return entry.item;
            }
            randomValue -= entry.weight;
        }

        return items[items.Count - 1].item;
    }
}
