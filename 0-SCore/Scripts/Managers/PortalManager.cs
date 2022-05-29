using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PortalItem
{
	public Vector3i Position;
	public string Source;
	public string Destination;

	public PortalItem(Vector3i position, string signData)
    {
		Position = position;
		Source = signData;
		Destination = signData;
		foreach (var config in signData.Split(','))
        {
			if ( config.StartsWith("source="))
				Source = config.Split('=')[1];
			if (config.StartsWith("destination="))
				Destination = config.Split('=')[1];
		}

	}
}
public class PortalManager
{
	private const float SAVE_TIME_SEC = 60f;
	private const string saveFile = "Portals.dat";
	private static byte Version = 1;
	private float saveTime = 60f;
	private ThreadManager.ThreadInfo dataSaveThreadInfo;
	
	private Dictionary<Vector3i, string> PortalMap = new Dictionary<Vector3i, string>();
	private static PortalManager instance = null;


    public static PortalManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new PortalManager();
                instance.Init();
            }
            return instance;
        }
    }
    public void Init()
    {
		Log.Out("Starting Portal Manager...");
		ModEvents.GameStartDone.RegisterHandler(new Action(Load));
		Load();
    }
   	public void Display()
    {
		Log.Out("Portal Mapping:");
		foreach( var temp in PortalMap )
        {
			Log.Out($"{temp.Key} : {temp.Value}");
        }
    }

	public int CountLocations( string source )
    {
		var counter = 0;
		foreach (var temp in PortalMap)
		{
			var item = new PortalItem(temp.Key, temp.Value);
			if (item.Source == source)
				counter++;
		}
		return counter;
	}
	public void AddPosition( Vector3i position )
    {
		// If this sign isn't registered, try to register it now.
		var tileEntity = GameManager.Instance.World.GetTileEntity(0, position) as TileEntityPoweredPortal;
		var text = "";
		if (tileEntity != null)
			text = tileEntity.GetText();
		
		var tileEntitySign = GameManager.Instance.World.GetTileEntity(0, position) as TileEntitySign;
		if ( tileEntitySign != null)
			text = tileEntitySign.GetText();

		if (string.IsNullOrEmpty(text)) return;
		PortalManager.Instance.AddPosition(position, text);

	}
	public void AddPosition(Vector3i position, string name)
    {
		if (string.IsNullOrEmpty(name)) return;

		if ( CountLocations(name) == 2)
        {
			// Location already exists.
			return;
        }
		if (PortalMap.ContainsKey(position))
		{
			// Already in there, so no need to re-add.
			if (PortalMap[position].Equals(name)) return;
			PortalMap[position] = name;
		}
		else
			PortalMap.Add(position, name);
		Save();
    }

	public bool IsLinked( Vector3i source)
    {
		if (GetDestination(source) == Vector3i.zero) return false;
		return true;

    }

	public string GetDestinationName( Vector3i source )
    {
		var destination = GetDestination(source);
		if (destination == Vector3i.zero) return Localization.Get("portal_not_configured");
		if (PortalMap.TryGetValue(destination, out string destinationName))
        {
			var item = new PortalItem(destination, destinationName);
			return item.Destination;
			//return destinationName;
		}
			
		return Localization.Get("portal_not_configured");
    }
	public Vector3i GetDestination( Vector3i source )
    {
		if ( PortalMap.ContainsKey(source))
        {
			var sourceName = PortalMap[source];
			var item = new PortalItem(source,  sourceName);
			if (item.Destination == "NA") return Vector3i.zero;

			// Loop around every teleport position, matchng up the name.
			foreach ( var portal in PortalMap)
            {
				var portalItem = new PortalItem(portal.Key, portal.Value);
				if ( item.Destination == portalItem.Source )
				//if (portal.Value.Equals(sourceName))
                {
					// don't teleport to the same location.
					if (source == portal.Key) continue;
					return portal.Key;
                }
            }
        }
		return Vector3i.zero;
    }

	public Vector3i GetDestination(string location)
	{
		var destinationItem = new PortalItem(Vector3i.zero, location);
		var destination = Vector3i.zero;
		foreach (var portal in PortalMap)
		{
			var portalItem = new PortalItem(portal.Key, portal.Value);
			if (location == portalItem.Destination)
			{
				destination = portal.Key;
				break;
			}
			if (destinationItem.Destination == portalItem.Destination)
			{
				destination = portal.Key;
				break;
			}

		}

		if (destination == Vector3i.zero) return destination;

		// If the portal needs power, make sure its on.
		var tileEntity = GameManager.Instance.World.GetTileEntity(0, destination) as TileEntityPoweredPortal;
		if (tileEntity == null) return Vector3i.zero; // Dead portal
		if (tileEntity.RequiredPower <= 0 ) return destination;
		if (tileEntity.IsPowered) return destination;
		return Vector3i.zero;

	}
	public void RemovePosition( Vector3i position)
    {
		PortalMap.Remove(position);
		Save();
    }
    public void Write(BinaryWriter _bw)
    {
        _bw.Write(PortalManager.Version);
		var writeOut = "";
		foreach( var temp in PortalMap)
			writeOut = $"{temp.Key}:{temp.Value};";

		Log.Out($"Write: {writeOut}");
		writeOut = writeOut.TrimEnd(';');
		_bw.Write(writeOut);
    }

    public void Read(BinaryReader _br)
    {
        _br.ReadByte();
		var positions = _br.ReadString();
		foreach (var position in positions.Split(';'))
		{
			PortalMap.Add(StringParsers.ParseVector3i(position.Split(':')[0]), position.Split(':')[1]);
		}
	}

	private int saveDataThreaded(ThreadManager.ThreadInfo _threadInfo)
	{
		PooledExpandableMemoryStream pooledExpandableMemoryStream = (PooledExpandableMemoryStream)_threadInfo.parameter;
		string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), saveFile);
		if (!Directory.Exists(GameIO.GetSaveGameDir()))
		{
			return -1;
		}
		if (File.Exists(text))
		{
			File.Copy(text, string.Format("{0}/{1}", GameIO.GetSaveGameDir(), $"{saveFile}.bak"), true);
		}
		pooledExpandableMemoryStream.Position = 0L;
		StreamUtils.WriteStreamToFile(pooledExpandableMemoryStream, text);
		MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
		return -1;
	}

	public void Save()
	{
		if (this.dataSaveThreadInfo == null || !ThreadManager.ActiveThreads.ContainsKey("silent_PortalDataSave"))
		{
			PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true);
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				this.Write(pooledBinaryWriter);
			}
			this.dataSaveThreadInfo = ThreadManager.StartThread("silent_PortalDataSave", null, new ThreadManager.ThreadFunctionLoopDelegate(this.saveDataThreaded), null, System.Threading.ThreadPriority.Normal, pooledExpandableMemoryStream, null, false);
		}
	}

	public void Load()
	{
		Log.Out("Reading Portal Data...");
		PortalMap.Clear();
		string path = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), saveFile);
		if (Directory.Exists(GameIO.GetSaveGameDir()) && File.Exists(path))
		{
			try
			{
				using (FileStream fileStream = File.OpenRead(path))
				{
					using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
					{
						pooledBinaryReader.SetBaseStream(fileStream);
						this.Read(pooledBinaryReader);
					}
				}
			}
			catch (Exception)
			{
				path = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), $"{saveFile}.bak");
				if (File.Exists(path))
				{
					using (FileStream fileStream2 = File.OpenRead(path))
					{
						using (PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(false))
						{
							pooledBinaryReader2.SetBaseStream(fileStream2);
							this.Read(pooledBinaryReader2);
						}
					}
				}
			}
		}
	}
}

