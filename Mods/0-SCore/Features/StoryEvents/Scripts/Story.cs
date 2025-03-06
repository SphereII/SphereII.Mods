using System;
using System.Collections.Generic;
using UnityEngine;

public class Story
{
    private Guid _guid = Guid.NewGuid();
    private List<int> _players = new List<int>();
    private List<int> _npcEntityId = new List<int>();
    private string _title;
    private string _description;
    private Vector3i _position;

    public Story(string _title, string _description, Vector3i _position)
    {
        this._title = _title;
        this._description = _description;
        this._position = _position;
    }
    public void Generate(int range = 20)
    {
        CollectParticipants(range);
    }

    private void CollectParticipants(int range)
    {
        var bb = new Bounds(_position, new Vector3(range, 10, range));
        List<Entity> entityTempList = new List<Entity>();
        GameManager.Instance.World.GetEntitiesInBounds(typeof(Entity), bb, entityTempList);
        foreach (var entity in entityTempList)
        {
            switch (entity)
            {
                case EntityPlayer:
                    _players.Add(entity.entityId);
                    break;
                case EntityAliveSDX:
                    _npcEntityId.Add(entity.entityId);
                    break;
            }
        }
    }
}
