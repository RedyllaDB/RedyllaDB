namespace RedyllaDB

open System

/// Represents a multiplexer that manages connections to a Scylla cluster.
/// Designed similarly to StackExchange.Redis's IConnectionMultiplexer interface.
type IConnectionMultiplexer =
    inherit IDisposable
    
    /// <summary>
    /// Retrieves a logical database interface to interact with Scylla.
    /// </summary>
    /// <returns>
    /// An <see cref="IDatabase" /> instance representing a Scylla database connection.
    /// </returns>
    abstract GetDatabase : unit -> IDatabase
        
    /// Gets a value indicating whether the multiplexer is currently connected to the Redis server.    
    abstract IsConnected : bool