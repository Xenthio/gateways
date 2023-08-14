
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Editor;
using System.Drawing;


[HammerEntity,Solid]
public partial class GatewayEntity : ModelEntity
{
	[Property]
	public EntityTarget TargetGateway { get; set; }

	[Net]
	public GatewayEntity TargetGatewayEntity { get; set; }

	public ScenePortal PortalView;
	Model PortalModel;
	public override void Spawn()
	{
		base.Spawn();
		SetupPhysicsFromModel(PhysicsMotionType.Keyframed);
		PortalModel = MakeModel();
		var size = CollisionBounds.Size;
		size.x = 0.01f;
		var box = BBox.FromPositionAndSize(Vector3.Zero, size);
		SetupPhysicsFromOBB(PhysicsMotionType.Keyframed, box.Mins, box.Maxs);
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
		buf.Init(false);
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
	Model MakeModelExperimental()
	{
		var m = new Mesh();
		 

		VertexBuffer buf = new();
		buf.Init(true);
		buf.Clear();

		var size = CollisionBounds.Size;
		size.x = 0.01f;
		Log.Info(size); 
		buf.AddCube(Vector3.Zero, size, Rotation.Identity); 
		 
		//buf.Default.Position = Vector3.Zero;
		buf.Default.Normal = Rotation.Forward;
		m.CreateBuffers(buf);

		var model = Model.Builder;
		model.AddMesh(m);
		model.AddCollisionBox( size/2, new Vector3(size.x/2), Rotation.Identity);

		return model.Create();
	}
	public override void ClientSpawn()
	{ 
		base.ClientSpawn(); 

	}
	
	[GameEvent.Client.Frame]
	public void FramePortal()
	{
		//return;
		if (TargetGatewayEntity == null) return;
		if (Model == null) return;
		if (PortalModel == null)
		{
			PortalModel = MakeModel();
		}

		if (PortalView == null)
		{
			PortalView = new ScenePortal(Game.SceneWorld, PortalModel, Transform, true, 2560);
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
