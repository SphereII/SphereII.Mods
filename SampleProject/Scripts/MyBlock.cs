
// <property name="Class" value="MyBlock, SampleProject" />
class MyBlock : Block
{
    public override void Init()
    {
        Log.Out($"Hello from {GetType()}!");
        base.Init();
    }
}