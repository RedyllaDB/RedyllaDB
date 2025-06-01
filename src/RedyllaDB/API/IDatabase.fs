namespace RedyllaDB          

open System
open System.Runtime.InteropServices
open System.Threading.Tasks
open StackExchange.Redis

/// Represents an interface for interacting with a Scylla cluster.
/// Mimics key functionality provided by StackExchange.Redis.
type IDatabase =
    /// <summary>
    /// Asynchronously sets a string value in Redis for the given key.
    /// </summary>
    /// <param name="key">The key under which the value is stored.</param>
    /// <param name="value">The value to store.</param>
    /// <param name="expiry">Optional expiration timespan. Null means no expiration.</param>
    /// <param name="keepTtl">If true, retains the existing TTL instead of overwriting it.</param>
    /// <param name="flags">Command behavior flags.</param>
    /// <returns>A task that represents the asynchronous operation, returning true if the value was set.</returns>
    abstract StringSetAsync:
        key:RedisKey * value:RedisValue *
        [<Optional;DefaultParameterValue(Nullable<TimeSpan>())>] expiry:Nullable<TimeSpan> *
        [<Optional;DefaultParameterValue(false)>] keepTtl:bool *
        [<Optional;DefaultParameterValue(CommandFlags.None)>] flags:CommandFlags -> Task<bool>
    
    /// <summary>
    /// Asynchronously retrieves the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to retrieve.</param>
    /// <param name="flags">Command behavior flags.</param>
    /// <returns>A task that represents the asynchronous operation, returning the value stored at the key.</returns>
    abstract StringGetAsync:
        key:RedisKey *
        [<Optional;DefaultParameterValue(CommandFlags.None)>] flags:CommandFlags -> Task<RedisValue>
              
    /// <summary>
    /// Asynchronously retrieves the values of multiple keys.
    /// </summary>
    /// <param name="keys">The keys to retrieve.</param>
    /// <param name="flags">Command behavior flags.</param>
    /// <returns>A task that represents the asynchronous operation, returning an array of values corresponding to the specified keys.</returns>                                    
    abstract StringGetAsync:
        keys:RedisKey[] *
        [<Optional;DefaultParameterValue(CommandFlags.None)>] flags:CommandFlags -> Task<RedisValue[]>
            
    /// <summary>
    /// Asynchronously sets an expiration time on the specified key.
    /// </summary>
    /// <param name="key">The key to set expiration for.</param>
    /// <param name="expiry">The expiration time. Null removes any existing expiration.</param>
    /// <param name="flags">Command behavior flags.</param>
    /// <returns>A task that represents the asynchronous operation, returning true if the expiration was successfully set.</returns>                                    
    abstract KeyExpireAsync:
        key:RedisKey *
        [<Optional;DefaultParameterValue(Nullable<TimeSpan>())>] expiry:Nullable<TimeSpan> *
        [<Optional;DefaultParameterValue(CommandFlags.None)>] flags:CommandFlags -> Task<bool>
    
    /// <summary>
    /// Asynchronously retrieves the expiration time of a key, if any.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <param name="flags">Command behavior flags.</param>
    /// <returns>A task that represents the asynchronous operation, returning the expiration time or null if none is set.</returns>
    abstract KeyExpireTimeAsync:
        key:RedisKey *
        [<Optional;DefaultParameterValue(CommandFlags.None)>] flags:CommandFlags -> Task<Nullable<DateTime>>
        
    /// <summary>
    /// Asynchronously deletes the specified key.
    /// </summary>
    /// <param name="key">The key to delete.</param>
    /// <param name="flags">Command behavior flags.</param>
    /// <returns>A task that represents the asynchronous operation, returning true if the key was deleted.</returns>
    abstract KeyDeleteAsync:
        key:RedisKey * 
        [<Optional;DefaultParameterValue(CommandFlags.None)>] flags:CommandFlags -> Task<bool>