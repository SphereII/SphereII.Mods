using System;
using System.Collections.Generic;
using UnityEngine;

namespace UAI
{
    public class UAIConsiderationSelfHasItemV4 : UAIConsiderationBase
    {
        private string[] _tagList  = System.Array.Empty<string>();
        private string[] _itemIds  = System.Array.Empty<string>();
        private ItemAction _itemAction;
        private string _property;

        public override void Init(Dictionary<string, string> parameters)
        {
            base.Init(parameters);

            if (parameters.ContainsKey("tags"))
                _tagList = parameters["tags"].Split(',');

            if (parameters.ContainsKey("action"))
            {
                var itemAction = ReflectionHelpers.GetTypeWithPrefix("ItemAction", parameters["action"]);
                _itemAction = (ItemAction) Activator.CreateInstance(itemAction);
            }

            if (parameters.ContainsKey("property"))
                _property = parameters["property"];

            if (parameters.ContainsKey("items"))
                _itemIds = parameters["items"].Split(',');
        }

        public override float GetScore(Context _context, object target)
        {
            if (_itemIds.Length > 0)
            {
                foreach (var id in _itemIds)
                {
                    if (id.Contains("*"))
                    {
                        var parts      = id.Split('*');
                        var startsWith = parts[0];
                        var endsWith   = parts[1];

                        if (_context.Self.lootContainer != null)
                        {
                            foreach (var slot in _context.Self.lootContainer.items)
                            {
                                var itemName = slot.itemValue.ItemClass.GetItemName();
                                if (itemName.StartsWith(startsWith) && itemName.EndsWith(endsWith))
                                    return 1f;
                            }
                        }
                        return 0f;
                    }

                    var item = ItemClass.GetItem(id);
                    if (item != null)
                    {
                        if (_context.Self.inventory.GetItemCount(item) > 0) return 1f;
                        if (_context.Self.bag.GetItemCount(item) > 0)       return 1f;
                        if (_context.Self.lootContainer != null &&
                            _context.Self.lootContainer.HasItem(item))       return 1f;
                    }
                }
                return 0f;
            }

            if (!string.IsNullOrEmpty(_property))
            {
                return EntityUtilities.GetItemStackByProperty(_context.Self.entityId, _property) != null
                    ? 1f : 0f;
            }

            if (_itemAction != null)
            {
                return EntityUtilities.GetItemStackByAction(_context.Self.entityId, _itemAction.GetType()) != null
                    ? 1f : 0f;
            }

            foreach (var tag in _tagList)
            {
                if (EntityUtilities.GetItemStackByTag(_context.Self.entityId, tag) != ItemStack.Empty)
                    return 1f;
            }

            return 0f;
        }
    }
}