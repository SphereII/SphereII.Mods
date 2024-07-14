using System;
using System.Collections.Generic;
using UnityEngine;

public class CanDamageChecks
{
    EntityAlive source;
    EntityAlive target;
   
    public void Start(string sourceEntityClass, string targetEntityClass)
    {
        // generate the entities
        source = CreateEntity(sourceEntityClass);
        target = CreateEntity(targetEntityClass);
        if (source == null || target == null)
        {
            Log.Out($"{GetType()} Failed to Start. Source: {sourceEntityClass} or Target: {targetEntityClass} could not be created.");
            return;
        }

        // Test all your conditions
        TestConditions();

        // Clean up the ghost entities.
        source.OnEntityUnload();
        target.OnEntityUnload();
    }

    private void TestConditions()
    {
        Compare(source, target);
        Compare(target, source);
    }

    private EntityAlive CreateEntity(string entityClass)
    {
        foreach (KeyValuePair<int, EntityClass> keyValuePair3 in EntityClass.list.Dict)
        {
            //if (keyValuePair3.Value.)
            {
                if (keyValuePair3.Value.entityClassName.Equals(entityClass, StringComparison.CurrentCultureIgnoreCase))
                {
                    return EntityFactory.CreateEntity(keyValuePair3.Key, new Vector3(0, 0, 0)) as EntityAlive;
                }
            }
        }

        Log.Out($"Could not create {entityClass}: Does not exist.");
        return null;
    }
 
    private void Compare(EntityAlive _source, EntityAlive _target)
    {

        Log.Out($"Testing {GetEntityID(_source)} to {GetEntityID(_target)}");
        Log.Out($"\tCan Damage? {EntityTargetingUtilities.CanDamage(_source, _target)}");
        Log.Out($"\tIs Enemy? {EntityTargetingUtilities.IsEnemy(_source, _target)}");
        Log.Out($"\tIs Friend? {EntityTargetingUtilities.IsFriend(_source, _target)}");
        Log.Out($"\tIs Ally? {EntityTargetingUtilities.IsAlly(_source, _target)}"); 

    }

    private string GetEntityID(EntityAlive entityAlive)
    {
        return $"{entityAlive.EntityName} ({entityAlive.entityId}) ";
    }

}

