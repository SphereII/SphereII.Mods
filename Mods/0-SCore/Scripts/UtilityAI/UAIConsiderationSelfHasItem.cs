using System;
using System.Collections.Generic;
using UnityEngine;

namespace UAI
{
    public class UAIConsiderationSelfHasItem : UAIConsiderationBase
    {
        private string _tags;
        private ItemAction _itemAction;
        private string property;
        private string items;
        public override void Init(Dictionary<string, string> parameters)
        {
            base.Init(parameters);
            if (parameters.ContainsKey("tags"))
            {
                _tags = parameters["tags"];
            }

            if (parameters.ContainsKey("action"))
            {
                var action = parameters["action"];
                var itemAction = ReflectionHelpers.GetTypeWithPrefix("ItemAction", action);
                _itemAction = (ItemAction)Activator.CreateInstance(itemAction);
            }

            if (parameters.ContainsKey("property"))
            {
                property = parameters["property"];
            }


            if (parameters.ContainsKey("items"))
            {
                items = parameters["items"];
            }
        }

        public override float GetScore(Context _context, object target)
        {
            
            if ( !string.IsNullOrEmpty(items))
            {
                foreach( var ID in items.Split(','))
                {
                    if (ID.Contains("*"))
                    {
                        var startsWith = ID.Split('*')[0];
                        var endsWith = ID.Split('*')[1];

                        if (_context.Self.lootContainer != null)
                        {
                            foreach (var items in _context.Self.lootContainer.items)
                            {
                                var itemName = items.itemValue.ItemClass.GetItemName();
                                if (itemName.StartsWith(startsWith) && itemName.EndsWith(endsWith))
                                    return 1f;
                            }
                        }
                        return 0f;
                    }
                    var item = ItemClass.GetItem(ID);
                    if (item != null)
                    {
                        if (_context.Self.inventory.GetItemCount(item) > 0)
                            return 1f;

                        if (_context.Self.bag.GetItemCount(item) > 0)
                            return 1f;

                        if (_context.Self.lootContainer != null)
                            if (_context.Self.lootContainer.HasItem(item))
                                return 1f;
                    }
                }
                return 0f;
            }
            if (!string.IsNullOrEmpty(property))
            {
                var item = EntityUtilities.GetItemStackByProperty(_context.Self.entityId, property);
                if (item != null)
                {
                    Debug.Log("Found an item.");
                    return 1f;
                }
                Debug.Log("No Items found.");
                return 0f;
            }

            if (_itemAction != null)
            {
                var item = EntityUtilities.GetItemStackByAction(_context.Self.entityId, _itemAction.GetType());
                if (item != null)
                    return 1f;
                return 0f;
            }

            if (string.IsNullOrEmpty(_tags))
                return 0f;

            // If there's no comma, it's just one tag
            if (!_tags.Contains(","))
            {
                if (EntityUtilities.GetItemStackByTag(_context.Self.entityId, _tags) != ItemStack.Empty)
                    return 1f;
            }

            foreach (var tag in _tags.Split(','))
            {
                if (EntityUtilities.GetItemStackByTag(_context.Self.entityId, tag) != ItemStack.Empty)
                    return 1f;
            }

            return 0f;
        }
    }
}