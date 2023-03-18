﻿namespace irods_Csharp;

public class CollectionManager
{
    public IrodsSession Session;
    private readonly Path _home;

    /// <summary>
    /// Collection manager constructor.
    /// </summary>
    /// <param name="session">Session which contains account and connection</param>
    /// <param name="home">Path to home directory</param>
    public CollectionManager(IrodsSession session, string home)
    {
        Session = session;
        _home = new Path(home);
    }

    /// <summary>
    /// Renames collection.
    /// </summary>
    /// <param name="source">Original name of collection</param>
    /// <param name="target">New name of collection</param>
    public void Rename(string source, string target)
    {
        Session.Rename(source, target, true);
    }

    /// <summary>
    /// Looks for collection on server.
    /// </summary>
    /// <param name="path">Path to collection parent</param>
    /// <param name="id">Id of collection, will be queried if not supplied</param>
    /// <returns>Collection object</returns>
    public Collection Open(string path, int id = -1)
    {
        if (id == -1)
        {
            id = Session.Queries.QueryCollection(Path.First(path), Path.Last(path), true)[0].Id;
        }
        return new Collection(new Path(path), id, this);
    }

    /// <summary>
    /// Creates collection.
    /// </summary>
    /// <param name="path">Path where collection should be created, including name</param>
    public void Create(string path)
    {
        KeyValPairPi mkdirRequestMsgPair = new (0, new string[0], new string[0]);
        Packet<CollInpNewPi> mkdirRequest = new (ApiNumberData.COLL_CREATE_AN)
        {
            MsgBody = new CollInpNewPi(_home + path, 0, 0, mkdirRequestMsgPair)
        };
        Session.SendPacket(mkdirRequest);

        Session.ReceivePacket<None>();
    }

    /// <summary>
    /// Removes collection.
    /// </summary>
    /// <param name="path">Path to collection parent</param>
    /// <param name="recursive">Delete items of collection if they exist</param>
    public void Remove(string path, bool recursive = true)
    {
        Packet<CollInpNewPi> rmdirRequest = new (ApiNumberData.RM_COLL_AN)
        {
            MsgBody = new CollInpNewPi(_home + path, 0, 0)
            {
                KeyValPairPi = recursive ? new KeyValPairPi(1, new[] { "recursiveOpr" }, new[] { "" }) : new KeyValPairPi(0, new string[0], new string[0])
            }
        };
        Session.SendPacket(rmdirRequest);

        Session.ReceivePacket<CollOprStatPi>();
    }
}