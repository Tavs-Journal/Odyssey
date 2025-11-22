using System.Collections.Generic;
using UnityEngine;

public abstract class EntityState<T> where T : Entity<T>
{
    public static EntityState<T> CreateFromString(string typeName)
    {
        return (EntityState<T>)System.Activator
            .CreateInstance(System.Type.GetType(typeName));
    }
    public static List<EntityState<T>> CreatListFromStringArray(string[] array)
    {
        var list = new List<EntityState<T>>();
        foreach(var typeName in array)
        {
            list.Add(CreateFromString(typeName));
        }  
        return list;
    }
}
