// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
//
// // 		  <objective type="EntityEnemySDXKill, SCore" id="npcHarleyEmptyHand" value="2" phase="3"/>
//
// public class ObjectiveEntityEnemySDXKill : ObjectiveEntityAliveSDXKill
// {
//     public override void AddHooks()
//     {
//         SCoreQuestEventManager.Instance.EntityEnemySDXKill += Current_EntityAliveSDX;
//     }
//
//     public override void RemoveHooks()
//     {
//         SCoreQuestEventManager.Instance.EntityEnemySDXKill -= Current_EntityAliveSDX;
//     }
//
//     public override BaseObjective Clone()
//     {
//         var cloneObject = new ObjectiveEntityEnemySDXKill();
//         this.CopyValues(cloneObject);
//         cloneObject.localizedName = this.localizedName;
//         return cloneObject;
//     }
// }
//
