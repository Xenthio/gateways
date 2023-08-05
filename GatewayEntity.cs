using Editor;
using Sandbox;

[HammerEntity,Solid]
public partial class GatewayEntity : ModelEntity
{
	[Property]
	public EntityTarget TargetGateway { get; set; }

	[Net]
	public GatewayEntity TargetGatewayEntity { get; set; }
	public override void Spawn()
	{
		base.Spawn();
		SetupPhysicsFromModel(PhysicsMotionType.Keyframed);
		TargetGatewayEntity = TargetGateway.GetTarget<GatewayEntity>();
		if (TargetGatewayEntity != null )
		{
			TargetGatewayEntity.TargetGatewayEntity = this;
		}
		Tags.Add("gatewayidentifier");
		Tags.Add("solid");
	}
}
