/*
 * Class: EntityZombieCrawlSDX
 * Author:  sphereii and HAL9000
 * Category: Entity
 * Description:
 *      This mod is an extension of the base zombie class for support for walkers. 
  * *
 * Usage:
 *      Add the following class to entities that are meant to use these features.
 *
 *      <property name="Class" value="EntityZombieCrawlSDX, Mods" />
 *
 * Features:
 *
 *  All features are inherited from the base EntityZombieSDX class, with the exception of walk type.
 */
public class EntityZombieCrawlSDX : EntityZombie
{
    public int GetWalkType()
    {
        return 4;
    }

}

