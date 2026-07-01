
// <property name="Class" value="MyBlock, SampleProject" />
class MyBlock : Block
{
    public override void Init()
    {
        Log.Out($"Hello from {GetType()}!");
        base.Init();

        // If you want the OnBlockLoaded / OnBlockUnloaded to fire, set this to true
        IsNotifyOnLoadUnload = true;
    }

    // Only fires if IsNotifyOnLoadUnload is set to true
    public override void OnBlockLoaded(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockLoaded(_world, _blockPos, _blockValue);
    }

    // Only fires if IsNotifyOnLoadUnload is set to true
    public override void OnBlockUnloaded(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockUnloaded(_world, _blockPos, _blockValue);
    }
}