/*
 * Class: EntityPlayerSDX
 * Author:  sphereii 
 * Category: Entity
 * Description:
 *      This mod is an extension of the base Player Class. It must be incuded with the EntityPlayerSDXLocal class, as the code explicitly looks for this class name + "Local".
 *
 *      Currently this class is just a stub class.
 */
using System;
using System.IO;

class EntityPlayerSDX : EntityPlayer
{
    public override void Init(int _entityClass)
    {
        base.Init(_entityClass);
    }
}
