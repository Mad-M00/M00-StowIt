using System;
using System.Collections.Generic;
using System.Diagnostics;
using M00StowIt.Core;
using Platform;

namespace M00StowIt.Game;

// Finds storage containers around the player that a sort operation may touch:
// touched, not jammed, not quest loot, not locked against us, and not
// currently open in anyone's UI.
//
// Instead of probing every block position in the radius box (15x15x15 =
// 3,375 world lookups at the default radius), this walks the tile-entity
// lists of the few chunks overlapping the box - the game already keeps
// containers in those lists, so the cost scales with how many containers
// actually exist, not with the size of the search box.
internal sealed class ContainerScanner
{
	private readonly Func<StowItSettings> getSettings;
	private readonly ISorterLog log;

	public ContainerScanner(Func<StowItSettings> getSettings, ISorterLog log)
	{
		this.getSettings = getSettings;
		this.log = log;
	}

	public TEFeatureStorage[] FindNearby()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		GridRadius radius = getSettings().SortRadius;
		World world = GameManager.Instance.World;
		EntityPlayerLocal player = world.GetPrimaryPlayer();
		var playerPos = new Vector3i(player.position.x, player.position.y, player.position.z);
		int minX = playerPos.x - radius.X;
		int maxX = playerPos.x + radius.X;
		int minY = playerPos.y - radius.Y;
		int maxY = playerPos.y + radius.Y;
		int minZ = playerPos.z - radius.Z;
		int maxZ = playerPos.z + radius.Z;

		// Nearest-first: when several crates share a label, the closest one
		// fills up first and the overflow walks outward crate by crate.
		var hits = new List<(int DistanceSq, int Order, TEFeatureStorage Storage)>();
		for (int chunkX = minX >> 4; chunkX <= maxX >> 4; chunkX++)
		{
			for (int chunkZ = minZ >> 4; chunkZ <= maxZ >> 4; chunkZ++)
			{
				if (world.GetChunkSync(chunkX, chunkZ) is not Chunk chunk)
				{
					continue;
				}
				List<TileEntity> tileEntities = chunk.tileEntities.list;
				for (int i = 0; i < tileEntities.Count; i++)
				{
					TileEntity tileEntity = tileEntities[i];
					Vector3i pos = tileEntity.ToWorldPos();
					if (pos.x < minX || pos.x > maxX || pos.y < minY || pos.y > maxY || pos.z < minZ || pos.z > maxZ)
					{
						continue;
					}
					TEFeatureStorage storage = tileEntity.GetSelfOrFeature<TEFeatureStorage>();
					if (storage == null)
					{
						continue;
					}
					if (storage.IsUserAccessing())
					{
						log.Info("Unable to sort/restock while having a container opened");
						return Array.Empty<TEFeatureStorage>();
					}
					if (!storage.isJammed && !storage.isQuestLoot && storage.bTouched && IsAccessible(storage))
					{
						int dx = pos.x - playerPos.x;
						int dy = pos.y - playerPos.y;
						int dz = pos.z - playerPos.z;
						hits.Add((dx * dx + dy * dy + dz * dz, hits.Count, storage));
					}
				}
			}
		}
		hits.Sort((a, b) => a.DistanceSq != b.DistanceSq
			? a.DistanceSq.CompareTo(b.DistanceSq)
			: a.Order.CompareTo(b.Order));
		var containers = new TEFeatureStorage[hits.Count];
		for (int i = 0; i < hits.Count; i++)
		{
			containers[i] = hits[i].Storage;
		}
		log.Info($"Found {containers.Length} nearby suitable containers in {stopwatch.ElapsedMilliseconds} ms");
		return containers;
	}

	private static bool IsAccessible(TEFeatureStorage storage)
	{
		TEFeatureLockable lockFeature = storage.lockFeature;
		bool allowedThroughLock = lockFeature == null
			|| !lockFeature.IsLocked()
			|| lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier);
		return allowedThroughLock && !LockManager.Instance.IsLockedByLocalPlayer(storage, 0);
	}
}
