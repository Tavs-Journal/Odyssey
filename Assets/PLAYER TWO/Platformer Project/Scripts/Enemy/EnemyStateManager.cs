using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateManager : EntityStateManager<Enemy>
{
    [ClassTypeName(typeof(EnemyState))]
    public string[] states;

    protected override List<EntityState<Enemy>> GetStateList()
    {
        return EnemyState.CreatListFromStringArray(states);
    }
}
