// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
//
// // 		  <objective type="EntityAliveSDXKill, SCore" id="npcHarleyEmptyHand" value="2" phase="3"/>
//
// public class ObjectiveEntityAliveSDXKill : BaseObjective
// {
//     public string localizedName = "";
//     private int neededKillCount;
//     private string[] otherEntityAliveSDX;
//     public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
//     {
//         get
//         {
//             return BaseObjective.ObjectiveValueTypes.Number;
//         }
//     }
//
//     public override void SetupObjective()
//     {
//         this.neededKillCount = Convert.ToInt32(base.Value);
//         if (base.ID != null)
//         {
//             string[] array = base.ID.Split(new char[]
//             {
//                 ','
//             });
//             if (array.Length > 1)
//             {
//                 base.ID = array[0];
//                 this.otherEntityAliveSDX = new string[array.Length - 1];
//                 for (int i = 1; i < array.Length; i++)
//                 {
//                     this.otherEntityAliveSDX[i - 1] = array[i];
//                 }
//             }
//         }
//     }
//
//     public override void SetupDisplay()
//     {
//         this.keyword = Localization.Get("ObjectiveEntityAliveSDXKill_keyword");
//         if (this.localizedName == "")
//         {
//             this.localizedName = ((base.ID != null && base.ID != "") ? Localization.Get(base.ID) : "Any NPC");
//         }
//         base.Description = string.Format(this.keyword, this.localizedName);
//         this.StatusText = string.Format("{0}/{1}", base.CurrentValue, this.neededKillCount);
//     }
//
//     public override void AddHooks()
//     {
//         SCoreQuestEventManager.Instance.EntityAliveSDXKill += Current_EntityAliveSDX;
//     }
//
//     public override void RemoveHooks()
//     {
//         SCoreQuestEventManager.Instance.EntityAliveSDXKill -= Current_EntityAliveSDX;
//     }
//
//     public void Current_EntityAliveSDX(string name)
//     {
//         if (base.Complete)
//         {
//             return;
//         }
//         bool flag = false;
//         if (base.ID == null || name.EqualsCaseInsensitive(base.ID))
//         {
//             flag = true;
//         }
//         if (!flag && this.otherEntityAliveSDX != null)
//         {
//             for (int i = 0; i < this.otherEntityAliveSDX.Length; i++)
//             {
//                 if (this.otherEntityAliveSDX[i].EqualsCaseInsensitive(name))
//                 {
//                     flag = true;
//                     break;
//                 }
//             }
//         }
//         if (flag && base.OwnerQuest.CheckRequirements())
//         {
//             //byte currentValue = base.CurrentValue;
//             base.CurrentValue++;//= currentValue++;
//             this.Refresh();
//         }
//     }
//
//     public override void Refresh()
//     {
//         if (base.Complete)
//         {
//             return;
//         }
//         base.Complete = ((int)base.CurrentValue >= this.neededKillCount);
//         if (base.Complete)
//         {
//             base.OwnerQuest.CheckForCompletion(QuestClass.CompletionTypes.AutoComplete, null, true);
//         }
//     }
//
//     public override BaseObjective Clone()
//     {
//         var objectiveEntityAliveSDXKill = new ObjectiveEntityAliveSDXKill();
//         this.CopyValues(objectiveEntityAliveSDXKill);
//         objectiveEntityAliveSDXKill.localizedName = this.localizedName;
//         return objectiveEntityAliveSDXKill;
//     }
//
//     public override void ParseProperties(DynamicProperties properties)
//     {
//         base.ParseProperties(properties);
//         if (properties.Values.ContainsKey(ObjectiveAnimalKill.PropObjectiveKey))
//         {
//             this.localizedName = Localization.Get(properties.Values[ObjectiveAnimalKill.PropObjectiveKey]);
//         }
//     }
//
//     public override string ParseBinding(string bindingName)
//     {
//         string id = base.ID;
//         string value = base.Value;
//         if (this.localizedName == "")
//         {
//             this.localizedName = ((id != null && id != "") ? Localization.Get(id) : "Any EntityAlive");
//         }
//         if (bindingName != null)
//         {
//             if (bindingName == "target")
//             {
//                 return this.localizedName;
//             }
//             if (bindingName == "targetwithcount")
//             {
//                 return Convert.ToInt32(value).ToString() + " " + this.localizedName;
//             }
//         }
//         return "";
//     }
//
// }
//
