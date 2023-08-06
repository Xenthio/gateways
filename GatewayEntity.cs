using Editor;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

[HammerEntity,Solid]
public partial class GatewayEntity : ModelEntity
{
	[Property]
	public EntityTarget TargetGateway { get; set; }

	[Net]
	public GatewayEntity TargetGatewayEntity { get; set; }

	public ScenePortal PortalView;
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
		SetMaterialOverride(Material.Load("materials/monitor_gateway.vmat")); 
	}
	Model MakeModel()
	{
		var m = new Mesh();

		VertexBuffer buf = new();
		buf.Init(true);

		var v1 = new Vertex(CollisionBounds.Corners.ElementAt(0));
		var v2 = new Vertex(CollisionBounds.Corners.ElementAt(3));
		var v3 = new Vertex(CollisionBounds.Corners.ElementAt(7));
		var v4 = new Vertex(CollisionBounds.Corners.ElementAt(4));
		buf.Add(v1);
		buf.Add(v2);
		buf.Add(v3);
		buf.Add(v4);

		m.CreateBuffers(buf);

		return Model.Builder.AddMesh(m).Create();
	}
	public override void ClientSpawn()
	{ 
		base.ClientSpawn(); 

	}
	
	[GameEvent.Client.Frame]
	public void FramePortal()
	{

		if (PortalView == null && TargetGatewayEntity != null && Model != null)
		{
			PortalView = new ScenePortal(Game.SceneWorld, MakeModel(), Transform, false, 2560);
			PortalView.Flags.WantsFrameBufferCopy = true;
			PortalView.Flags.CastShadows = false; 
		}
		if (PortalView == null) return;
		var trn = new Transform(Camera.Position, Camera.Rotation);
		var trnlocal = Transform.ToLocal(trn);

		var targettrn = TargetGatewayEntity.Transform;
		targettrn.Rotation = Rotation.LookAt(targettrn.Rotation.Backward, targettrn.Rotation.Up);

		var worltrn = targettrn.ToWorld(trnlocal);
		PortalView.ViewPosition = worltrn.Position;
		PortalView.ViewRotation = worltrn.Rotation;
		PortalView.FieldOfView = Camera.FieldOfView;
		PortalView.RenderingEnabled = true;
		PortalView.RenderShadows = true;
		PortalView.Aspect = Screen.Width / Screen.Height;

	}

}
