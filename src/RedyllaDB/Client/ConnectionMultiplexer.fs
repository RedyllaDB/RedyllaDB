namespace RedyllaDB

open System.Runtime.InteropServices
open Cassandra
open RedyllaDB.Client.RedyllaDbClient
open RedyllaDB.Storage
open RedyllaDB.Storage.ScyllaQueries

type ConnectionMultiplexer internal (cluster: Cluster, session: ISession, queries: ScyllaQueries) =
    
    static member Connect(hosts: string, keyspace: string,
                          [<Optional;DefaultParameterValue(CompressionType.LZ4)>] compression: CompressionType) =
        let cluster =
            Cluster
                .Builder()
                .AddContactPoints(hosts)
                .WithCompression(compression)
                .Build()
                
        let session = cluster.Connect()
        
        let query = $"""
                    CREATE KEYSPACE IF NOT EXISTS {keyspace}
                    WITH replication = {{
                        'class': 'NetworkTopologyStrategy',
                        'replication_factor': 3
                    }}
                    AND tablets = {{
                        'enabled': false
                    }}
                    AND durable_writes = true;""";
        
        query |> session.Execute |> ignore
        keyspace |> session.ChangeKeyspace
        
        let queries = ScyllaQueries.init session
        new ConnectionMultiplexer(cluster, session, queries)
    
    member this.GetDatabase() =
        RedyllaDbClient(session, queries) :> IDatabase
        
    interface IConnectionMultiplexer with
        member this.Dispose() = session.Dispose()
        member this.GetDatabase() = this.GetDatabase()        
        member this.IsConnected = cluster.Metadata.AllHosts() |> Seq.exists(_.IsUp)        