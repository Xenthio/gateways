using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sandbox;

public class GatewayTrace
{
	public Vector3 StartPosition;
	public Vector3 EndPosition;
	public Rotation StartRotation;
	public Entity IgnoreEntity;
	public BBox BBox;
	public string[] WithoutTagsList;
	public string[] WithAnyTagsList;
	public bool UsesHitboxes = false;

	public TraceType TraceType = TraceType.Ray;
	public static GatewayTrace Ray(Vector3 StartPosition, Vector3 EndPosition)
	{
		var b = new GatewayTrace();
		b.StartPosition = StartPosition;
		b.EndPosition = EndPosition;
		b.TraceType = TraceType.Ray;
		return b;
	}
	public static GatewayTrace Box(BBox bbox, Vector3 from, Vector3 to)
	{

		var b = new GatewayTrace();
		b.StartPosition = from;
		b.EndPosition = to;
		b.TraceType = TraceType.Box;
		b.BBox = bbox;
		return b;

	}

	public GatewayTrace FromTo(Vector3 from, Vector3 to)
	{
		var b = this;
		b.StartPosition = from; 
		b.EndPosition = to;
		return b;
	}

	public GatewayTrace Size(Vector3 mins, Vector3 maxs)
	{
		var b = this;
		b.BBox = new BBox(mins, maxs);
		b.TraceType = TraceType.Box;
		return b;
	}

	public GatewayTrace WithAnyTags(params string[] tags)
	{
		var b = this;
		b.WithAnyTagsList = tags;
		return b;
	}
	public GatewayTrace WithoutTags(params string[] tags)
	{
		var b = this;
		b.WithoutTagsList = tags;
		return b;
	}
	public GatewayTrace UseHitboxes()
	{
		var b = this;
		b.UsesHitboxes = true;
		return b;
	}
	public GatewayTrace Ignore(Entity ent)
	{
		var b = this;
		b.IgnoreEntity = ent;
		return b;
	}
	public GatewayTraceResult Run()
	{
		var b = new GatewayTraceResult();
		switch (this.TraceType)
		{
			case TraceType.Ray:
				b = TraceRayRun();
				break;
			case TraceType.Box:
				b = TraceBoxRun();
				break;
		} 
		return b;
	}

	GatewayTraceResult TraceRayRun()
	{

		var b = new GatewayTraceResult();
		b.StartPosition = StartPosition;
		b.StartRotation = StartRotation;
		b.EndRotation = StartRotation;

		bool isDone = false;
		var pos1 = StartPosition;
		var pos2 = EndPosition;
		Entity ignore = null;
		var depth = 0;
		var dist = 0f;
		//trace and run another trace if we hit a portal
		while (!isDone)
		{
			depth++;
			
			isDone = true;
			var pretr = Trace.Ray(pos1, pos2).Ignore(ignore).Ignore(IgnoreEntity);
			if (WithoutTagsList != null)
			{
				pretr = pretr.WithoutTags(WithoutTagsList);
			}

			if (WithAnyTagsList != null)
			{
				pretr = pretr.WithAnyTags(WithAnyTagsList);
			}
			if (UsesHitboxes)
			{
				pretr = pretr.UseHitboxes();
			}
			var tr = pretr.Run();
			dist += tr.Distance;
			b.Results.Add(tr);
			//DebugOverlay.Line(pos1, pos2, 5);
			pos1 = tr.StartPosition;
			pos2 = tr.EndPosition; 
			if (tr.Entity is GatewayEntity gateway)
			{
				Log.Info("hit gateway");
				if (gateway.TargetGatewayEntity == null)
				{
					Log.Info("gateway we hit has no output");
					break;
				} 
				ignore = gateway.TargetGatewayEntity;

				var localp1 = gateway.Transform.PointToLocal(tr.HitPosition);

				//localp1 = Vector3.Reflect(localp1, gateway.Rotation.Forward);
				localp1 = Vector3.Reflect(localp1, gateway.TargetGatewayEntity.Rotation.Right);
				localp1 = Vector3.Reflect(localp1, gateway.TargetGatewayEntity.Rotation.Forward);

				pos1 = gateway.TargetGatewayEntity.Transform.PointToWorld(localp1);
				var localp2 = gateway.Transform.PointToLocal(EndPosition);

				localp2 = Vector3.Reflect(localp2, gateway.TargetGatewayEntity.Rotation.Right);
				localp2 = Vector3.Reflect(localp2, gateway.TargetGatewayEntity.Rotation.Forward);

				pos2 = gateway.TargetGatewayEntity.Transform.PointToWorld(localp2);


				var Forward = StartRotation.Forward + gateway.Position;

				Forward = gateway.Transform.PointToLocal(Forward);

				Forward = Vector3.Reflect(Forward, gateway.TargetGatewayEntity.Rotation.Right);
				Forward = Vector3.Reflect(Forward, gateway.TargetGatewayEntity.Rotation.Forward);

				Forward = gateway.TargetGatewayEntity.Transform.PointToWorld(Forward) - gateway.TargetGatewayEntity.Position;

				var Up = StartRotation.Up + gateway.Position;

				Up = gateway.Transform.PointToLocal(Up);

				Up = Vector3.Reflect(Up, gateway.TargetGatewayEntity.Rotation.Right);
				Up = Vector3.Reflect(Up, gateway.TargetGatewayEntity.Rotation.Forward);

				Up = gateway.TargetGatewayEntity.Transform.PointToWorld(Up) - gateway.TargetGatewayEntity.Position;

				b.EndRotation = Rotation.LookAt(Forward, Up);

				if (depth < 64) isDone = false;
			}
			b.Entity = tr.Entity;
			b.Fraction = tr.Fraction;
			b.Normal = tr.Normal;
			b.Hit = tr.Hit;
			b.StartedSolid = tr.StartedSolid;
			b.Direction = tr.Direction;
			b.Bone = tr.Bone;
			b.Surface = tr.Surface;
			b.Hitbox = tr.Hitbox;
		}
		b.Depth = depth;
		b.Distance = dist;
		b.EndPosition = pos2;

		return b;
	}
	GatewayTraceResult TraceBoxRun()
	{

		var b = new GatewayTraceResult();
		b.StartPosition = StartPosition;
		b.StartRotation = StartRotation;
		b.EndRotation = StartRotation;

		bool isDone = false;
		var pos1 = StartPosition;
		var pos2 = EndPosition;
		Entity ignore = null;
		var depth = 0;
		var dist = 0f;
		//trace and run another trace if we hit a portal
		while (!isDone)
		{
			depth++;

			isDone = true;
			var pretr = Trace.Ray(pos1, pos2).Ignore(ignore).Ignore(IgnoreEntity);
			//pretr = pretr.Size(BBox);
			if (WithoutTagsList != null)
			{
				pretr = pretr.WithoutTags(WithoutTagsList);
			}

			if (WithAnyTagsList != null)
			{
				pretr = pretr.WithAnyTags(WithAnyTagsList);
			}
			if (UsesHitboxes)
			{
				pretr = pretr.UseHitboxes();
			}
			var tr = pretr.Run();
			dist += tr.Distance;
			b.Results.Add(tr);
			//DebugOverlay.Line(pos1, pos2, 5);
			pos1 = tr.StartPosition;
			pos2 = tr.EndPosition; 
			if (tr.Entity is GatewayEntity gateway)
			{
				Log.Info("hit gateway");
				if (gateway.TargetGatewayEntity == null)
				{
					Log.Info("gateway we hit has no output");
					break;
				} 
				ignore = gateway.TargetGatewayEntity;

				var localp1 = gateway.Transform.PointToLocal(tr.HitPosition);

				//localp1 += Vector3.Forward * 1.3f;
				//localp1 = Vector3.Reflect(localp1, gateway.Rotation.Forward);
				localp1 = Vector3.Reflect(localp1, gateway.TargetGatewayEntity.Rotation.Right);
				localp1 = Vector3.Reflect(localp1, gateway.TargetGatewayEntity.Rotation.Forward);

				pos1 = gateway.TargetGatewayEntity.Transform.PointToWorld(localp1);
				var localp2 = gateway.Transform.PointToLocal(EndPosition);

				localp2 = Vector3.Reflect(localp2, gateway.TargetGatewayEntity.Rotation.Right);
				localp2 = Vector3.Reflect(localp2, gateway.TargetGatewayEntity.Rotation.Forward);

				pos2 = gateway.TargetGatewayEntity.Transform.PointToWorld(localp2);


				var Forward = StartRotation.Forward + gateway.Position;

				Forward = gateway.Transform.PointToLocal(Forward);

				Forward = Vector3.Reflect(Forward, gateway.TargetGatewayEntity.Rotation.Right);
				Forward = Vector3.Reflect(Forward, gateway.TargetGatewayEntity.Rotation.Forward);

				Forward = gateway.TargetGatewayEntity.Transform.PointToWorld(Forward) - gateway.TargetGatewayEntity.Position;

				var Up = StartRotation.Up + gateway.Position;

				Up = gateway.Transform.PointToLocal(Up);

				Up = Vector3.Reflect(Up, gateway.TargetGatewayEntity.Rotation.Right);
				Up = Vector3.Reflect(Up, gateway.TargetGatewayEntity.Rotation.Forward);

				Up = gateway.TargetGatewayEntity.Transform.PointToWorld(Up) - gateway.TargetGatewayEntity.Position;

				b.EndRotation = Rotation.LookAt(Forward, Up);

				if (depth < 64) isDone = false;
			}
			b.Entity = tr.Entity;
			b.Fraction = tr.Fraction;
			b.Normal = tr.Normal;
			b.Hit = tr.Hit;
			b.StartedSolid = tr.StartedSolid;
			b.Direction = tr.Direction;
			b.EndPosition = tr.EndPosition;  
			b.Bone = tr.Bone;
			b.Surface = tr.Surface;
			b.Hitbox = tr.Hitbox;
		}
		b.Depth = depth;
		b.Distance = dist; 

		var trray = GatewayTrace.Ray(StartPosition, EndPosition).Ignore(IgnoreEntity).Run();
		if (depth > 1)
		{
			b.EndPosition = trray.EndPosition;
		} else
		{
			b.EndPosition = pos2;
		}
		b.Normal = trray.Normal;

		return b;
	}

	GatewayTraceResult TraceBoxRunOld()
	{
		//Log.Info("box trace");
		var b = new GatewayTraceResult();
		var corners = BBox.Corners.ToList();


		var newcorners = new List<Vector3>();
		var lastcorner = Vector3.Zero;
		foreach (var corner in corners)
		{

			//Log.Info("box CORNER");
			var tr = GatewayTrace.Ray(corner + StartPosition, corner + EndPosition).Ignore(IgnoreEntity).Run();

			b.Entity = tr.Entity;
			b.Fraction = tr.Fraction;
			b.Normal = tr.Normal; 
			b.Direction = tr.Direction;

			b.StartedSolid = tr.StartedSolid;
			b.Hit = tr.Hit;

			b.Results.AddRange(tr.Results);
			newcorners.Add(b.EndPosition);
			lastcorner = corner;
		}

		foreach (var trr in b.Results)
		{
			if (trr.Hit)
			{
				b.EndPosition = trr.EndPosition;
				b.Normal = trr.Normal;
			}
		}
		var trray = GatewayTrace.Ray(StartPosition, EndPosition).Ignore(IgnoreEntity).Run();
		
		b.Normal = trray.Normal;
		b.Entity = trray.Entity;
		b.EndPosition = trray.EndPosition;

		return b;
	}

	
}
public enum TraceType {
	Ray,
	Box
}
public class GatewayTraceResult
{
	public List<TraceResult> Results = new();
	public Vector3 StartPosition;
	public Vector3 EndPosition;
	public Vector3 HitPosition;

	public Rotation StartRotation;
	public Rotation EndRotation;

	public bool Hit;
	public bool StartedSolid;
	public Vector3 Direction;
	public float Distance;

	public int Depth = 0;

	public Entity Entity;

	public float Fraction;
	public Vector3 Normal;

	public Surface Surface;
	public Hitbox Hitbox;
	public int Bone;

	public TraceResult AsTraceResult()
	{
		var b = Results.Last();
		b.StartedSolid = StartedSolid;
		//b.Distance = Distance;
		b.StartPosition = StartPosition;
		b.EndPosition = EndPosition+(Normal * 0.01f);
		b.HitPosition = HitPosition+(Normal * 0.01f);
		b.Hit = Hit;
		b.Fraction = Fraction;
		b.Normal = Normal;
		b.Entity = Entity; 
		return b;
	}
}